using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WardSystemProject.Models;
using WardSystemProject.Data;

namespace WardSystemProject.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorPatientController : Controller
    {
        private readonly WardSystemDBContext _context;

        public DoctorPatientController(WardSystemDBContext context)
        {
            _context = context;
        }

        // GET: DoctorPatient/Index - Doctor Dashboard
        public async Task<IActionResult> Index()
        {
            var doctorId = GetCurrentDoctorId();
            
            // Debug information
            var currentUser = User.Identity?.Name;
            var currentUserEmail = User.Identity?.Name;
            var staffMembers = await _context.Staff.Where(s => s.Role == "Doctor").ToListAsync();
            
            if (doctorId == null)
            {
                // Return debug information instead of NotFound
                ViewBag.DebugInfo = new
                {
                    CurrentUser = currentUser,
                    CurrentUserEmail = currentUserEmail,
                    DoctorStaffCount = staffMembers.Count,
                    DoctorStaff = staffMembers.Select(s => new { s.Id, s.Email, s.Role, s.IsActive })
                };
                
                // Create a simple dashboard for testing
                var testDashboard = new DoctorDashboardViewModel
                {
                    TotalPatients = 0,
                    RecentVisits = new List<DoctorVisit>(),
                    PendingInstructions = new List<DoctorInstruction>()
                };
                
                return View(testDashboard);
            }

            var dashboard = new DoctorDashboardViewModel
            {
                TotalPatients = await _context.Patients.CountAsync(p => p.AssignedDoctorId == doctorId && p.IsCurrentlyAdmitted),
                RecentVisits = await _context.DoctorVisits
                    .Include(v => v.Patient)
                    .Where(v => v.DoctorId == doctorId && v.VisitDate >= DateTime.Today.AddDays(-7))
                    .OrderByDescending(v => v.VisitDate)
                    .Take(5)
                    .ToListAsync(),
                PendingInstructions = await _context.DoctorInstructions
                    .Include(i => i.Patient)
                    .Where(i => i.DoctorId == doctorId && i.Status == "Pending")
                    .OrderByDescending(i => i.InstructionDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(dashboard);
        }

        // GET: DoctorPatient/MyPatients - List of assigned patients
        public async Task<IActionResult> MyPatients()
        {
            var doctorId = GetCurrentDoctorId();
            
            if (doctorId == null)
            {
                // Return empty list with debug info instead of NotFound
                ViewBag.DebugInfo = new
                {
                    CurrentUser = User.Identity?.Name,
                    Message = "No doctor ID found - showing empty patient list"
                };
                
                return View(new List<Patient>());
            }

            var patients = await _context.Patients
                .Include(p => p.Ward)
                .Include(p => p.Bed)
                .Include(p => p.AssignedDoctor)
                .Where(p => p.AssignedDoctorId == doctorId && p.IsCurrentlyAdmitted)
                .OrderBy(p => p.Ward.Name)
                .ThenBy(p => p.Bed.BedNumber)
                .ToListAsync();

            return View(patients);
        }

        // GET: DoctorPatient/ViewPatientFolder/5 - Comprehensive patient folder
        public async Task<IActionResult> ViewPatientFolder(int? id)
        {
            if (id == null) return NotFound();

            var doctorId = GetCurrentDoctorId();
            
            if (doctorId == null)
            {
                // Return debug info instead of NotFound
                ViewBag.DebugInfo = new
                {
                    CurrentUser = User.Identity?.Name,
                    Message = "No doctor ID found - cannot access patient folder"
                };
                
                return View("Error", new ErrorViewModel { RequestId = "Doctor ID not found" });
            }

            var patient = await _context.Patients
                .Include(p => p.Ward)
                .Include(p => p.Bed)
                .Include(p => p.AssignedDoctor)
                .Include(p => p.PatientAllergies)
                .Include(p => p.MedicalConditions)
                .Include(p => p.VitalSigns.Where(v => v.IsActive).OrderByDescending(v => v.RecordDate))
                .Include(p => p.MedicationAdministrations.Where(m => m.IsActive).OrderByDescending(m => m.AdministrationDate))
                .Include(p => p.DoctorInstructions.Where(i => i.IsActive).OrderByDescending(i => i.InstructionDate))
                .Include(p => p.DoctorVisits.Where(v => v.IsActive).OrderByDescending(v => v.VisitDate))
                .Include(p => p.Prescriptions.Where(p => p.IsActive).OrderByDescending(p => p.PrescriptionDate))
                .FirstOrDefaultAsync(p => p.Id == id && p.AssignedDoctorId == doctorId);

            if (patient == null) return NotFound();

            return View(patient);
        }

        // GET: DoctorPatient/CreateVisit - Record a new doctor visit
        public async Task<IActionResult> CreateVisit(int? patientId)
        {
            var doctorId = GetCurrentDoctorId();
            
            if (doctorId == null)
            {
                // Return debug info instead of NotFound
                ViewBag.DebugInfo = new
                {
                    CurrentUser = User.Identity?.Name,
                    Message = "No doctor ID found - cannot create visit"
                };
                
                // Return empty visit form with debug info
                ViewBag.PatientId = new SelectList(new List<Patient>(), "Id", "FullName");
                
                return View(new DoctorVisit());
            }

            if (patientId.HasValue)
            {
                var patient = await _context.Patients
                    .Include(p => p.Ward)
                    .Include(p => p.Bed)
                    .FirstOrDefaultAsync(p => p.Id == patientId && p.AssignedDoctorId == doctorId);
                
                if (patient == null) return NotFound();
                
                ViewBag.Patient = patient;
            }

            ViewBag.PatientId = new SelectList(
                await _context.Patients
                    .Where(p => p.AssignedDoctorId == doctorId && p.IsCurrentlyAdmitted)
                    .ToListAsync(), 
                "Id", "FullName", patientId);

            return View(new DoctorVisit());
        }

        // POST: DoctorPatient/CreateVisit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVisit([Bind("PatientId,VisitType,Notes,NextVisitDate")] DoctorVisit visit)
        {
            var doctorId = GetCurrentDoctorId();
            
            if (doctorId == null)
            {
                // Return debug info instead of NotFound
                ViewBag.DebugInfo = new
                {
                    CurrentUser = User.Identity?.Name,
                    Message = "No doctor ID found - cannot create visit"
                };
                
                // Return empty visit form with debug info
                ViewBag.PatientId = new SelectList(new List<Patient>(), "Id", "FullName");
                
                return View(visit);
            }

            if (ModelState.IsValid)
            {
                visit.DoctorId = doctorId.Value;
                visit.VisitDate = DateTime.Now;
                visit.IsActive = true;

                _context.Add(visit);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Doctor visit recorded successfully.";
                return RedirectToAction(nameof(ViewPatientFolder), new { id = visit.PatientId });
            }

            ViewBag.PatientId = new SelectList(
                await _context.Patients
                    .Where(p => p.AssignedDoctorId == doctorId && p.IsCurrentlyAdmitted)
                    .ToListAsync(), 
                "Id", "FullName", visit.PatientId);

            return View(visit);
        }

        // GET: DoctorPatient/CreatePrescription - Prescribe medication
        public async Task<IActionResult> CreatePrescription(int? patientId)
        {
            var doctorId = GetCurrentDoctorId();
            
            if (doctorId == null)
            {
                // Return debug info instead of NotFound
                ViewBag.DebugInfo = new
                {
                    CurrentUser = User.Identity?.Name,
                    Message = "No doctor ID found - cannot create prescription"
                };
                
                // Return empty prescription form with debug info
                ViewBag.PatientId = new SelectList(new List<Patient>(), "Id", "FullName");
                ViewBag.MedicationId = new SelectList(new List<Medication>(), "Id", "Name");
                
                return View(new Prescription());
            }

            if (patientId.HasValue)
            {
                var patient = await _context.Patients
                    .Include(p => p.Ward)
                    .Include(p => p.Bed)
                    .FirstOrDefaultAsync(p => p.Id == patientId && p.AssignedDoctorId == doctorId);
                
                if (patient == null) return NotFound();
                
                ViewBag.Patient = patient;
            }

            ViewBag.PatientId = new SelectList(
                await _context.Patients
                    .Where(p => p.AssignedDoctorId == doctorId && p.IsCurrentlyAdmitted)
                    .ToListAsync(), 
                "Id", "FullName", patientId);

            ViewBag.MedicationId = new SelectList(
                await _context.Medications
                    .Where(m => m.IsActive)
                    .ToListAsync(), 
                "Id", "Name");

            return View(new Prescription());
        }

        // POST: DoctorPatient/CreatePrescription
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePrescription([Bind("PatientId,MedicationId,DosageInstructions,Duration,Instructions")] Prescription prescription)
        {
            var doctorId = GetCurrentDoctorId();
            
            if (doctorId == null)
            {
                // Return debug info instead of NotFound
                ViewBag.DebugInfo = new
                {
                    CurrentUser = User.Identity?.Name,
                    Message = "No doctor ID found - cannot create prescription"
                };
                
                // Return empty prescription form with debug info
                ViewBag.PatientId = new SelectList(new List<Patient>(), "Id", "FullName");
                ViewBag.MedicationId = new SelectList(new List<Medication>(), "Id", "Name");
                
                return View(prescription);
            }

            if (ModelState.IsValid)
            {
                prescription.DoctorId = doctorId.Value;
                prescription.PrescriptionDate = DateTime.Now;
                prescription.IsActive = true;

                _context.Add(prescription);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Prescription created successfully.";
                return RedirectToAction(nameof(ViewPatientFolder), new { id = prescription.PatientId });
            }

            ViewBag.PatientId = new SelectList(
                await _context.Patients
                    .Where(p => p.AssignedDoctorId == doctorId && p.IsCurrentlyAdmitted)
                    .ToListAsync(), 
                "Id", "FullName", prescription.PatientId);

            ViewBag.MedicationId = new SelectList(
                await _context.Medications
                    .Where(m => m.IsActive)
                    .ToListAsync(), 
                "Id", "Name");

            return View(prescription);
        }

        // GET: DoctorPatient/CreateInstruction - Create doctor instruction
        public async Task<IActionResult> CreateInstruction(int? patientId)
        {
            var doctorId = GetCurrentDoctorId();
            
            if (doctorId == null)
            {
                // Return debug info instead of NotFound
                ViewBag.DebugInfo = new
                {
                    CurrentUser = User.Identity?.Name,
                    Message = "No doctor ID found - cannot create instruction"
                };
                
                // Return empty instruction form with debug info
                ViewBag.PatientId = new SelectList(new List<Patient>(), "Id", "FullName");
                
                return View(new DoctorInstruction());
            }

            if (patientId.HasValue)
            {
                var patient = await _context.Patients
                    .Include(p => p.Ward)
                    .Include(p => p.Bed)
                    .FirstOrDefaultAsync(p => p.Id == patientId && p.AssignedDoctorId == doctorId);
                
                if (patient == null) return NotFound();
                
                ViewBag.Patient = patient;
            }

            ViewBag.PatientId = new SelectList(
                await _context.Patients
                    .Where(p => p.AssignedDoctorId == doctorId && p.IsCurrentlyAdmitted)
                    .ToListAsync(), 
                "Id", "FullName", patientId);

            return View(new DoctorInstruction());
        }

        // POST: DoctorPatient/CreateInstruction
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateInstruction([Bind("PatientId,InstructionType,Instructions,Priority,Status")] DoctorInstruction instruction)
        {
            var doctorId = GetCurrentDoctorId();
            
            if (doctorId == null)
            {
                // Return debug info instead of NotFound
                ViewBag.DebugInfo = new
                {
                    CurrentUser = User.Identity?.Name,
                    Message = "No doctor ID found - cannot create instruction"
                };
                
                // Return empty instruction form with debug info
                ViewBag.PatientId = new SelectList(new List<Patient>(), "Id", "FullName");
                
                return View(instruction);
            }

            if (ModelState.IsValid)
            {
                instruction.DoctorId = doctorId.Value;
                instruction.InstructionDate = DateTime.Now;
                instruction.IsActive = true;

                _context.Add(instruction);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Doctor instruction created successfully.";
                return RedirectToAction(nameof(ViewPatientFolder), new { id = instruction.PatientId });
            }

            ViewBag.PatientId = new SelectList(
                await _context.Patients
                    .Where(p => p.AssignedDoctorId == doctorId && p.IsCurrentlyAdmitted)
                    .ToListAsync(), 
                "Id", "FullName", instruction.PatientId);

            return View(instruction);
        }

        // GET: DoctorPatient/DischargePatient/5 - Discharge patient
        public async Task<IActionResult> DischargePatient(int? id)
        {
            if (id == null) return NotFound();

            var doctorId = GetCurrentDoctorId();
            if (doctorId == null) return NotFound();

            var patient = await _context.Patients
                .Include(p => p.Ward)
                .Include(p => p.Bed)
                .Include(p => p.AssignedDoctor)
                .FirstOrDefaultAsync(p => p.Id == id && p.AssignedDoctorId == doctorId && p.IsCurrentlyAdmitted);

            if (patient == null) return NotFound();

            return View(patient);
        }

        // POST: DoctorPatient/DischargePatient/5
        [HttpPost, ActionName("DischargePatient")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DischargePatientConfirmed(int id, string dischargeSummary)
        {
            var doctorId = GetCurrentDoctorId();
            if (doctorId == null) return NotFound();

            var patient = await _context.Patients
                .Include(p => p.Bed)
                .FirstOrDefaultAsync(p => p.Id == id && p.AssignedDoctorId == doctorId && p.IsCurrentlyAdmitted);

            if (patient == null) return NotFound();

            try
            {
                // Update patient status
                patient.PatientStatus = "Discharged";
                patient.DischargeDate = DateTime.Now;
                patient.DischargeSummary = dischargeSummary;

                // Free up the bed
                if (patient.Bed != null)
                {
                    patient.Bed.PatientId = null;
                    patient.BedId = null;
                }

                _context.Update(patient);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Patient discharged successfully.";
                return RedirectToAction(nameof(MyPatients));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error discharging patient: {ex.Message}");
                return View(patient);
            }
        }

        // Helper method to get current doctor ID
        private int? GetCurrentDoctorId()
        {
            var currentUser = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUser)) 
            {
                return null;
            }
            
            var doctor = _context.Staff
                .FirstOrDefault(s => s.Email == currentUser && s.Role == "Doctor" && s.IsActive);

            return doctor?.Id;
        }
    }
   
}
