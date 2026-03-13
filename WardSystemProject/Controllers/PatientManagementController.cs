using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WardSystemProject.Core.Interfaces;
using WardSystemProject.Data;
using WardSystemProject.Features.PatientManagement;
using WardSystemProject.Models;
using WardSystemProject.ViewModels;

namespace WardSystemProject.Controllers
{
    [Authorize(Roles = "Ward Admin")]
    public class PatientManagementController : Controller
    {
        private readonly IPatientService _patientService;
        private readonly PatientFolderPdfService _pdfService;
        private readonly WardSystemDBContext _context;  // used only for dropdown data
        private readonly ILogger<PatientManagementController> _logger;

        public PatientManagementController(
            IPatientService patientService,
            PatientFolderPdfService pdfService,
            WardSystemDBContext context,
            ILogger<PatientManagementController> logger)
        {
            _patientService = patientService;
            _pdfService     = pdfService;
            _context        = context;
            _logger         = logger;
        }

        // GET: PatientManagement
        public async Task<IActionResult> Index()
        {
            var patients = await _patientService.GetAllActiveAsync();
            return View(patients);
        }

        // GET: PatientManagement/Search
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return RedirectToAction(nameof(Index));
            var results = await _patientService.SearchAsync(query);
            return View("Index", results);
        }

        // ── Admission ─────────────────────────────────────────────────────────

        // GET: PatientManagement/Admit
        public IActionResult Admit()
        {
            LoadAdmitDropdowns();
            return View(new AdmitPatientViewModel { AdmissionDate = DateTime.UtcNow });
        }

        // POST: PatientManagement/Admit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Admit(AdmitPatientViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                LoadAdmitDropdowns(vm.WardId, vm.BedId, vm.AssignedDoctorId);
                return View(vm);
            }

            try
            {
                var patient = await _patientService.AdmitAsync(vm, User.Identity?.Name ?? "Unknown");
                TempData["SuccessMessage"] = $"Patient {patient.FullName} admitted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error admitting patient");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
            }

            LoadAdmitDropdowns(vm.WardId, vm.BedId, vm.AssignedDoctorId);
            return View(vm);
        }

        // ── Edit ──────────────────────────────────────────────────────────────

        public async Task<IActionResult> EditPatient(int? id)
        {
            if (id == null) return NotFound();
            var patient = await _patientService.GetByIdAsync(id.Value);
            if (patient == null) return NotFound();

            var vm = new EditPatientViewModel
            {
                Id                     = patient.Id,
                FirstName              = patient.FirstName,
                LastName               = patient.LastName,
                DateOfBirth            = patient.DateOfBirth,
                Gender                 = patient.Gender,
                ContactNumber          = patient.ContactNumber,
                EmergencyContact       = patient.EmergencyContact,
                EmergencyContactNumber = patient.EmergencyContactNumber,
                Address                = patient.Address,
                NextOfKin              = patient.NextOfKin,
                NextOfKinContact       = patient.NextOfKinContact,
                BloodType              = patient.BloodType,
                ChronicMedications     = patient.ChronicMedications,
                MedicalHistory         = patient.MedicalHistory,
                Allergies              = patient.Allergies,
                WardId                 = patient.WardId,
                BedId                  = patient.BedId,
                AssignedDoctorId       = patient.AssignedDoctorId,
                PatientStatus          = patient.PatientStatus
            };

            LoadEditDropdowns(patient.WardId, patient.BedId, patient.AssignedDoctorId, patient.Id);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPatient(int id, EditPatientViewModel vm)
        {
            if (id != vm.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                LoadEditDropdowns(vm.WardId, vm.BedId, vm.AssignedDoctorId, vm.Id);
                return View(vm);
            }

            try
            {
                await _patientService.UpdateAsync(id, vm);
                TempData["SuccessMessage"] = $"Patient information updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient {PatientId}", id);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
            }

            LoadEditDropdowns(vm.WardId, vm.BedId, vm.AssignedDoctorId, vm.Id);
            return View(vm);
        }

        // ── Details ───────────────────────────────────────────────────────────

        public async Task<IActionResult> DetailsPatient(int? id)
        {
            if (id == null) return NotFound();
            var patient = await _patientService.GetByIdAsync(id.Value);
            return patient == null ? NotFound() : View(patient);
        }

        // ── Patient Folder ────────────────────────────────────────────────────

        public async Task<IActionResult> PatientFolder(int? id)
        {
            if (id == null) return NotFound();
            var patient = await _patientService.GetFullFolderAsync(id.Value);
            return patient == null ? NotFound() : View(patient);
        }

        // ── Discharge ─────────────────────────────────────────────────────────

        public async Task<IActionResult> Discharge(int? id)
        {
            if (id == null) return NotFound();
            var patient = await _patientService.GetByIdAsync(id.Value);
            if (patient == null) return NotFound();

            var vm = new DischargePatientViewModel
            {
                PatientId   = patient.Id,
                PatientName = patient.FullName,
                WardName    = patient.Ward?.Name,
                BedNumber   = patient.Bed?.BedNumber,
                DoctorName  = patient.AssignedDoctor?.FullName,
                DischargeDate = DateTime.UtcNow
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Discharge(int id, DischargePatientViewModel vm)
        {
            vm.PatientId = id;
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                await _patientService.DischargeAsync(id, vm, User.Identity?.Name ?? "Unknown");
                TempData["SuccessMessage"] = $"Patient {vm.PatientName} has been successfully discharged.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error discharging patient {PatientId}", id);
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
        }

        // ── Soft Delete ───────────────────────────────────────────────────────

        public async Task<IActionResult> DeletePatient(int? id)
        {
            if (id == null) return NotFound();
            var patient = await _patientService.GetByIdAsync(id.Value);
            return patient == null ? NotFound() : View(patient);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _patientService.SoftDeleteAsync(id);
                TempData["SuccessMessage"] = "Patient has been deactivated.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting patient {PatientId}", id);
                TempData["ErrorMessage"] = "Could not deactivate patient.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ── Medications (reference data, managed here for Ward Admin convenience) ──

        public async Task<IActionResult> ManageMedications()
        {
            var medications = await _context.Medications.Where(m => m.IsActive).ToListAsync();
            return View(medications);
        }

        public IActionResult CreateMedication() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMedication([Bind("Name,Description,Dosage,Schedule")] Medication medication)
        {
            if (!ModelState.IsValid) return View(medication);
            medication.IsActive = true;
            _context.Add(medication);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Medication '{medication.Name}' created.";
            return RedirectToAction(nameof(ManageMedications));
        }

        public async Task<IActionResult> EditMedication(int? id)
        {
            if (id == null) return NotFound();
            var med = await _context.Medications.FindAsync(id);
            return med == null ? NotFound() : View(med);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMedication(int id, [Bind("Id,Name,Description,Dosage,Schedule,IsActive")] Medication medication)
        {
            if (id != medication.Id) return NotFound();
            if (!ModelState.IsValid) return View(medication);
            _context.Update(medication);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Medication '{medication.Name}' updated.";
            return RedirectToAction(nameof(ManageMedications));
        }

        public async Task<IActionResult> DeleteMedication(int? id)
        {
            if (id == null) return NotFound();
            var med = await _context.Medications.FindAsync(id);
            return med == null ? NotFound() : View(med);
        }

        [HttpPost, ActionName("DeleteMedication"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedicationConfirmed(int id)
        {
            var med = await _context.Medications.FindAsync(id);
            if (med != null) { med.IsActive = false; await _context.SaveChangesAsync(); }
            TempData["SuccessMessage"] = "Medication deactivated.";
            return RedirectToAction(nameof(ManageMedications));
        }

        // ── Allergies ──────────────────────────────────────────────────────────

        public async Task<IActionResult> ManageAllergies(int? id)
        {
            if (id == null) return NotFound();
            var allergies = await _context.Allergies
                .Include(a => a.Patient)
                .Where(a => a.PatientId == id && a.IsActive)
                .ToListAsync();
            ViewBag.PatientId = id;
            return View(allergies);
        }

        public IActionResult CreateAllergy()
        {
            ViewBag.PatientId = new SelectList(_context.Patients.Where(p => p.IsActive), "Id", "FirstName");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAllergy(int patientId, string allergyName)
        {
            _context.Add(new Allergy { PatientId = patientId, AllergyName = allergyName, IsActive = true });
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Allergy added.";
            return RedirectToAction(nameof(ManageAllergies), new { id = patientId });
        }

        public async Task<IActionResult> EditAllergy(int? id)
        {
            if (id == null) return NotFound();
            var allergy = await _context.Allergies.Include(a => a.Patient).FirstOrDefaultAsync(a => a.Id == id);
            return allergy == null ? NotFound() : View(allergy);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAllergy(int id, [Bind("Id,PatientId,AllergyName,IsActive")] Allergy allergy)
        {
            if (id != allergy.Id) return NotFound();
            if (ModelState.IsValid) { _context.Update(allergy); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(ManageAllergies), new { id = allergy.PatientId });
        }

        public async Task<IActionResult> DeleteAllergy(int? id)
        {
            if (id == null) return NotFound();
            var allergy = await _context.Allergies.Include(a => a.Patient).FirstOrDefaultAsync(a => a.Id == id);
            return allergy == null ? NotFound() : View(allergy);
        }

        [HttpPost, ActionName("DeleteAllergy"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllergyConfirmed(int id)
        {
            var allergy = await _context.Allergies.FindAsync(id);
            if (allergy != null) { allergy.IsActive = false; await _context.SaveChangesAsync(); }
            TempData["SuccessMessage"] = "Allergy removed.";
            return RedirectToAction(nameof(ManageAllergies), new { id = allergy?.PatientId });
        }

        // ── Medical Conditions ────────────────────────────────────────────────

        public async Task<IActionResult> ManageMedicalConditions()
        {
            var conditions = await _context.MedicalConditions.Include(m => m.Patient).Where(m => m.IsActive).ToListAsync();
            return View(conditions);
        }

        public IActionResult CreateMedicalCondition()
        {
            ViewBag.PatientId = new SelectList(_context.Patients.Where(p => p.IsActive), "Id", "FirstName");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMedicalCondition(int patientId, string conditionName)
        {
            _context.Add(new MedicalCondition { PatientId = patientId, ConditionName = conditionName, IsActive = true });
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Condition added.";
            return RedirectToAction(nameof(ManageMedicalConditions));
        }

        public async Task<IActionResult> EditMedicalCondition(int? id)
        {
            if (id == null) return NotFound();
            var cond = await _context.MedicalConditions.Include(m => m.Patient).FirstOrDefaultAsync(m => m.Id == id);
            return cond == null ? NotFound() : View(cond);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMedicalCondition(int id, [Bind("Id,PatientId,ConditionName,IsActive")] MedicalCondition condition)
        {
            if (id != condition.Id) return NotFound();
            if (ModelState.IsValid) { _context.Update(condition); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(ManageMedicalConditions));
        }

        public async Task<IActionResult> DeleteMedicalCondition(int? id)
        {
            if (id == null) return NotFound();
            var cond = await _context.MedicalConditions.Include(m => m.Patient).FirstOrDefaultAsync(m => m.Id == id);
            return cond == null ? NotFound() : View(cond);
        }

        [HttpPost, ActionName("DeleteMedicalCondition"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedicalConditionConfirmed(int id)
        {
            var cond = await _context.MedicalConditions.FindAsync(id);
            if (cond != null) { cond.IsActive = false; await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(ManageMedicalConditions));
        }

        // ── Patient Movements ─────────────────────────────────────────────────

        public async Task<IActionResult> ManageMovements(int? id)
        {
            if (id == null) return NotFound();

            var patient = await _patientService.GetByIdAsync(id.Value);
            if (patient == null) return NotFound();

            var movements = await _context.PatientMovements
                .Include(m => m.FromWard)
                .Include(m => m.ToWard)
                .Where(m => m.PatientId == id && m.IsActive)
                .OrderByDescending(m => m.MovementDate)
                .ToListAsync();

            ViewBag.Patient   = patient;
            ViewBag.FromWards = new SelectList(_context.Wards.Where(w => w.IsActive), "Id", "Name");
            ViewBag.ToWards   = new SelectList(_context.Wards.Where(w => w.IsActive), "Id", "Name");
            return View(movements);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageMovements(int id,
            [Bind("FromWardId,ToWardId,MovementDate,MovementType,MovementReason")] PatientMovement movement)
        {
            movement.PatientId  = id;
            movement.RecordedBy = User.Identity?.Name ?? "Unknown";
            movement.IsActive   = true;

            if (ModelState.IsValid)
            {
                _context.Add(movement);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Movement recorded successfully.";
                return RedirectToAction(nameof(ManageMovements), new { id });
            }

            var patient = await _patientService.GetByIdAsync(id);
            var movements = await _context.PatientMovements
                .Include(m => m.FromWard).Include(m => m.ToWard)
                .Where(m => m.PatientId == id && m.IsActive)
                .OrderByDescending(m => m.MovementDate).ToListAsync();

            ViewBag.Patient   = patient;
            ViewBag.FromWards = new SelectList(_context.Wards.Where(w => w.IsActive), "Id", "Name", movement.FromWardId);
            ViewBag.ToWards   = new SelectList(_context.Wards.Where(w => w.IsActive), "Id", "Name", movement.ToWardId);
            return View(movements);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void LoadAdmitDropdowns(int wardId = 0, int bedId = 0, int doctorId = 0)
        {
            ViewBag.WardId = new SelectList(_context.Wards.Where(w => w.IsActive), "Id", "Name", wardId);
            ViewBag.BedId  = new SelectList(_context.Beds.Where(b => b.IsActive && b.PatientId == null), "Id", "BedNumber", bedId);
            ViewBag.AssignedDoctorId = new SelectList(_context.Staff.Where(s => s.Role == "Doctor" && s.IsActive), "Id", "FullName", doctorId);
        }

        private void LoadEditDropdowns(int? wardId, int? bedId, int? doctorId, int patientId)
        {
            ViewBag.WardId = new SelectList(_context.Wards.Where(w => w.IsActive), "Id", "Name", wardId);
            ViewBag.BedId  = new SelectList(_context.Beds.Where(b => b.IsActive && (b.PatientId == null || b.PatientId == patientId)), "Id", "BedNumber", bedId);
            ViewBag.AssignedDoctorId = new SelectList(_context.Staff.Where(s => s.Role == "Doctor" && s.IsActive), "Id", "FullName", doctorId);
        }

        // ── PDF Downloads ─────────────────────────────────────────────────────

        /// <summary>
        /// Downloads the patient folder as a PDF.
        /// Available to Ward Admin.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DownloadPatientFolder(int id)
        {
            var patient = await _patientService.GetFullFolderAsync(id);
            if (patient == null) return NotFound();

            var pdf = _pdfService.Generate(patient);
            var filename = $"PatientFolder_{patient.LastName}_{patient.FirstName}_{DateTime.UtcNow:yyyyMMdd}.pdf";
            return File(pdf, "application/pdf", filename);
        }

        /// <summary>
        /// Downloads the discharge summary as a PDF.
        /// Only available once the patient has been discharged.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DownloadDischargeSummary(int id)
        {
            var patient = await _patientService.GetFullFolderAsync(id);
            if (patient == null) return NotFound();

            if (patient.PatientStatus != "Discharged")
            {
                TempData["ErrorMessage"] = "Discharge summary is only available after the patient has been discharged.";
                return RedirectToAction(nameof(PatientFolder), new { id });
            }

            var pdf = _pdfService.GenerateDischargeSummary(patient);
            var filename = $"DischargeSummary_{patient.LastName}_{patient.FirstName}_{DateTime.UtcNow:yyyyMMdd}.pdf";
            return File(pdf, "application/pdf", filename);
        }
    }
}
