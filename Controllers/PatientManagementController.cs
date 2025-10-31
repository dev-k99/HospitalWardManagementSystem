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

        public PatientManagementController(WardSystemDBContext context)
        {
            _context = context;
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
            return View();
        }

        // POST: PatientManagement/Admit - Process patient admission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Admit([Bind("FirstName,LastName,DateOfBirth,Gender,ContactNumber,EmergencyContact,EmergencyContactNumber,Address,NextOfKin,NextOfKinContact,BloodType,ChronicMedications,MedicalHistory,Allergies,AdmissionReason,WardId,BedId,AssignedDoctorId")] Patient patient)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Set admission details
                    patient.AdmissionDate = DateTime.Now;
                    patient.PatientStatus = "Admitted";
                    patient.IsActive = true;

                    _context.Add(patient);
                    await _context.SaveChangesAsync();

                    // Assign bed if selected
                    if (patient.BedId.HasValue)
                    {
                        var bed = await _context.Beds.FindAsync(patient.BedId);
                        if (bed != null)
                        {
                            bed.PatientId = patient.Id;
                            _context.Update(bed);
                        }
                    }

                    await _context.SaveChangesAsync();

                    // Create patient movement record for admission
                    var movement = new PatientMovement
                    {
                        PatientId = patient.Id,
                        FromWardId = 0, // External admission
                        ToWardId = patient.WardId.Value,
                        MovementDate = DateTime.Now,
                        IsActive = true
                    };
                    _context.Add(movement);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Patient {patient.FirstName} {patient.LastName} has been successfully admitted.";
                    return RedirectToAction(nameof(PatientFolder), new { id = patient.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error admitting patient: {ex.Message}");
                }
            }

            ViewBag.WardId = new SelectList(_context.Wards.Where(w => w.IsActive), "Id", "Name", patient.WardId);
            ViewBag.BedId = new SelectList(_context.Beds.Where(b => b.IsActive && b.PatientId == null), "Id", "BedNumber", patient.BedId);
            ViewBag.AssignedDoctorId = new SelectList(_context.Staff.Where(s => s.Role == "Doctor" && s.IsActive), "Id", "FullName", patient.AssignedDoctorId);
            return View(patient);
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

        // GET: PatientManagement/Edit - Edit patient information
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var patient = await _context.Patients
                .Include(p => p.Ward)
                .Include(p => p.Bed)
                .Include(p => p.AssignedDoctor)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null) return NotFound();

            ViewBag.WardId = new SelectList(_context.Wards.Where(w => w.IsActive), "Id", "Name", patient.WardId);
            ViewBag.BedId = new SelectList(_context.Beds.Where(b => b.IsActive && (b.PatientId == null || b.PatientId == patient.Id)), "Id", "BedNumber", patient.BedId);
            ViewBag.AssignedDoctorId = new SelectList(_context.Staff.Where(s => s.Role == "Doctor" && s.IsActive), "Id", "FullName", patient.AssignedDoctorId);
            return View(patient);
        }

        // POST: PatientManagement/Edit - Update patient information
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,DateOfBirth,Gender,ContactNumber,EmergencyContact,EmergencyContactNumber,Address,NextOfKin,NextOfKinContact,BloodType,ChronicMedications,MedicalHistory,Allergies,WardId,BedId,AssignedDoctorId,PatientStatus")] Patient patient)
        {
            if (id != patient.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingPatient = await _context.Patients.FindAsync(id);
                    if (existingPatient == null) return NotFound();

                    // Handle bed assignment changes
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

                    // Update patient properties
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

                    _context.Update(existingPatient);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Patient information updated successfully.";
                    return RedirectToAction(nameof(PatientFolder), new { id = patient.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(patient.Id)) return NotFound();
                    else throw;
                }
            }

            ViewBag.WardId = new SelectList(_context.Wards.Where(w => w.IsActive), "Id", "Name", patient.WardId);
            ViewBag.BedId = new SelectList(_context.Beds.Where(b => b.IsActive && (b.PatientId == null || b.PatientId == patient.Id)), "Id", "BedNumber", patient.BedId);
            ViewBag.AssignedDoctorId = new SelectList(_context.Staff.Where(s => s.Role == "Doctor" && s.IsActive), "Id", "FullName", patient.AssignedDoctorId);
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

        // GET: PatientManagement/Details - View patient details
        public async Task<IActionResult> Details(int? id)
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
        public async Task<IActionResult> Delete(int? id)
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
                var patientName = $"{patient.FirstName} {patient.LastName}";
                // Free up bed if assigned
                if (patient.BedId.HasValue)
                {
                    var bed = await _context.Beds.FindAsync(patient.BedId);
                    if (bed != null) bed.PatientId = null;
                }

                patient.IsActive = false;
                _context.Update(patient);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Patient '{patientName}' has been successfully deactivated.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: PatientManagement/ManageAllergies - Manage patient allergies
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

        private bool PatientExists(int id)
        {
            return _context.Patients.Any(e => e.Id == id);
        }
    }
}