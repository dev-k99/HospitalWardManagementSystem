using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WardSystemProject.Data;
using WardSystemProject.Models;

namespace WardSystemProject.Controllers
{
    [Authorize(Roles = "Ward Admin")]
    public class PatientManagementController : Controller
    {
        private readonly WardSystemDBContext _context;
        private readonly ILogger<PatientManagementController> _logger;

        public PatientManagementController(WardSystemDBContext context, ILogger<PatientManagementController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: PatientManagement - List all patients
        public async Task<IActionResult> Index()
        {
            var patients = await _context.Patients
                .Include(p => p.Ward)
                .Include(p => p.Bed)
                .Include(p => p.AssignedDoctor)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.AdmissionDate)
                .ToListAsync();
            return View(patients);
        }
        public async Task<IActionResult> ManageMedications()
        {
            var medications = await _context.Medications.Where(m => m.IsActive).ToListAsync();
            return View(medications);
        }

        // GET: PatientManagement/Search - Search patients
        public async Task<IActionResult> Search(string query)
        {
            var filteredPatients = await _context.Patients
                .Include(p => p.Ward)
                .Include(p => p.Bed)
                .Include(p => p.AssignedDoctor)
                .Where(p => p.IsActive && 
                    (p.FirstName.Contains(query) || 
                     p.LastName.Contains(query) || 
                     p.ContactNumber.Contains(query)))
                .OrderByDescending(p => p.AdmissionDate)
                .ToListAsync();
            return View("Index", filteredPatients);
        }

        // GET: PatientManagement/Admit - Patient admission form
        public IActionResult Admit()
        {
            ViewBag.WardId = new SelectList(_context.Wards.Where(w => w.IsActive), "Id", "Name");
            ViewBag.BedId = new SelectList(_context.Beds.Where(b => b.IsActive && b.PatientId == null), "Id", "BedNumber");
            ViewBag.AssignedDoctorId = new SelectList(_context.Staff.Where(s => s.Role == "Doctor" && s.IsActive), "Id", "FullName");
            return View(new Patient { AdmissionDate = DateTime.Now, PatientStatus = "Admitted", IsActive = true });
        }

        // POST: PatientManagement/Admit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Admit([Bind("FirstName,LastName,DateOfBirth,Gender,ContactNumber,EmergencyContact,EmergencyContactNumber,Address,NextOfKin,NextOfKinContact,BloodType,ChronicMedications,MedicalHistory,Allergies,AdmissionReason,AdmissionDate,DischargeDate,DischargeSummary,PatientStatus,IsActive,WardId,BedId,AssignedDoctorId")] Patient patient)
        {
            try
            {
              
                patient.PatientStatus = "Admitted";
                patient.IsActive = true;

                // Add patient
                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Patient {PatientId} added to database", patient.Id);
                TempData["SuccessMessage"] = $"Patient {patient.FirstName} {patient.LastName} has been successfully admitted.";
                return RedirectToAction(nameof(Index));
                // Assign bed
                if (patient.BedId.HasValue)
                {
                    var bed = await _context.Beds.FindAsync(patient.BedId);
                    if (bed != null)
                    {
                        if (bed.PatientId != null)
                        {
                            ModelState.AddModelError("", "Selected bed is already occupied. Please choose another bed.");
                            ReloadDropdowns(patient);
                            return View(patient);
                        }
                        bed.PatientId = patient.Id;
                        _context.Update(bed);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Bed {BedId} assigned to patient {PatientId}", patient.BedId, patient.Id);
                    }
                }

                // Create patient folder
                var patientFolder = new PatientFolder
                {
                    PatientId = patient.Id,
                    AdmissionDate = patient.AdmissionDate,
                    DischargeDate = patient.DischargeDate,
                    DischargeSummary = patient.DischargeSummary,
                    AssignedDoctorId = patient.AssignedDoctorId,
                    WardId = patient.WardId,
                    BedId = patient.BedId,
                    PatientStatus = patient.PatientStatus
                };
                _context.PatientFolders.Add(patientFolder);
                await _context.SaveChangesAsync();
                _logger.LogInformation("PatientFolder created for patient {PatientId}", patient.Id);

                // Record patient movement
                var movement = new PatientMovement
                {
                    PatientId = patient.Id,
                    FromWardId = 0, // External admission
                    ToWardId = patient.WardId ?? 0,
                    MovementDate = DateTime.Now,
                    IsActive = true
                };
                _context.PatientMovements.Add(movement);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Patient movement recorded for patient {PatientId} to ward {WardId}", patient.Id, patient.WardId);
                         
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error admitting patient: {Message}", ex.Message);
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            ReloadDropdowns(patient);
            return View(patient);
        }
        public IActionResult CreateMedication()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMedication([Bind("Name,Description,Dosage,Schedule")] Medication medication)
        {
            if (ModelState.IsValid)
            {
                medication.IsActive = true;
                _context.Add(medication);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Medication '{medication.Name}' created successfully!";
                return RedirectToAction(nameof(ManageMedications));
            }
            return View(medication);
        }

        public async Task<IActionResult> EditMedication(int? id)
        {
            if (id == null) return NotFound();
            var medication = await _context.Medications.FindAsync(id);
            if (medication == null) return NotFound();
            return View(medication);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMedication(int id, [Bind("Id,Name,Description,Dosage,IsActive")] Medication medication)
        {
            if (id != medication.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(medication);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Medication '{medication.Name}' updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicationExists(medication.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(ManageMedications));
            }
            return View(medication);
        }

        public async Task<IActionResult> DeleteMedication(int? id)
        {
            if (id == null) return NotFound();
            var medication = await _context.Medications.FindAsync(id);
            if (medication == null) return NotFound();
            return View(medication);
        }

        [HttpPost, ActionName("DeleteMedication")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedicationConfirmed(int id)
        {
            var medication = await _context.Medications.FindAsync(id);
            if (medication != null)
            {
                var medicationName = medication.Name;
                medication.IsActive = false;
                _context.Update(medication);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Medication '{medicationName}' deactivated successfully!";
            }
            return RedirectToAction(nameof(ManageMedications));
        }

        // Allergy Management Actions
        //public async Task<IActionResult> ManageAllergies()
        //{
        //    var allergies = await _context.Allergies
        //        .Include(a => a.Patient)
        //        .Where(a => a.IsActive)
        //        .ToListAsync();
        //    return View(allergies);
        //}

        public IActionResult CreateAllergy()
        {
            ViewBag.PatientId = new SelectList(_context.Patients.Where(p => p.IsActive), "Id", "FirstName");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAllergy(int patientId, string allergyName)
        {
            try
            {
                var allergy = new Allergy
                {
                    PatientId = patientId,
                    AllergyName = allergyName,
                    IsActive = true
                };
                _context.Add(allergy);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $" Allergy added";
                return RedirectToAction(nameof(ManageAllergies));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                ViewBag.PatientId = new SelectList(_context.Patients.Where(p => p.IsActive), "Id", "FirstName", patientId);
                return View();
            }

        }
        public IActionResult CreateMedicalCondition()
        {
            ViewBag.PatientId = new SelectList(_context.Patients.Where(p => p.IsActive), "Id", "FirstName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMedicalCondition(int patientId, string conditionName)
        {

            try
            {
                var medicalCondition = new MedicalCondition
                {
                    PatientId = patientId,
                    ConditionName = conditionName,
                    IsActive = true
                };
                _context.Add(medicalCondition);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $" Condition added";
                return RedirectToAction(nameof(ManageAllergies));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                ViewBag.PatientId = new SelectList(_context.Patients.Where(p => p.IsActive), "Id", "FirstName", patientId);
                return View();
            }

        }
        public async Task<IActionResult> EditAllergy(int? id)
        {
            if (id == null) return NotFound();
            var allergy = await _context.Allergies
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (allergy == null) return NotFound();
            return View(allergy);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAllergy(int id, [Bind("Id,PatientId,AllergyName,IsActive")] Allergy allergy)
        {
            if (id != allergy.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(allergy);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Allergy '{allergy.AllergyName}' updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AllergyExists(allergy.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(ManageAllergies));
            }
            return View(allergy);
        }

        public async Task<IActionResult> DeleteAllergy(int? id)
        {
            if (id == null) return NotFound();
            var allergy = await _context.Allergies
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (allergy == null) return NotFound();
            return View(allergy);
        }

        [HttpPost, ActionName("DeleteAllergy")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllergyConfirmed(int id)
        {
            var allergy = await _context.Allergies.FindAsync(id);
            if (allergy != null)
            {
                var allergyName = allergy.AllergyName;
                allergy.IsActive = false;
                _context.Update(allergy);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Allergy '{allergyName}' deactivated successfully!";
            }
            return RedirectToAction(nameof(ManageAllergies));
        }

        // Medical Condition Management Actions
        public async Task<IActionResult> ManageMedicalConditions()
        {
            var conditions = await _context.MedicalConditions
                .Include(m => m.Patient)
                .Where(m => m.IsActive)
                .ToListAsync();
            return View(conditions);
        }



        public async Task<IActionResult> EditMedicalCondition(int? id)
        {
            if (id == null) return NotFound();
            var condition = await _context.MedicalConditions
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (condition == null) return NotFound();
            return View(condition);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMedicalCondition(int id, [Bind("Id,PatientId,ConditionName,IsActive")] MedicalCondition condition)
        {
            if (id != condition.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(condition);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Medical condition '{condition.ConditionName}' updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicalConditionExists(condition.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(ManageMedicalConditions));
            }
            return View(condition);
        }

        public async Task<IActionResult> DeleteMedicalCondition(int? id)
        {
            if (id == null) return NotFound();
            var condition = await _context.MedicalConditions
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (condition == null) return NotFound();
            return View(condition);
        }

        [HttpPost, ActionName("DeleteMedicalCondition")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedicalConditionConfirmed(int id)
        {
            var condition = await _context.MedicalConditions.FindAsync(id);
            if (condition != null)
            {
                var conditionName = condition.ConditionName;
                condition.IsActive = false;
                _context.Update(condition);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Medical condition '{conditionName}' deactivated successfully!";
            }
            return RedirectToAction(nameof(ManageMedicalConditions));
        }

        private void ReloadDropdowns(Patient patient)
        {
            ViewBag.WardId = new SelectList(_context.Wards.Where(w => w.IsActive), "Id", "Name", patient.WardId);
            ViewBag.BedId = new SelectList(_context.Beds.Where(b => b.IsActive && b.PatientId == null), "Id", "BedNumber", patient.BedId);
            ViewBag.AssignedDoctorId = new SelectList(_context.Staff.Where(s => s.Role == "Doctor" && s.IsActive), "Id", "FullName", patient.AssignedDoctorId);
        }



        // GET: PatientManagement/Edit - Edit patient information
        
        public async Task<IActionResult> EditPatient(int? id)
        {
            try
            {
                if (id == null)
                {
                    TempData["ErrorMessage"] = "Invalid patient ID provided.";
                    return RedirectToAction(nameof(Index));
                }

                var patient = await _context.Patients
                    .Include(p => p.Ward)
                    .Include(p => p.Bed)
                    .Include(p => p.AssignedDoctor)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Patient not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Dropdowns
                ViewBag.WardId = new SelectList(
                    _context.Wards.Where(w => w.IsActive), "Id", "Name", patient.WardId);
                ViewBag.BedId = new SelectList(
                    _context.Beds.Where(b => b.IsActive && (b.PatientId == null || b.PatientId == patient.Id)),
                    "Id", "BedNumber", patient.BedId);
                ViewBag.AssignedDoctorId = new SelectList(
                    _context.Staff.Where(s => s.Role == "Doctor" && s.IsActive),
                    "Id", "FullName", patient.AssignedDoctorId);

                return View(patient);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading patient details: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }



        // POST: PatientManagement/Edit - Update patient information
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPatient(int id, [Bind("Id,FirstName,LastName,DateOfBirth,Gender,ContactNumber,EmergencyContact,EmergencyContactNumber,Address,NextOfKin,NextOfKinContact,BloodType,ChronicMedications,MedicalHistory,Allergies,AdmissionDate,DischargeDate,PatientStatus,DischargeSummary,WardId,BedId,AssignedDoctorId")] Patient patient)
        {
            if (id != patient.Id)
            {
                TempData["ErrorMessage"] = "Mismatched patient ID.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    throw new InvalidOperationException("Please correct the highlighted form errors before submitting.");
                }

                var existingPatient = await _context.Patients
                    .Include(p => p.Bed)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (existingPatient == null)
                {
                    TempData["ErrorMessage"] = "Patient record could not be found.";
                    return RedirectToAction(nameof(Index));
                }

                // Handle bed reassignment logic
                if (existingPatient.BedId != patient.BedId)
                {
                    // Free up old bed
                    if (existingPatient.BedId.HasValue)
                    {
                        var oldBed = await _context.Beds.FindAsync(existingPatient.BedId);
                        if (oldBed != null) oldBed.PatientId = null;
                    }

                    // Assign new bed
                    if (patient.BedId.HasValue)
                    {
                        var newBed = await _context.Beds.FindAsync(patient.BedId);
                        if (newBed != null) newBed.PatientId = patient.Id;
                    }
                }

                // Update patient info
                existingPatient.FirstName = patient.FirstName;
                existingPatient.LastName = patient.LastName;
                existingPatient.DateOfBirth = patient.DateOfBirth;
                existingPatient.Gender = patient.Gender;
                existingPatient.ContactNumber = patient.ContactNumber;
                existingPatient.EmergencyContact = patient.EmergencyContact;
                existingPatient.EmergencyContactNumber = patient.EmergencyContactNumber;
                existingPatient.Address = patient.Address;
                existingPatient.NextOfKin = patient.NextOfKin;
                existingPatient.NextOfKinContact = patient.NextOfKinContact;
                existingPatient.BloodType = patient.BloodType;
                existingPatient.ChronicMedications = patient.ChronicMedications;
                existingPatient.MedicalHistory = patient.MedicalHistory;
                existingPatient.Allergies = patient.Allergies;
                existingPatient.WardId = patient.WardId;
                existingPatient.BedId = patient.BedId;
                existingPatient.AssignedDoctorId = patient.AssignedDoctorId;
                existingPatient.PatientStatus = patient.PatientStatus;
                existingPatient.DischargeSummary = patient.DischargeSummary;

                _context.Update(existingPatient);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Patient information updated successfully.";
                return RedirectToAction(nameof(Index), new { id = patient.Id });
            }
            catch (InvalidOperationException ex)
            {
                // Handle validation issues
                ModelState.AddModelError("", ex.Message);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PatientExists(patient.Id))
                {
                    TempData["ErrorMessage"] = "Patient record no longer exists.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "A concurrency issue occurred. Please try again.";
                }
            }
            catch (DbUpdateException dbEx)
            {
                TempData["ErrorMessage"] = $"Database error: {dbEx.Message}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An unexpected error occurred: {ex.Message}";
            }

            // Repopulate dropdowns if returning to form
            ViewBag.WardId = new SelectList(
                _context.Wards.Where(w => w.IsActive), "Id", "Name", patient.WardId);
            ViewBag.BedId = new SelectList(
                _context.Beds.Where(b => b.IsActive && (b.PatientId == null || b.PatientId == patient.Id)),
                "Id", "BedNumber", patient.BedId);
            ViewBag.AssignedDoctorId = new SelectList(
                _context.Staff.Where(s => s.Role == "Doctor" && s.IsActive),
                "Id", "FullName", patient.AssignedDoctorId);

            return View(patient);
        }

        public async Task<IActionResult> DetailsPatient(int? id)
        {
            if (id == null) return NotFound();

            var patient = await _context.Patients
                .Include(p => p.Ward)
                .Include(p => p.Bed)
                .Include(p => p.AssignedDoctor)
                .Include(p => p.PatientAllergies)
                .Include(p => p.MedicalConditions)
                .Include(p => p.PatientMovements).ThenInclude(m => m.FromWard)
                .Include(p => p.PatientMovements).ThenInclude(m => m.ToWard)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null) return NotFound();

            return View(patient);
        }

        // GET: PatientManagement/Delete - Delete patient
        public async Task<IActionResult> DeletePatient(int? id)
        {
            if (id == null) return NotFound();

            var patient = await _context.Patients
                .Include(p => p.Ward)
                .Include(p => p.Bed)
                .Include(p => p.AssignedDoctor)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null) return NotFound();

            return View(patient);
        }

        // POST: PatientManagement/Delete - Soft delete patient
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {          
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                var patientName = patient.FirstName;
                // Free up bed if assigned
                if (patient.BedId.HasValue)
                {
                    var bed = await _context.Beds.FindAsync(patient.BedId);
                    if (bed != null) bed.PatientId = null;
                }

                patient.IsActive = false;
                _context.Remove(patient);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Patient '{patientName}' has been successfully deactivated.";
            }
            return RedirectToAction(nameof(Index));
        }   

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDeleteBed(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                var patientName  = patient.FirstName;
                patient.IsActive = false; // This part performs the soft delete
                _context.Update(patient);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Patient '{patientName}' has been deactivated successfully.";
            }
            return RedirectToAction(nameof(Index));
        }
        private bool PatientExists(int id)
        {
            return _context.Patients.Any(e => e.Id == id);
        }
       // GET: PatientManagement/PatientFolder - Open patient admission folder
        public async Task<IActionResult> PatientFolder(int? id)
        {
            if (id == null) return NotFound();

            var patient = await _context.Patients
                .Include(p => p.Ward)
                .Include(p => p.Bed)
                .Include(p => p.AssignedDoctor)
                .Include(p => p.PatientAllergies)
                .Include(p => p.MedicalConditions)
                .Include(p => p.PatientMovements).ThenInclude(m => m.FromWard)
                .Include(p => p.PatientMovements).ThenInclude(m => m.ToWard)
                .Include(p => p.DoctorVisits).ThenInclude(dv => dv.Doctor)
                .Include(p => p.VitalSigns)
                .Include(p => p.MedicationAdministrations).ThenInclude(ma => ma.Medication)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null) return NotFound();

            return View(patient);
        }
       // GET: PatientManagement/Discharge - Discharge patient
        public async Task<IActionResult> Discharge(int? id)
        {
            if (id == null) return NotFound();

            var patient = await _context.Patients
                .Include(p => p.Ward)
                .Include(p => p.Bed)
                .Include(p => p.AssignedDoctor)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null) return NotFound();

            return View(patient);
        }

        // POST: PatientManagement/Discharge - Process patient discharge
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Discharge(int id, [Bind("DischargeSummary")] Patient patient)
        {
            var existingPatient = await _context.Patients
                .Include(p => p.Bed)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingPatient == null) return NotFound();

            try
            {
                // Update discharge information
                existingPatient.DischargeDate = DateTime.Now;
                existingPatient.DischargeSummary = patient.DischargeSummary;
                existingPatient.PatientStatus = "Discharged";

                // Free up the bed
                if (existingPatient.BedId.HasValue)
                {
                    var bed = await _context.Beds.FindAsync(existingPatient.BedId);
                    if (bed != null)
                    {
                        bed.PatientId = null;
                        _context.Update(bed);
                    }
                    existingPatient.BedId = null;
                }

                // Remove ward assignment
                existingPatient.WardId = null;
                existingPatient.AssignedDoctorId = null;

                _context.Update(existingPatient);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Patient {existingPatient.FirstName} {existingPatient.LastName} has been successfully discharged.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error discharging patient: {ex.Message}");
                return View(existingPatient);
            }
        }

        // GET: PatientManagement/Movement - Record patient movement
        public async Task<IActionResult> Movement(int? id)
        {
            if (id == null) return NotFound();

            var patient = await _context.Patients
                .Include(p => p.Ward)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null) return NotFound();

            var movement = new PatientMovement
            {
                PatientId = patient.Id,
                FromWardId = patient.WardId ?? 0,
                MovementDate = DateTime.Now
            };

            ViewBag.FromWardId = new SelectList(_context.Wards.Where(w => w.IsActive), "Id", "Name", movement.FromWardId);
            ViewBag.ToWardId = new SelectList(_context.Wards.Where(w => w.IsActive), "Id", "Name");
            return View(movement);
        }

        // POST: PatientManagement/Movement - Process patient movement
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Movement([Bind("PatientId,FromWardId,ToWardId,MovementDate")] PatientMovement movement)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    movement.IsActive = true;
                    _context.Add(movement);

                    // Update patient's current ward
                    var patient = await _context.Patients.FindAsync(movement.PatientId);
                    if (patient != null)
                    {
                        patient.WardId = movement.ToWardId;
                        _context.Update(patient);
                    }

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Patient movement recorded successfully.";
                    return RedirectToAction(nameof(PatientFolder), new { id = movement.PatientId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error recording movement: {ex.Message}");
                }
            }

            ViewBag.FromWardId = new SelectList(_context.Wards.Where(w => w.IsActive), "Id", "Name", movement.FromWardId);
            ViewBag.ToWardId = new SelectList(_context.Wards.Where(w => w.IsActive), "Id", "Name", movement.ToWardId);
            return View(movement);
        }
        //GET: PatientManagement/ManageAllergies - Manage patient allergies
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

        // GET: PatientManagement/ManageConditions - Manage patient medical conditions
        public async Task<IActionResult> ManageConditions(int? id)
        {
            if (id == null) return NotFound();

            var conditions = await _context.MedicalConditions
                .Include(mc => mc.Patient)
                .Where(mc => mc.PatientId == id && mc.IsActive)
                .ToListAsync();

            ViewBag.PatientId = id;
            return View(conditions);
        }

        // GET: PatientManagement/ManageMovements - Manage patient movements
        public async Task<IActionResult> ManageMovements(int? id)
        {
            if (id == null) return NotFound();

            var movements = await _context.PatientMovements
                .Include(pm => pm.Patient)
                .Include(pm => pm.FromWard)
                .Include(pm => pm.ToWard)
                .Where(pm => pm.PatientId == id && pm.IsActive)
                .OrderByDescending(pm => pm.MovementDate)
                .ToListAsync();

            ViewBag.PatientId = id;
            return View(movements);
        }
        private bool WardExists(int id)
        {
            return _context.Wards.Any(e => e.Id == id);
        }

        private bool MedicationExists(int id)
        {
            return _context.Medications.Any(e => e.Id == id);
        }

        private bool AllergyExists(int id)
        {
            return _context.Allergies.Any(e => e.Id == id);
        }

        private bool MedicalConditionExists(int id)
        {
            return _context.MedicalConditions.Any(e => e.Id == id);
        }

        private bool StaffExists(int id)
        {
            return _context.Staff.Any(e => e.Id == id);
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }

        private bool BedExists(int id)
        {
            return _context.Beds.Any(e => e.Id == id);
        }
    }
}