using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WardSystemProject.Core.Interfaces;
using WardSystemProject.Data;
using WardSystemProject.Models;
using WardSystemProject.ViewModels;

namespace WardSystemProject.Controllers
{
    [Authorize(Roles = "Nurse,Nursing Sister")]
    public class PatientCareController : Controller
    {
        private readonly IVitalSignService _vitalSignService;
        private readonly WardSystemDBContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PatientCareController(IVitalSignService vitalSignService, WardSystemDBContext context, UserManager<IdentityUser> userManager)
        {
            _vitalSignService = vitalSignService;
            _context          = context;
            _userManager      = userManager;
        }

        // ── Dashboard ─────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var isNursingSister = User.IsInRole("Nursing Sister");

            // Resolve the nurse's ward assignment.
            // Primary: IdentityUserId match. Fallback: email match for older records.
            int? nurseWardId = null;
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser != null)
            {
                var staff = await _context.Staff
                                .FirstOrDefaultAsync(s => s.IdentityUserId == identityUser.Id && s.IsActive)
                            ?? await _context.Staff
                                .FirstOrDefaultAsync(s => s.Email == identityUser.Email && s.IsActive);
                nurseWardId = staff?.WardId;
            }

            // Ward-scoped patient list: show only assigned ward if WardId is set;
            // null WardId means no restriction (unassigned nurses see all patients).
            var patientsQuery = _context.Patients
                .Include(p => p.Ward)
                .Include(p => p.Bed)
                .Include(p => p.AssignedDoctor)
                .Where(p => p.PatientStatus == "Admitted");

            if (nurseWardId.HasValue)
                patientsQuery = patientsQuery.Where(p => p.WardId == nurseWardId.Value);

            var patients = await patientsQuery.ToListAsync();

            ViewBag.IsNursingSister = isNursingSister;
            ViewBag.Patients        = patients;
            ViewBag.RecentVitalSigns = await _context.VitalSigns
                .Include(v => v.Patient)
                .OrderByDescending(v => v.RecordDate).Take(10).ToListAsync();
            ViewBag.RecentMedications = await _context.MedicationAdministrations
                .Include(m => m.Patient).Include(m => m.Medication)
                .OrderByDescending(m => m.AdministrationDate).Take(10).ToListAsync();
            ViewBag.RecentInstructions = await _context.DoctorInstructions
                .Include(d => d.Patient).Include(d => d.Doctor)
                .OrderByDescending(d => d.InstructionDate).Take(10).ToListAsync();

            return View();
        }

        // ── Vital Signs ───────────────────────────────────────────────────────

        public async Task<IActionResult> ManageVitalSigns()
        {
            var vs = await _vitalSignService.GetAllAsync();
            return View(vs);
        }

        public IActionResult CreateVitalSign()
        {
            ViewBag.PatientId = AdmittedPatientsSelectList();
            return View(new RecordVitalSignViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVitalSign(RecordVitalSignViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.PatientId = AdmittedPatientsSelectList(vm.PatientId);
                return View(vm);
            }

            await _vitalSignService.RecordAsync(vm, User.Identity?.Name ?? "Unknown");
            TempData["SuccessMessage"] = "Vital signs recorded successfully.";
            return RedirectToAction(nameof(ManageVitalSigns));
        }

        public async Task<IActionResult> EditVitalSign(int? id)
        {
            if (id == null) return NotFound();
            var vs = await _vitalSignService.GetByIdAsync(id.Value);
            if (vs == null) return NotFound();

            var vm = new RecordVitalSignViewModel
            {
                Id               = vs.Id,
                PatientId        = vs.PatientId,
                Temperature      = vs.Temperature,
                Pulse            = vs.Pulse,
                BloodPressure    = vs.BloodPressure,
                HeartRate        = vs.HeartRate,
                RespiratoryRate  = vs.RespiratoryRate,
                OxygenSaturation = vs.OxygenSaturation,
                Notes            = vs.Notes
            };
            ViewBag.PatientId = AdmittedPatientsSelectList(vs.PatientId);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVitalSign(int id, RecordVitalSignViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.PatientId = AdmittedPatientsSelectList(vm.PatientId);
                return View(vm);
            }

            await _vitalSignService.UpdateAsync(id, vm);
            TempData["SuccessMessage"] = "Vital signs updated.";
            return RedirectToAction(nameof(ManageVitalSigns));
        }

        public async Task<IActionResult> DetailsVitalSign(int? id)
        {
            if (id == null) return NotFound();
            var vs = await _vitalSignService.GetByIdAsync(id.Value);
            return vs == null ? NotFound() : View(vs);
        }

        public async Task<IActionResult> DeleteVitalSign(int? id)
        {
            if (id == null) return NotFound();
            var vs = await _vitalSignService.GetByIdAsync(id.Value);
            return vs == null ? NotFound() : View(vs);
        }

        [HttpPost, ActionName("DeleteVitalSign"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVitalSignConfirmed(int id)
        {
            await _vitalSignService.SoftDeleteAsync(id);
            return RedirectToAction(nameof(ManageVitalSigns));
        }

        // ── Medication Administration ─────────────────────────────────────────

        public async Task<IActionResult> ManageMedicationAdministrations()
        {
            var records = await _vitalSignService.GetAllAdministrationsAsync();
            ViewBag.IsNursingSister = User.IsInRole("Nursing Sister");
            return View(records);
        }

        public IActionResult CreateMedicationAdministration()
        {
            var isNursingSister = User.IsInRole("Nursing Sister");
            ViewBag.PatientId    = AdmittedPatientsSelectList();
            ViewBag.MedicationId = MedicationsSelectList(isNursingSister);
            ViewBag.IsNursingSister = isNursingSister;
            return View(new AdministerMedicationViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMedicationAdministration(AdministerMedicationViewModel vm)
        {
            var isNursingSister = User.IsInRole("Nursing Sister");

            if (!ModelState.IsValid)
            {
                ViewBag.PatientId    = AdmittedPatientsSelectList(vm.PatientId);
                ViewBag.MedicationId = MedicationsSelectList(isNursingSister, vm.MedicationId);
                ViewBag.IsNursingSister = isNursingSister;
                return View(vm);
            }

            try
            {
                await _vitalSignService.AdministerAsync(vm, User.Identity?.Name ?? "Unknown", isNursingSister);
                TempData["SuccessMessage"] = "Medication administration recorded.";
                return RedirectToAction(nameof(ManageMedicationAdministrations));
            }
            catch (UnauthorizedAccessException ex)
            {
                // Schedule restriction violation — show as a form-level validation error
                ModelState.AddModelError("MedicationId", ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            ViewBag.PatientId    = AdmittedPatientsSelectList(vm.PatientId);
            ViewBag.MedicationId = MedicationsSelectList(isNursingSister, vm.MedicationId);
            ViewBag.IsNursingSister = isNursingSister;
            return View(vm);
        }

        public async Task<IActionResult> EditMedicationAdministration(int? id)
        {
            if (id == null) return NotFound();
            var record = await _vitalSignService.GetAdministrationByIdAsync(id.Value);
            if (record == null) return NotFound();

            var isNursingSister = User.IsInRole("Nursing Sister");
            var vm = new AdministerMedicationViewModel
            {
                Id                   = record.Id,
                PatientId            = record.PatientId,
                MedicationId         = record.MedicationId,
                Dosage               = record.Dosage,
                AdministrationMethod = record.AdministrationMethod,
                Notes                = record.Notes
            };
            ViewBag.PatientId    = AdmittedPatientsSelectList(record.PatientId);
            ViewBag.MedicationId = MedicationsSelectList(isNursingSister, record.MedicationId);
            ViewBag.IsNursingSister = isNursingSister;
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMedicationAdministration(int id, AdministerMedicationViewModel vm)
        {
            var isNursingSister = User.IsInRole("Nursing Sister");
            if (!ModelState.IsValid)
            {
                ViewBag.PatientId    = AdmittedPatientsSelectList(vm.PatientId);
                ViewBag.MedicationId = MedicationsSelectList(isNursingSister, vm.MedicationId);
                return View(vm);
            }

            try
            {
                await _vitalSignService.UpdateAdministrationAsync(id, vm, isNursingSister);
                TempData["SuccessMessage"] = "Medication administration updated.";
                return RedirectToAction(nameof(ManageMedicationAdministrations));
            }
            catch (UnauthorizedAccessException ex)
            {
                ModelState.AddModelError("MedicationId", ex.Message);
            }

            ViewBag.PatientId    = AdmittedPatientsSelectList(vm.PatientId);
            ViewBag.MedicationId = MedicationsSelectList(isNursingSister, vm.MedicationId);
            return View(vm);
        }

        public async Task<IActionResult> DetailsMedicationAdministration(int? id)
        {
            if (id == null) return NotFound();
            var record = await _vitalSignService.GetAdministrationByIdAsync(id.Value);
            return record == null ? NotFound() : View(record);
        }

        public async Task<IActionResult> DeleteMedicationAdministration(int? id)
        {
            if (id == null) return NotFound();
            var record = await _vitalSignService.GetAdministrationByIdAsync(id.Value);
            return record == null ? NotFound() : View(record);
        }

        [HttpPost, ActionName("DeleteMedicationAdministration"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedicationAdministrationConfirmed(int id)
        {
            await _vitalSignService.SoftDeleteAdministrationAsync(id);
            return RedirectToAction(nameof(ManageMedicationAdministrations));
        }

        // ── Doctor Instructions ───────────────────────────────────────────────

        public async Task<IActionResult> ManageDoctorInstructions()
        {
            var instructions = await _context.DoctorInstructions
                .Include(d => d.Patient).ThenInclude(p => p.Ward)
                .Include(d => d.Doctor)
                .OrderByDescending(d => d.InstructionDate)
                .ToListAsync();
            return View(instructions);
        }

        public async Task<IActionResult> ViewDoctorInstructions(int? patientId)
        {
            if (patientId == null) return NotFound();
            var patient = await _context.Patients.Include(p => p.Ward).Include(p => p.AssignedDoctor)
                .FirstOrDefaultAsync(p => p.Id == patientId);
            if (patient == null) return NotFound();

            var instructions = await _context.DoctorInstructions
                .Include(d => d.Doctor)
                .Where(d => d.PatientId == patientId)
                .OrderByDescending(d => d.InstructionDate)
                .ToListAsync();

            ViewBag.Patient = patient;
            return View(instructions);
        }

        public IActionResult CreateDoctorInstruction()
        {
            ViewBag.PatientId = AdmittedPatientsSelectList();
            ViewBag.DoctorId  = new SelectList(_context.Staff.Where(s => s.Role == "Doctor" && s.IsActive), "Id", "FullName");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDoctorInstruction(
            [Bind("PatientId,DoctorId,InstructionType,Instructions,Priority,Status")] DoctorInstruction instruction)
        {
            if (ModelState.IsValid)
            {
                instruction.InstructionDate = DateTime.Now;
                instruction.IsActive        = true;
                _context.Add(instruction);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Doctor instruction recorded.";
                return RedirectToAction(nameof(ManageDoctorInstructions));
            }

            ViewBag.PatientId = AdmittedPatientsSelectList(instruction.PatientId);
            ViewBag.DoctorId  = new SelectList(_context.Staff.Where(s => s.Role == "Doctor" && s.IsActive), "Id", "FullName", instruction.DoctorId);
            return View(instruction);
        }

        public async Task<IActionResult> EditDoctorInstruction(int? id)
        {
            if (id == null) return NotFound();
            var instruction = await _context.DoctorInstructions
                .Include(d => d.Patient).Include(d => d.Doctor).FirstOrDefaultAsync(d => d.Id == id);
            if (instruction == null) return NotFound();

            ViewBag.PatientId = AdmittedPatientsSelectList(instruction.PatientId);
            ViewBag.DoctorId  = new SelectList(_context.Staff.Where(s => s.Role == "Doctor" && s.IsActive), "Id", "FullName", instruction.DoctorId);
            return View(instruction);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDoctorInstruction(int id,
            [Bind("Id,PatientId,DoctorId,InstructionType,Instructions,Priority,Status")] DoctorInstruction instruction)
        {
            if (id != instruction.Id) return NotFound();
            if (ModelState.IsValid)
            {
                var existing = await _context.DoctorInstructions.FindAsync(id);
                if (existing == null) return NotFound();
                existing.InstructionType = instruction.InstructionType;
                existing.Instructions    = instruction.Instructions;
                existing.Priority        = instruction.Priority;
                existing.Status          = instruction.Status;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Instruction updated.";
                return RedirectToAction(nameof(ManageDoctorInstructions));
            }

            ViewBag.PatientId = AdmittedPatientsSelectList(instruction.PatientId);
            ViewBag.DoctorId  = new SelectList(_context.Staff.Where(s => s.Role == "Doctor" && s.IsActive), "Id", "FullName", instruction.DoctorId);
            return View(instruction);
        }

        public async Task<IActionResult> DetailsDoctorInstruction(int? id)
        {
            if (id == null) return NotFound();
            var instruction = await _context.DoctorInstructions
                .Include(d => d.Patient).ThenInclude(p => p.Ward)
                .Include(d => d.Doctor)
                .FirstOrDefaultAsync(d => d.Id == id);
            return instruction == null ? NotFound() : View(instruction);
        }

        public async Task<IActionResult> DeleteDoctorInstruction(int? id)
        {
            if (id == null) return NotFound();
            var instruction = await _context.DoctorInstructions
                .Include(d => d.Patient).Include(d => d.Doctor).FirstOrDefaultAsync(d => d.Id == id);
            return instruction == null ? NotFound() : View(instruction);
        }

        [HttpPost, ActionName("DeleteDoctorInstruction"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDoctorInstructionConfirmed(int id)
        {
            var instruction = await _context.DoctorInstructions.FindAsync(id);
            if (instruction != null) { instruction.IsActive = false; await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(ManageDoctorInstructions));
        }

        // ── Select list helpers ───────────────────────────────────────────────

        private SelectList AdmittedPatientsSelectList(int selectedId = 0) =>
            new(_context.Patients.Where(p => p.IsActive && p.PatientStatus == "Admitted"), "Id", "FullName", selectedId);

        private SelectList MedicationsSelectList(bool isNursingSister, int selectedId = 0)
        {
            var query = isNursingSister
                ? _context.Medications.Where(m => m.IsActive)
                : _context.Medications.Where(m => m.IsActive && m.Schedule <= 4);
            return new SelectList(query, "Id", "Name", selectedId);
        }
    }
}
