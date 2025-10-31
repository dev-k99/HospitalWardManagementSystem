using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WardSystemProject.Models;
using WardSystemProject.Data;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace WardSystemProject.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorPatientController : Controller
    {
        private readonly WardSystemDBContext _context;
        private readonly ILogger<DoctorPatientController> _logger;
        private const string DoctorRole = "Doctor";

        public DoctorPatientController(WardSystemDBContext context, ILogger<DoctorPatientController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: DoctorPatient/Index - Doctor Dashboard
        public async Task<IActionResult> Index()
        {
            var doctorId = GetCurrentDoctorId();
            if (doctorId == null)
            {
                _logger.LogWarning("No doctor ID found for user {UserName}", User.Identity?.Name);
                ViewData["ErrorMessage"] = "Unable to load dashboard. Please contact support.";
                return View("Error", new ErrorViewModel { RequestId = "Doctor ID not found" });
            }

            var dashboard = new DoctorDashboardViewModel
            {
                TotalPatients = await _context.Patients.CountAsync(p => p.AssignedDoctorId == doctorId && p.PatientStatus != "Discharged"),
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
                _logger.LogWarning("No doctor ID found for user {UserName} while accessing MyPatients", User.Identity?.Name);
                ViewData["ErrorMessage"] = "Unable to load patient list. Please contact support.";
                return View("Error", new ErrorViewModel { RequestId = "Doctor ID not found" });
            }

            var patients = await _context.Patients
                .Include(p => p.Ward)
                .Include(p => p.Bed)
                .Include(p => p.AssignedDoctor)
                .Where(p => p.AssignedDoctorId == doctorId && p.PatientStatus != "Discharged")
                .OrderBy(p => p.Ward.Name)
                .ThenBy(p => p.Bed.BedNumber)
                .ToListAsync();

            return View(patients);
        }

        // GET: DoctorPatient/ViewPatientFolder/5 - Comprehensive patient folder
        public async Task<IActionResult> ViewPatientFolder(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Null patient ID provided for ViewPatientFolder");
                return NotFound();
            }

            var doctorId = GetCurrentDoctorId();
            if (doctorId == null)
            {
                _logger.LogWarning("No doctor ID found for user {UserName} while accessing patient folder {PatientId}", User.Identity?.Name, id);
                ViewData["ErrorMessage"] = "Unable to access patient folder. Please contact support.";
                return View("Error", new ErrorViewModel { RequestId = "Doctor ID not found" });
            }

            var patient = await _context.Patients
                .Include(p => p.Ward)
                .Include(p => p.Bed)
                .Include(p => p.AssignedDoctor)
                .Include(p => p.PatientAllergies)
                .Include(p => p.MedicalConditions)
                .Include(p => p.VitalSigns.Where(v => v.IsActive).OrderByDescending(v => v.RecordDate).Take(10))
                .Include(p => p.MedicationAdministrations.Where(m => m.IsActive).OrderByDescending(m => m.AdministrationDate).Take(10))
                .Include(p => p.DoctorInstructions.Where(i => i.IsActive).OrderByDescending(i => i.InstructionDate).Take(10))
                .Include(p => p.DoctorVisits.Where(v => v.IsActive).OrderByDescending(v => v.VisitDate).Take(10))
                .Include(p => p.Prescriptions.Where(p => p.IsActive).OrderByDescending(p => p.PrescriptionDate).Take(10))
                .FirstOrDefaultAsync(p => p.Id == id && p.AssignedDoctorId == doctorId);

            if (patient == null)
            {
                _logger.LogWarning("Patient {PatientId} not found or not assigned to doctor {DoctorId}", id, doctorId);
                return NotFound();
            }

            return View(patient);
        }

        // DoctorPatientController.cs
 

            // -----------------------------
            // CREATE VISIT
            // -----------------------------
            public async Task<IActionResult> CreateVisit(int? patientId)
            {
                try
                {
                    var doctorId = GetCurrentDoctorId();
                    if (doctorId == null)
                        throw new Exception("Doctor ID could not be determined.");

                    if (patientId.HasValue)
                    {
                        var patient = await _context.Patients
                            .Include(p => p.Ward)
                            .Include(p => p.Bed)
                            .FirstOrDefaultAsync(p => p.Id == patientId &&
                                                      p.AssignedDoctorId == doctorId &&
                                                      p.PatientStatus != "Discharged");

                        if (patient == null)
                            throw new Exception("Patient not found or not assigned to the current doctor.");

                        ViewBag.Patient = patient;
                    }

                    var patients = await _context.Patients
                        .Where(p => p.AssignedDoctorId == doctorId && p.PatientStatus != "Discharged")
                        .Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName })
                        .ToListAsync();

                    ViewBag.PatientId = new SelectList(patients, "Id", "FullName", patientId);
                    return View(new DoctorVisit());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in CreateVisit (GET). PatientId: {PatientId}", patientId);
                    TempData["ErrorMessage"] = "An error occurred while preparing the visit form.";
                    return View("Error", new ErrorViewModel { RequestId = Guid.NewGuid().ToString() });
                }
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> CreateVisit([Bind("PatientId,VisitType,Notes,NextVisitDate")] DoctorVisit visit)
            {
                try
                {
                    var doctorId = GetCurrentDoctorId();
                    if (doctorId == null)
                        throw new Exception("Doctor ID could not be determined.");

                    visit.DoctorId = doctorId.Value;
                    visit.VisitDate = DateTime.Now;
                    visit.IsActive = true;

                    _context.Add(visit);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Doctor visit recorded successfully.";
                    return RedirectToAction(nameof(ViewPatientFolder), new { id = visit.PatientId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while creating visit for patient {PatientId}", visit.PatientId);
                    TempData["ErrorMessage"] = "Unable to record doctor visit. Please try again or contact support.";
                    return RedirectToAction(nameof(CreateVisit), new { patientId = visit.PatientId });
                }
            }

            // -----------------------------
            // CREATE PRESCRIPTION
            // -----------------------------
            public async Task<IActionResult> CreatePrescription(int? patientId)
            {
                try
                {
                    var doctorId = GetCurrentDoctorId();
                    if (doctorId == null)
                        throw new Exception("Doctor ID could not be determined.");

                    if (patientId.HasValue)
                    {
                        var patient = await _context.Patients
                            .Include(p => p.Ward)
                            .Include(p => p.Bed)
                            .FirstOrDefaultAsync(p => p.Id == patientId &&
                                                      p.AssignedDoctorId == doctorId &&
                                                      p.PatientStatus != "Discharged");

                        if (patient == null)
                            throw new Exception("Patient not found or not assigned to the current doctor.");

                        ViewBag.Patient = patient;
                    }

                    ViewBag.PatientId = new SelectList(
                        await _context.Patients
                            .Where(p => p.AssignedDoctorId == doctorId && p.PatientStatus != "Discharged")
                            .Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName })
                            .ToListAsync(),
                        "Id", "FullName", patientId
                    );

                    ViewBag.MedicationId = new SelectList(
                        await _context.Medications
                            .Where(m => m.IsActive)
                            .Select(m => new { m.Id, m.Name })
                            .ToListAsync(),
                        "Id", "Name"
                    );

                    return View(new Prescription());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in CreatePrescription (GET). PatientId: {PatientId}", patientId);
                    TempData["ErrorMessage"] = "An error occurred while preparing the prescription form.";
                    return View("Error", new ErrorViewModel { RequestId = Guid.NewGuid().ToString() });
                }
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> CreatePrescription([Bind("PatientId,MedicationId,DosageInstructions,Duration,Instructions")] Prescription prescription)
            {
                try
                {
                    var doctorId = GetCurrentDoctorId();
                    if (doctorId == null)
                        throw new Exception("Doctor ID could not be determined.");

                    prescription.DoctorId = doctorId.Value;
                    prescription.PrescriptionDate = DateTime.Now;
                    prescription.IsActive = true;

                    _context.Add(prescription);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Prescription created successfully.";
                    return RedirectToAction(nameof(ViewPatientFolder), new { id = prescription.PatientId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating prescription for patient {PatientId}", prescription.PatientId);
                    TempData["ErrorMessage"] = "Unable to create prescription. Please verify your inputs and try again.";
                    return RedirectToAction(nameof(CreatePrescription), new { patientId = prescription.PatientId });
                }
            }

            // -----------------------------
            // CREATE INSTRUCTION
            // -----------------------------
            public async Task<IActionResult> CreateInstruction(int? patientId)
            {
                try
                {
                    var doctorId = GetCurrentDoctorId();
                    if (doctorId == null)
                        throw new Exception("Doctor ID could not be determined.");

                    if (patientId.HasValue)
                    {
                        var patient = await _context.Patients
                            .Include(p => p.Ward)
                            .Include(p => p.Bed)
                            .FirstOrDefaultAsync(p => p.Id == patientId &&
                                                      p.AssignedDoctorId == doctorId &&
                                                      p.PatientStatus != "Discharged");

                        if (patient == null)
                            throw new Exception("Patient not found or not assigned to the current doctor.");

                        ViewBag.Patient = patient;
                    }

                    ViewBag.PatientId = new SelectList(
                        await _context.Patients
                            .Where(p => p.AssignedDoctorId == doctorId && p.PatientStatus != "Discharged")
                            .Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName })
                            .ToListAsync(),
                        "Id", "FullName", patientId
                    );

                    return View(new DoctorInstruction());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in CreateInstruction (GET). PatientId: {PatientId}", patientId);
                    TempData["ErrorMessage"] = "An error occurred while preparing the instruction form.";
                    return View("Error", new ErrorViewModel { RequestId = Guid.NewGuid().ToString() });
                }
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> CreateInstruction([Bind("PatientId,InstructionType,Instructions,Priority,Status")] DoctorInstruction instruction)
            {
                try
                {
                    var doctorId = GetCurrentDoctorId();
                    if (doctorId == null)
                        throw new Exception("Doctor ID could not be determined.");

                    instruction.DoctorId = doctorId.Value;
                    instruction.InstructionDate = DateTime.Now;
                    instruction.IsActive = true;

                    _context.Add(instruction);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Doctor instruction created successfully.";
                    return RedirectToAction(nameof(ViewPatientFolder), new { id = instruction.PatientId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating instruction for patient {PatientId}", instruction.PatientId);
                    TempData["ErrorMessage"] = "Unable to create instruction. Please verify your inputs and try again.";
                    return RedirectToAction(nameof(CreateInstruction), new { patientId = instruction.PatientId });
                }
            }
        

        // GET: DoctorPatient/DischargePatient/5 - Discharge patient
        public async Task<IActionResult> DischargePatient(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Null patient ID provided for DischargePatient");
                return NotFound();
            }

            var doctorId = GetCurrentDoctorId();
            if (doctorId == null)
            {
                _logger.LogWarning("No doctor ID found for user {UserName} while accessing DischargePatient {PatientId}", User.Identity?.Name, id);
                ViewData["ErrorMessage"] = "Unable to discharge patient. Please contact support.";
                return View("Error", new ErrorViewModel { RequestId = "Doctor ID not found" });
            }

            var patient = await _context.Patients
                .Include(p => p.Ward)
                .Include(p => p.Bed)
                .Include(p => p.AssignedDoctor)
                .FirstOrDefaultAsync(p => p.Id == id && p.AssignedDoctorId == doctorId && p.PatientStatus != "Discharged");

            if (patient == null)
            {
                _logger.LogWarning("Patient {PatientId} not found or not assigned to doctor {DoctorId} for DischargePatient", id, doctorId);
                return NotFound();
            }

            return View(patient);
        }

        // POST: DoctorPatient/DischargePatient/5
        [HttpPost, ActionName("DischargePatient")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DischargePatientConfirmed(int id, string dischargeSummary)
        {
            var doctorId = GetCurrentDoctorId();
            if (doctorId == null)
            {
                _logger.LogWarning("No doctor ID found for user {UserName} while posting DischargePatient {PatientId}", User.Identity?.Name, id);
                ViewData["ErrorMessage"] = "Unable to discharge patient. Please contact support.";
                return View("Error", new ErrorViewModel { RequestId = "Doctor ID not found" });
            }

            var patient = await _context.Patients
                .Include(p => p.Bed)
                .FirstOrDefaultAsync(p => p.Id == id && p.AssignedDoctorId == doctorId && p.PatientStatus != "Discharged");

            if (patient == null)
            {
                _logger.LogWarning("Patient {PatientId} not found or not assigned to doctor {DoctorId} for DischargePatientConfirmed", id, doctorId);
                return NotFound();
            }

            try
            {
                patient.PatientStatus = "Discharged";
                patient.DischargeDate = DateTime.Now;
                patient.DischargeSummary = dischargeSummary ?? string.Empty;

                if (patient.BedId.HasValue)
                {
                    var bed = await _context.Beds.FindAsync(patient.BedId);
                    if (bed != null) bed.PatientId = null;
                    patient.BedId = null;
                }

                _context.Update(patient);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Patient {PatientId} discharged by doctor {DoctorId}", id, doctorId);
                TempData["SuccessMessage"] = "Patient discharged successfully.";
                return RedirectToAction(nameof(MyPatients));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error discharging patient {PatientId} by doctor {DoctorId}", id, doctorId);
                ModelState.AddModelError("", $"Error discharging patient: {ex.Message}");
                return View(patient);
            }
        }

        private int? GetCurrentDoctorId()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                _logger.LogWarning("No email claim found for current user");
                return null;
            }

            var doctor = _context.Staff
                .FirstOrDefault(s => s.Email == userEmail && s.Role == DoctorRole && s.IsActive);

            if (doctor == null)
            {
                _logger.LogWarning("No active doctor found for email {Email}", userEmail);
            }

            return doctor?.Id;
        }
    }
}


//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using WardSystemProject.Models;
//using WardSystemProject.Data;
//using System.Security.Claims;
//using Microsoft.Extensions.Logging;

//namespace WardSystemProject.Controllers
//{
//    [Authorize(Roles = "Doctor")]
//    public class DoctorPatientController : Controller
//    {
//        private readonly WardSystemDBContext _context;
//        private readonly ILogger<DoctorPatientController> _logger;
//        private const string DoctorRole = "Doctor";

//        public DoctorPatientController(WardSystemDBContext context, ILogger<DoctorPatientController> logger)
//        {
//            _context = context ?? throw new ArgumentNullException(nameof(context));
//            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//        }

//        // GET: DoctorPatient/Index - Doctor Dashboard
//        public async Task<IActionResult> Index()
//        {
//            var doctorId = GetCurrentDoctorId();
//            if (doctorId == null)
//            {
//                _logger.LogWarning("No doctor ID found for user {UserName}", User.Identity?.Name);
//                ViewData["ErrorMessage"] = "Unable to load dashboard. Please contact support.";
//                return View("Error", new ErrorViewModel { RequestId = "Doctor ID not found" });
//            }

//            var dashboard = new DoctorDashboardViewModel
//            {
//                TotalPatients = await _context.Patients.CountAsync(p => p.AssignedDoctorId == doctorId && p.PatientStatus != "Discharged"),
//                RecentVisits = await _context.DoctorVisits
//                    .Include(v => v.Patient)
//                    .Where(v => v.DoctorId == doctorId && v.VisitDate >= DateTime.Today.AddDays(-7))
//                    .OrderByDescending(v => v.VisitDate)
//                    .Take(5)
//                    .ToListAsync(),
//                PendingInstructions = await _context.DoctorInstructions
//                    .Include(i => i.Patient)
//                    .Where(i => i.DoctorId == doctorId && i.Status == "Pending")
//                    .OrderByDescending(i => i.InstructionDate)
//                    .Take(5)
//                    .ToListAsync()
//            };

//            return View(dashboard);
//        }

//        // GET: DoctorPatient/MyPatients - List of assigned patients
//        public async Task<IActionResult> MyPatients()
//        {
//            var doctorId = GetCurrentDoctorId();
//            if (doctorId == null)
//            {
//                _logger.LogWarning("No doctor ID found for user {UserName} while accessing MyPatients", User.Identity?.Name);
//                ViewData["ErrorMessage"] = "Unable to load patient list. Please contact support.";
//                return View("Error", new ErrorViewModel { RequestId = "Doctor ID not found" });
//            }

//            var patients = await _context.Patients
//                .Include(p => p.Ward)
//                .Include(p => p.Bed)
//                .Include(p => p.AssignedDoctor)
//                .Where(p => p.AssignedDoctorId == doctorId && p.PatientStatus != "Discharged")
//                .OrderBy(p => p.Ward.Name)
//                .ThenBy(p => p.Bed.BedNumber)
//                .ToListAsync();

//            return View(patients);
//        }

//        // GET: DoctorPatient/ViewPatientFolder/5 - Comprehensive patient folder
//        public async Task<IActionResult> ViewPatientFolder(int? id)
//        {
//            if (id == null)
//            {
//                _logger.LogWarning("Null patient ID provided for ViewPatientFolder");
//                return NotFound();
//            }

//            var doctorId = GetCurrentDoctorId();
//            if (doctorId == null)
//            {
//                _logger.LogWarning("No doctor ID found for user {UserName} while accessing patient folder {PatientId}", User.Identity?.Name, id);
//                ViewData["ErrorMessage"] = "Unable to access patient folder. Please contact support.";
//                return View("Error", new ErrorViewModel { RequestId = "Doctor ID not found" });
//            }

//            var patient = await _context.Patients
//                .Include(p => p.Ward)
//                .Include(p => p.Bed)
//                .Include(p => p.AssignedDoctor)
//                .Include(p => p.PatientAllergies)
//                .Include(p => p.MedicalConditions)
//                .Include(p => p.VitalSigns.Where(v => v.IsActive).OrderByDescending(v => v.RecordDate).Take(10))
//                .Include(p => p.MedicationAdministrations.Where(m => m.IsActive).OrderByDescending(m => m.AdministrationDate).Take(10))
//                .Include(p => p.DoctorInstructions.Where(i => i.IsActive).OrderByDescending(i => i.InstructionDate).Take(10))
//                .Include(p => p.DoctorVisits.Where(v => v.IsActive).OrderByDescending(v => v.VisitDate).Take(10))
//                .Include(p => p.Prescriptions.Where(p => p.IsActive).OrderByDescending(p => p.PrescriptionDate).Take(10))
//                .FirstOrDefaultAsync(p => p.Id == id && p.AssignedDoctorId == doctorId);

//            if (patient == null)
//            {
//                _logger.LogWarning("Patient {PatientId} not found or not assigned to doctor {DoctorId}", id, doctorId);
//                return NotFound();
//            }

//            return View(patient);
//        }

//        // GET: DoctorPatient/CreateVisit - Record a new doctor visit
//        public async Task<IActionResult> CreateVisit(int? patientId)
//        {
//            var doctorId = GetCurrentDoctorId();
//            if (doctorId == null)
//            {
//                _logger.LogWarning("No doctor ID found for user {UserName} while accessing CreateVisit", User.Identity?.Name);
//                ViewData["ErrorMessage"] = "Unable to create visit. Please contact support.";
//                return View("Error", new ErrorViewModel { RequestId = "Doctor ID not found" });
//            }

//            if (patientId.HasValue)
//            {
//                var patient = await _context.Patients
//                    .Include(p => p.Ward)
//                    .Include(p => p.Bed)
//                    .FirstOrDefaultAsync(p => p.Id == patientId && p.AssignedDoctorId == doctorId && p.PatientStatus != "Discharged");

//                if (patient == null)
//                {
//                    _logger.LogWarning("Patient {PatientId} not found or not assigned to doctor {DoctorId} for CreateVisit", patientId, doctorId);
//                    return NotFound();
//                }

//                ViewBag.Patient = patient;
//            }

//            var patients = await _context.Patients
//                .Where(p => p.AssignedDoctorId == doctorId && p.PatientStatus != "Discharged")
//                .Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName })
//                .ToListAsync();

//            ViewBag.PatientId = new SelectList(patients, "Id", "FullName", patientId);
//            return View(new DoctorVisit());
//        }

//        // POST: DoctorPatient/CreateVisit
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> CreateVisit([Bind("PatientId,VisitType,Notes,NextVisitDate")] DoctorVisit visit)
//        {
//            var doctorId = GetCurrentDoctorId();
//            if (doctorId == null)
//            {
//                _logger.LogWarning("No doctor ID found for user {UserName} while posting CreateVisit", User.Identity?.Name);
//                ViewData["ErrorMessage"] = "Unable to create visit. Please contact support.";
//                return View("Error", new ErrorViewModel { RequestId = "Doctor ID not found" });
//            }

//            // Validate patient exists and is assigned to the doctor
//            var patient = await _context.Patients
//                .FirstOrDefaultAsync(p => p.Id == visit.PatientId && p.AssignedDoctorId == doctorId && p.PatientStatus != "Discharged");
//            if (patient == null)
//            {
//                _logger.LogWarning("Patient {PatientId} not found or not assigned to doctor {DoctorId} for CreateVisit", visit.PatientId, doctorId);
//                ModelState.AddModelError("PatientId", "Invalid or unassigned patient.");
//            }

//            if (ModelState.IsValid)
//            {
//                visit.DoctorId = doctorId.Value;
//                visit.VisitDate = DateTime.Now;
//                visit.IsActive = true;

//                _context.Add(visit);
//                await _context.SaveChangesAsync();

//                TempData["SuccessMessage"] = "Doctor visit recorded successfully.";
//                return RedirectToAction(nameof(ViewPatientFolder), new { id = visit.PatientId });
//            }

//            var patients = await _context.Patients
//                .Where(p => p.AssignedDoctorId == doctorId && p.PatientStatus != "Discharged")
//                .Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName })
//                .ToListAsync();

//            ViewBag.PatientId = new SelectList(patients, "Id", "FullName", visit.PatientId);
//            return View(visit);
//        }

//        // GET: DoctorPatient/CreatePrescription - Prescribe medication
//        public async Task<IActionResult> CreatePrescription(int? patientId)
//        {
//            var doctorId = GetCurrentDoctorId();
//            if (doctorId == null)
//            {
//                _logger.LogWarning("No doctor ID found for user {UserName} while accessing CreatePrescription", User.Identity?.Name);
//                ViewData["ErrorMessage"] = "Unable to create prescription. Please contact support.";
//                return View("Error", new ErrorViewModel { RequestId = "Doctor ID not found" });
//            }

//            if (patientId.HasValue)
//            {
//                var patient = await _context.Patients
//                    .Include(p => p.Ward)
//                    .Include(p => p.Bed)
//                    .FirstOrDefaultAsync(p => p.Id == patientId && p.AssignedDoctorId == doctorId && p.PatientStatus != "Discharged");

//                if (patient == null)
//                {
//                    _logger.LogWarning("Patient {PatientId} not found or not assigned to doctor {DoctorId} for CreatePrescription", patientId, doctorId);
//                    return NotFound();
//                }

//                ViewBag.Patient = patient;
//            }

//            var patients = await _context.Patients
//                .Where(p => p.AssignedDoctorId == doctorId && p.PatientStatus != "Discharged")
//                .Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName })
//                .ToListAsync();

//            var medications = await _context.Medications
//                .Where(m => m.IsActive)
//                .Select(m => new { m.Id, m.Name })
//                .ToListAsync();

//            ViewBag.PatientId = new SelectList(patients, "Id", "FullName", patientId);
//            ViewBag.MedicationId = new SelectList(medications, "Id", "Name");
//            return View(new Prescription());
//        }

//        // POST: DoctorPatient/CreatePrescription
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> CreatePrescription([Bind("PatientId,MedicationId,DosageInstructions,Duration,Instructions")] Prescription prescription)
//        {
//            var doctorId = GetCurrentDoctorId();
//            if (doctorId == null)
//            {
//                _logger.LogWarning("No doctor ID found for user {UserName} while posting CreatePrescription", User.Identity?.Name);
//                ViewData["ErrorMessage"] = "Unable to create prescription. Please contact support.";
//                return View("Error", new ErrorViewModel { RequestId = "Doctor ID not found" });
//            }

//            // Validate patient and medication
//            var patient = await _context.Patients
//                .FirstOrDefaultAsync(p => p.Id == prescription.PatientId && p.AssignedDoctorId == doctorId && p.PatientStatus != "Discharged");
//            if (patient == null)
//            {
//                _logger.LogWarning("Patient {PatientId} not found or not assigned to doctor {DoctorId} for CreatePrescription", prescription.PatientId, doctorId);
//                ModelState.AddModelError("PatientId", "Invalid or unassigned patient.");
//            }

//            var medication = await _context.Medications
//                .FirstOrDefaultAsync(m => m.Id == prescription.MedicationId && m.IsActive);
//            if (medication == null)
//            {
//                _logger.LogWarning("Medication {MedicationId} not found or inactive for CreatePrescription", prescription.MedicationId);
//                ModelState.AddModelError("MedicationId", "Invalid or inactive medication.");
//            }

//            if (ModelState.IsValid)
//            {
//                prescription.DoctorId = doctorId.Value;
//                prescription.PrescriptionDate = DateTime.Now;
//                prescription.IsActive = true;

//                _context.Add(prescription);
//                await _context.SaveChangesAsync();

//                TempData["SuccessMessage"] = "Prescription created successfully.";
//                return RedirectToAction(nameof(ViewPatientFolder), new { id = prescription.PatientId });
//            }

//            var patients = await _context.Patients
//                .Where(p => p.AssignedDoctorId == doctorId && p.PatientStatus != "Discharged")
//                .Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName })
//                .ToListAsync();

//            var medications = await _context.Medications
//                .Where(m => m.IsActive)
//                .Select(m => new { m.Id, m.Name })
//                .ToListAsync();

//            ViewBag.PatientId = new SelectList(patients, "Id", "FullName", prescription.PatientId);
//            ViewBag.MedicationId = new SelectList(medications, "Id", "Name", prescription.MedicationId);
//            return View(prescription);
//        }

//        // GET: DoctorPatient/CreateInstruction - Create doctor instruction
//        public async Task<IActionResult> CreateInstruction(int? patientId)
//        {
//            var doctorId = GetCurrentDoctorId();
//            if (doctorId == null)
//            {
//                _logger.LogWarning("No doctor ID found for user {UserName} while accessing CreateInstruction", User.Identity?.Name);
//                ViewData["ErrorMessage"] = "Unable to create instruction. Please contact support.";
//                return View("Error", new ErrorViewModel { RequestId = "Doctor ID not found" });
//            }

//            if (patientId.HasValue)
//            {
//                var patient = await _context.Patients
//                    .Include(p => p.Ward)
//                    .Include(p => p.Bed)
//                    .FirstOrDefaultAsync(p => p.Id == patientId && p.AssignedDoctorId == doctorId && p.PatientStatus != "Discharged");

//                if (patient == null)
//                {
//                    _logger.LogWarning("Patient {PatientId} not found or not assigned to doctor {DoctorId} for CreateInstruction", patientId, doctorId);
//                    return NotFound();
//                }

//                ViewBag.Patient = patient;
//            }

//            var patients = await _context.Patients
//                .Where(p => p.AssignedDoctorId == doctorId && p.PatientStatus != "Discharged")
//                .Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName })
//                .ToListAsync();

//            ViewBag.PatientId = new SelectList(patients, "Id", "FullName", patientId);
//            return View(new DoctorInstruction());
//        }

//        // POST: DoctorPatient/CreateInstruction
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> CreateInstruction([Bind("PatientId,InstructionType,Instructions,Priority,Status")] DoctorInstruction instruction)
//        {
//            var doctorId = GetCurrentDoctorId();
//            if (doctorId == null)
//            {
//                _logger.LogWarning("No doctor ID found for user {UserName} while posting CreateInstruction", User.Identity?.Name);
//                ViewData["ErrorMessage"] = "Unable to create instruction. Please contact support.";
//                return View("Error", new ErrorViewModel { RequestId = "Doctor ID not found" });
//            }

//            // Validate patient
//            var patient = await _context.Patients
//                .FirstOrDefaultAsync(p => p.Id == instruction.PatientId && p.AssignedDoctorId == doctorId && p.PatientStatus != "Discharged");
//            if (patient == null)
//            {
//                _logger.LogWarning("Patient {PatientId} not found or not assigned to doctor {DoctorId} for CreateInstruction", instruction.PatientId, doctorId);
//                ModelState.AddModelError("PatientId", "Invalid or unassigned patient.");
//            }

//            if (ModelState.IsValid)
//            {
//                instruction.DoctorId = doctorId.Value;
//                instruction.InstructionDate = DateTime.Now;
//                instruction.IsActive = true;

//                _context.Add(instruction);
//                await _context.SaveChangesAsync();

//                TempData["SuccessMessage"] = "Doctor instruction created successfully.";
//                return RedirectToAction(nameof(ViewPatientFolder), new { id = instruction.PatientId });
//            }

//            var patients = await _context.Patients
//                .Where(p => p.AssignedDoctorId == doctorId && p.PatientStatus != "Discharged")
//                .Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName })
//                .ToListAsync();

//            ViewBag.PatientId = new SelectList(patients, "Id", "FullName", instruction.PatientId);
//            return View(instruction);
//        }

//        // GET: DoctorPatient/DischargePatient/5 - Discharge patient
//        public async Task<IActionResult> DischargePatient(int? id)
//        {
//            if (id == null)
//            {
//                _logger.LogWarning("Null patient ID provided for DischargePatient");
//                return NotFound();
//            }

//            var doctorId = GetCurrentDoctorId();
//            if (doctorId == null)
//            {
//                _logger.LogWarning("No doctor ID found for user {UserName} while accessing DischargePatient {PatientId}", User.Identity?.Name, id);
//                ViewData["ErrorMessage"] = "Unable to discharge patient. Please contact support.";
//                return View("Error", new ErrorViewModel { RequestId = "Doctor ID not found" });
//            }

//            var patient = await _context.Patients
//                .Include(p => p.Ward)
//                .Include(p => p.Bed)
//                .Include(p => p.AssignedDoctor)
//                .FirstOrDefaultAsync(p => p.Id == id && p.AssignedDoctorId == doctorId && p.PatientStatus != "Discharged");

//            if (patient == null)
//            {
//                _logger.LogWarning("Patient {PatientId} not found or not assigned to doctor {DoctorId} for DischargePatient", id, doctorId);
//                return NotFound();
//            }

//            return View(patient);
//        }

//        // POST: DoctorPatient/DischargePatient/5
//        [HttpPost, ActionName("DischargePatient")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DischargePatientConfirmed(int id, string dischargeSummary)
//        {
//            var doctorId = GetCurrentDoctorId();
//            if (doctorId == null)
//            {
//                _logger.LogWarning("No doctor ID found for user {UserName} while posting DischargePatient {PatientId}", User.Identity?.Name, id);
//                ViewData["ErrorMessage"] = "Unable to discharge patient. Please contact support.";
//                return View("Error", new ErrorViewModel { RequestId = "Doctor ID not found" });
//            }

//            var patient = await _context.Patients
//                .Include(p => p.Bed)
//                .FirstOrDefaultAsync(p => p.Id == id && p.AssignedDoctorId == doctorId && p.PatientStatus != "Discharged");

//            if (patient == null)
//            {
//                _logger.LogWarning("Patient {PatientId} not found or not assigned to doctor {DoctorId} for DischargePatientConfirmed", id, doctorId);
//                return NotFound();
//            }

//            try
//            {
//                // Update patient status
//                patient.PatientStatus = "Discharged";
//                patient.DischargeDate = DateTime.Now;
//                patient.DischargeSummary = dischargeSummary ?? string.Empty;

//                // Free up the bed if assigned
//                if (patient.BedId.HasValue && patient.Bed != null)
//                {
//                    patient.Bed.PatientId = null;
//                    patient.BedId = null;
//                }

//                _context.Update(patient);
//                await _context.SaveChangesAsync();

//                _logger.LogInformation("Patient {PatientId} discharged by doctor {DoctorId}", id, doctorId);
//                TempData["SuccessMessage"] = "Patient discharged successfully.";
//                return RedirectToAction(nameof(MyPatients));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error discharging patient {PatientId} by doctor {DoctorId}", id, doctorId);
//                ModelState.AddModelError("", $"Error discharging patient: {ex.Message}");
//                return View(patient);
//            }
//        }

//        // Helper method to get current doctor ID
//        private int? GetCurrentDoctorId()
//        {
//            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
//            if (string.IsNullOrEmpty(userEmail))
//            {
//                _logger.LogWarning("No email claim found for current user");
//                return null;
//            }

//            var doctor = _context.Staff
//                .FirstOrDefault(s => s.Email == userEmail && s.Role == DoctorRole && s.IsActive);

//            if (doctor == null)
//            {
//                _logger.LogWarning("No active doctor found for email {Email}", userEmail);
//            }

//            return doctor?.Id;
//        }
//    }
//}
