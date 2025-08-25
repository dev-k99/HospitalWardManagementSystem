using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WardSystemProject.Data;
using WardSystemProject.Models;

namespace WardSystemProject.Controllers
{
    [Authorize(Roles = "Nurse,Nursing Sister")]
    public class PatientCareController : Controller
    {
        private readonly WardSystemDBContext _context;

        public PatientCareController(WardSystemDBContext context)
        {
            _context = context;
        }

        // GET: PatientCare - Dashboard
        public async Task<IActionResult> Index()
        {
            var currentUser = User.Identity.Name;
            var isNursingSister = User.IsInRole("Nursing Sister");

            // Get patients assigned to current nurse's ward
            var patients = await _context.Patients
                .Include(p => p.Ward)
                .Include(p => p.Bed)
                .Include(p => p.AssignedDoctor)
                .Where(p => p.IsActive && p.PatientStatus == "Admitted")
                .ToListAsync();

            // Get recent vital signs
            var recentVitalSigns = await _context.VitalSigns
                .Include(v => v.Patient)
                .Where(v => v.IsActive)
                .OrderByDescending(v => v.RecordDate)
                .Take(10)
                .ToListAsync();

            // Get recent medication administrations
            var recentMedications = await _context.MedicationAdministrations
                .Include(m => m.Patient)
                .Include(m => m.Medication)
                .Where(m => m.IsActive)
                .OrderByDescending(m => m.AdministrationDate)
                .Take(10)
                .ToListAsync();

            // Get recent doctor instructions
            var recentInstructions = await _context.DoctorInstructions
                .Include(d => d.Patient)
                .Include(d => d.Doctor)
                .Where(d => d.IsActive)
                .OrderByDescending(d => d.InstructionDate)
                .Take(10)
                .ToListAsync();

            ViewBag.IsNursingSister = isNursingSister;
            ViewBag.Patients = patients;
            ViewBag.RecentVitalSigns = recentVitalSigns;
            ViewBag.RecentMedications = recentMedications;
            ViewBag.RecentInstructions = recentInstructions;

            return View();
        }

        // GET: PatientCare/ManageVitalSigns
        public async Task<IActionResult> ManageVitalSigns()
        {
            var vitalSigns = await _context.VitalSigns
                .Include(v => v.Patient)
                .Include(v => v.Patient.Ward)
                .Where(v => v.IsActive)
                .OrderByDescending(v => v.RecordDate)
                .ToListAsync();
            return View(vitalSigns);
        }

        // GET: PatientCare/CreateVitalSign
        public IActionResult CreateVitalSign()
        {
            ViewBag.PatientId = new SelectList(_context.Patients.Where(p => p.IsActive && p.PatientStatus == "Admitted"), "Id", "FullName");
            return View();
        }

        // POST: PatientCare/CreateVitalSign
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVitalSign([Bind("PatientId,BloodPressure,Temperature,HeartRate,RespiratoryRate,OxygenSaturation,Notes")] VitalSign vitalSign)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    vitalSign.RecordDate = DateTime.Now;
                    vitalSign.RecordedBy = User.Identity.Name;
                    vitalSign.IsActive = true;

                    _context.Add(vitalSign);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Vital signs recorded successfully.";
                    return RedirectToAction(nameof(ManageVitalSigns));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error recording vital signs: {ex.Message}");
                }
            }

            ViewBag.PatientId = new SelectList(_context.Patients.Where(p => p.IsActive && p.PatientStatus == "Admitted"), "Id", "FullName", vitalSign.PatientId);
            return View(vitalSign);
        }

        // GET: PatientCare/EditVitalSign/5
        public async Task<IActionResult> EditVitalSign(int? id)
        {
            if (id == null) return NotFound();

            var vitalSign = await _context.VitalSigns
                .Include(v => v.Patient)
                .FirstOrDefaultAsync(v => v.Id == id);
            if (vitalSign == null) return NotFound();

            ViewBag.PatientId = new SelectList(_context.Patients.Where(p => p.IsActive && p.PatientStatus == "Admitted"), "Id", "FullName", vitalSign.PatientId);
            return View(vitalSign);
        }

        // POST: PatientCare/EditVitalSign/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVitalSign(int id, [Bind("Id,PatientId,BloodPressure,Temperature,HeartRate,RespiratoryRate,OxygenSaturation,Notes")] VitalSign vitalSign)
        {
            if (id != vitalSign.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingVitalSign = await _context.VitalSigns.FindAsync(id);
                    if (existingVitalSign == null) return NotFound();

                    existingVitalSign.BloodPressure = vitalSign.BloodPressure;
                    existingVitalSign.Temperature = vitalSign.Temperature;
                    existingVitalSign.HeartRate = vitalSign.HeartRate;
                    existingVitalSign.RespiratoryRate = vitalSign.RespiratoryRate;
                    existingVitalSign.OxygenSaturation = vitalSign.OxygenSaturation;
                    existingVitalSign.Notes = vitalSign.Notes;

                    _context.Update(existingVitalSign);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Vital signs updated successfully.";
                    return RedirectToAction(nameof(ManageVitalSigns));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VitalSignExists(vitalSign.Id)) return NotFound();
                    else throw;
                }
            }

            ViewBag.PatientId = new SelectList(_context.Patients.Where(p => p.IsActive && p.PatientStatus == "Admitted"), "Id", "FullName", vitalSign.PatientId);
            return View(vitalSign);
        }

        // GET: PatientCare/ManageMedicationAdministrations
        public async Task<IActionResult> ManageMedicationAdministrations()
        {
            var isNursingSister = User.IsInRole("Nursing Sister");
            var medicationAdministrations = await _context.MedicationAdministrations
                .Include(m => m.Patient)
                .Include(m => m.Medication)
                .Include(m => m.Patient.Ward)
                .Where(m => m.IsActive)
                .OrderByDescending(m => m.AdministrationDate)
                .ToListAsync();

            ViewBag.IsNursingSister = isNursingSister;
            return View(medicationAdministrations);
        }

        // GET: PatientCare/CreateMedicationAdministration
        public IActionResult CreateMedicationAdministration()
        {
            var isNursingSister = User.IsInRole("Nursing Sister");
            
            // Filter medications based on user role
            var medications = isNursingSister 
                ? _context.Medications.Where(m => m.IsActive) // Nursing Sister can access all medications
                : _context.Medications.Where(m => m.IsActive && m.Schedule <= 4); // Nurse can only access Schedule 1-4

            ViewBag.PatientId = new SelectList(_context.Patients.Where(p => p.IsActive && p.PatientStatus == "Admitted"), "Id", "FullName");
            ViewBag.MedicationId = new SelectList(medications, "Id", "Name");
            ViewBag.IsNursingSister = isNursingSister;
            return View();
        }

        // POST: PatientCare/CreateMedicationAdministration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMedicationAdministration([Bind("PatientId,MedicationId,Dosage,AdministrationMethod,Notes")] MedicationAdministration medAdmin)
        {
            var isNursingSister = User.IsInRole("Nursing Sister");
            
            // Check medication schedule restrictions
            var medication = await _context.Medications.FindAsync(medAdmin.MedicationId);
            if (medication != null && medication.Schedule > 4 && !isNursingSister)
            {
                ModelState.AddModelError("MedicationId", "Nurses can only administer Schedule 1-4 medications. Schedule 5+ medications require a Nursing Sister.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    medAdmin.AdministrationDate = DateTime.Now;
                    medAdmin.AdministeredBy = User.Identity.Name;
                    medAdmin.IsActive = true;

                    _context.Add(medAdmin);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Medication administration recorded successfully.";
                    return RedirectToAction(nameof(ManageMedicationAdministrations));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error recording medication administration: {ex.Message}");
                }
            }

            // Re-populate dropdowns
            var medications = isNursingSister 
                ? _context.Medications.Where(m => m.IsActive)
                : _context.Medications.Where(m => m.IsActive && m.Schedule <= 4);

            ViewBag.PatientId = new SelectList(_context.Patients.Where(p => p.IsActive && p.PatientStatus == "Admitted"), "Id", "FullName", medAdmin.PatientId);
            ViewBag.MedicationId = new SelectList(medications, "Id", "Name", medAdmin.MedicationId);
            ViewBag.IsNursingSister = isNursingSister;
            return View(medAdmin);
        }

        // GET: PatientCare/EditMedicationAdministration/5
        public async Task<IActionResult> EditMedicationAdministration(int? id)
        {
            if (id == null) return NotFound();

            var medAdmin = await _context.MedicationAdministrations
                .Include(m => m.Patient)
                .Include(m => m.Medication)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (medAdmin == null) return NotFound();

            var isNursingSister = User.IsInRole("Nursing Sister");
            var medications = isNursingSister 
                ? _context.Medications.Where(m => m.IsActive)
                : _context.Medications.Where(m => m.IsActive && m.Schedule <= 4);

            ViewBag.PatientId = new SelectList(_context.Patients.Where(p => p.IsActive && p.PatientStatus == "Admitted"), "Id", "FullName", medAdmin.PatientId);
            ViewBag.MedicationId = new SelectList(medications, "Id", "Name", medAdmin.MedicationId);
            ViewBag.IsNursingSister = isNursingSister;
            return View(medAdmin);
        }

        // POST: PatientCare/EditMedicationAdministration/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMedicationAdministration(int id, [Bind("Id,PatientId,MedicationId,Dosage,AdministrationMethod,Notes")] MedicationAdministration medAdmin)
        {
            if (id != medAdmin.Id) return NotFound();

            var isNursingSister = User.IsInRole("Nursing Sister");
            
            // Check medication schedule restrictions
            var medication = await _context.Medications.FindAsync(medAdmin.MedicationId);
            if (medication != null && medication.Schedule > 4 && !isNursingSister)
            {
                ModelState.AddModelError("MedicationId", "Nurses can only administer Schedule 1-4 medications. Schedule 5+ medications require a Nursing Sister.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingMedAdmin = await _context.MedicationAdministrations.FindAsync(id);
                    if (existingMedAdmin == null) return NotFound();

                    existingMedAdmin.PatientId = medAdmin.PatientId;
                    existingMedAdmin.MedicationId = medAdmin.MedicationId;
                    existingMedAdmin.Dosage = medAdmin.Dosage;
                    existingMedAdmin.AdministrationMethod = medAdmin.AdministrationMethod;
                    existingMedAdmin.Notes = medAdmin.Notes;

                    _context.Update(existingMedAdmin);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Medication administration updated successfully.";
                    return RedirectToAction(nameof(ManageMedicationAdministrations));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicationAdministrationExists(medAdmin.Id)) return NotFound();
                    else throw;
                }
            }

            // Re-populate dropdowns
            var medications = isNursingSister 
                ? _context.Medications.Where(m => m.IsActive)
                : _context.Medications.Where(m => m.IsActive && m.Schedule <= 4);

            ViewBag.PatientId = new SelectList(_context.Patients.Where(p => p.IsActive && p.PatientStatus == "Admitted"), "Id", "FullName", medAdmin.PatientId);
            ViewBag.MedicationId = new SelectList(medications, "Id", "Name", medAdmin.MedicationId);
            ViewBag.IsNursingSister = isNursingSister;
            return View(medAdmin);
        }

        // GET: PatientCare/ManageDoctorInstructions
        public async Task<IActionResult> ManageDoctorInstructions()
        {
            var doctorInstructions = await _context.DoctorInstructions
                .Include(d => d.Patient)
                .Include(d => d.Doctor)
                .Include(d => d.Patient.Ward)
                .Where(d => d.IsActive)
                .OrderByDescending(d => d.InstructionDate)
                .ToListAsync();
            return View(doctorInstructions);
        }

        // GET: PatientCare/ViewDoctorInstructions/5
        public async Task<IActionResult> ViewDoctorInstructions(int? patientId)
        {
            if (patientId == null) return NotFound();

            var patient = await _context.Patients
                .Include(p => p.Ward)
                .Include(p => p.AssignedDoctor)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null) return NotFound();

            var doctorInstructions = await _context.DoctorInstructions
                .Include(d => d.Doctor)
                .Where(d => d.PatientId == patientId && d.IsActive)
                .OrderByDescending(d => d.InstructionDate)
                .ToListAsync();

            ViewBag.Patient = patient;
            return View(doctorInstructions);
        }

        // GET: PatientCare/CreateDoctorInstruction
        public IActionResult CreateDoctorInstruction()
        {
            ViewBag.PatientId = new SelectList(_context.Patients.Where(p => p.IsActive && p.PatientStatus == "Admitted"), "Id", "FullName");
            ViewBag.DoctorId = new SelectList(_context.Staff.Where(s => s.Role == "Doctor" && s.IsActive), "Id", "FullName");
            return View();
        }

        // POST: PatientCare/CreateDoctorInstruction
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDoctorInstruction([Bind("PatientId,DoctorId,InstructionType,Instructions,Priority,Status")] DoctorInstruction instruction)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    instruction.InstructionDate = DateTime.Now;
                    instruction.IsActive = true;

                    _context.Add(instruction);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Doctor instruction recorded successfully.";
                    return RedirectToAction(nameof(ManageDoctorInstructions));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error recording doctor instruction: {ex.Message}");
                }
            }

            ViewBag.PatientId = new SelectList(_context.Patients.Where(p => p.IsActive && p.PatientStatus == "Admitted"), "Id", "FullName", instruction.PatientId);
            ViewBag.DoctorId = new SelectList(_context.Staff.Where(s => s.Role == "Doctor" && s.IsActive), "Id", "FullName", instruction.DoctorId);
            return View(instruction);
        }

        // GET: PatientCare/EditDoctorInstruction/5
        public async Task<IActionResult> EditDoctorInstruction(int? id)
        {
            if (id == null) return NotFound();

            var instruction = await _context.DoctorInstructions
                .Include(d => d.Patient)
                .Include(d => d.Doctor)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (instruction == null) return NotFound();

            ViewBag.PatientId = new SelectList(_context.Patients.Where(p => p.IsActive && p.PatientStatus == "Admitted"), "Id", "FullName", instruction.PatientId);
            ViewBag.DoctorId = new SelectList(_context.Staff.Where(s => s.Role == "Doctor" && s.IsActive), "Id", "FullName", instruction.DoctorId);
            return View(instruction);
        }

        // POST: PatientCare/EditDoctorInstruction/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDoctorInstruction(int id, [Bind("Id,PatientId,DoctorId,InstructionType,Instructions,Priority,Status")] DoctorInstruction instruction)
        {
            if (id != instruction.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingInstruction = await _context.DoctorInstructions.FindAsync(id);
                    if (existingInstruction == null) return NotFound();

                    existingInstruction.InstructionType = instruction.InstructionType;
                    existingInstruction.Instructions = instruction.Instructions;
                    existingInstruction.Priority = instruction.Priority;
                    existingInstruction.Status = instruction.Status;

                    _context.Update(existingInstruction);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Doctor instruction updated successfully.";
                    return RedirectToAction(nameof(ManageDoctorInstructions));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorInstructionExists(instruction.Id)) return NotFound();
                    else throw;
                }
            }

            ViewBag.PatientId = new SelectList(_context.Patients.Where(p => p.IsActive && p.PatientStatus == "Admitted"), "Id", "FullName", instruction.PatientId);
            ViewBag.DoctorId = new SelectList(_context.Staff.Where(s => s.Role == "Doctor" && s.IsActive), "Id", "FullName", instruction.DoctorId);
            return View(instruction);
        }

        // GET: PatientCare/DetailsVitalSign/5
        public async Task<IActionResult> DetailsVitalSign(int? id)
        {
            if (id == null) return NotFound();

            var vitalSign = await _context.VitalSigns
                .Include(v => v.Patient)
                .Include(v => v.Patient.Ward)
                .FirstOrDefaultAsync(v => v.Id == id);
            if (vitalSign == null) return NotFound();

            return View(vitalSign);
        }

        // GET: PatientCare/DetailsMedicationAdministration/5
        public async Task<IActionResult> DetailsMedicationAdministration(int? id)
        {
            if (id == null) return NotFound();

            var medAdmin = await _context.MedicationAdministrations
                .Include(m => m.Patient)
                .Include(m => m.Medication)
                .Include(m => m.Patient.Ward)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (medAdmin == null) return NotFound();

            return View(medAdmin);
        }

        // GET: PatientCare/DetailsDoctorInstruction/5
        public async Task<IActionResult> DetailsDoctorInstruction(int? id)
        {
            if (id == null) return NotFound();

            var instruction = await _context.DoctorInstructions
                .Include(d => d.Patient)
                .Include(d => d.Doctor)
                .Include(d => d.Patient.Ward)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (instruction == null) return NotFound();

            return View(instruction);
        }

        // GET: PatientCare/DeleteVitalSign/5
        public async Task<IActionResult> DeleteVitalSign(int? id)
        {
            if (id == null) return NotFound();

            var vitalSign = await _context.VitalSigns
                .Include(v => v.Patient)
                .FirstOrDefaultAsync(v => v.Id == id);
            if (vitalSign == null) return NotFound();

            return View(vitalSign);
        }

        // POST: PatientCare/DeleteVitalSign/5
        [HttpPost, ActionName("DeleteVitalSign")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVitalSignConfirmed(int id)
        {
            var vitalSign = await _context.VitalSigns.FindAsync(id);
            if (vitalSign != null)
            {
                vitalSign.IsActive = false;
                _context.Update(vitalSign);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ManageVitalSigns));
        }

        // GET: PatientCare/DeleteMedicationAdministration/5
        public async Task<IActionResult> DeleteMedicationAdministration(int? id)
        {
            if (id == null) return NotFound();

            var medAdmin = await _context.MedicationAdministrations
                .Include(m => m.Patient)
                .Include(m => m.Medication)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (medAdmin == null) return NotFound();

            return View(medAdmin);
        }

        // POST: PatientCare/DeleteMedicationAdministration/5
        [HttpPost, ActionName("DeleteMedicationAdministration")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedicationAdministrationConfirmed(int id)
        {
            var medAdmin = await _context.MedicationAdministrations.FindAsync(id);
            if (medAdmin != null)
            {
                medAdmin.IsActive = false;
                _context.Update(medAdmin);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ManageMedicationAdministrations));
        }

        // GET: PatientCare/DeleteDoctorInstruction/5
        public async Task<IActionResult> DeleteDoctorInstruction(int? id)
        {
            if (id == null) return NotFound();

            var instruction = await _context.DoctorInstructions
                .Include(d => d.Patient)
                .Include(d => d.Doctor)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (instruction == null) return NotFound();

            return View(instruction);
        }

        // POST: PatientCare/DeleteDoctorInstruction/5
        [HttpPost, ActionName("DeleteDoctorInstruction")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDoctorInstructionConfirmed(int id)
        {
            var instruction = await _context.DoctorInstructions.FindAsync(id);
            if (instruction != null)
            {
                instruction.IsActive = false;
                _context.Update(instruction);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ManageDoctorInstructions));
        }

        private bool VitalSignExists(int id)
        {
            return _context.VitalSigns.Any(e => e.Id == id);
        }

        private bool MedicationAdministrationExists(int id)
        {
            return _context.MedicationAdministrations.Any(e => e.Id == id);
        }

        private bool DoctorInstructionExists(int id)
        {
            return _context.DoctorInstructions.Any(e => e.Id == id);
        }
    }
}