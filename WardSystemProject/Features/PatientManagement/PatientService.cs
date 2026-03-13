using Microsoft.EntityFrameworkCore;
using WardSystemProject.Core.Interfaces;
using WardSystemProject.Data;
using WardSystemProject.Models;
using WardSystemProject.ViewModels;

namespace WardSystemProject.Features.PatientManagement
{
    /// <summary>
    /// All patient lifecycle business logic lives here — not in the controller.
    /// This makes it independently testable and reusable across controllers/APIs.
    /// </summary>
    public sealed class PatientService : IPatientService
    {
        private readonly WardSystemDBContext _db;
        private readonly ILogger<PatientService> _logger;

        public PatientService(WardSystemDBContext db, ILogger<PatientService> logger)
        {
            _db     = db;
            _logger = logger;
        }

        public async Task<IEnumerable<Patient>> GetAllActiveAsync() =>
            await _db.Patients
                .Include(p => p.Ward)
                .Include(p => p.Bed)
                .Include(p => p.AssignedDoctor)
                .OrderByDescending(p => p.AdmissionDate)
                .ToListAsync();   // IsActive filter applied by global query filter

        public async Task<IEnumerable<Patient>> SearchAsync(string query)
        {
            var q = query.Trim().ToLower();
            return await _db.Patients
                .Include(p => p.Ward)
                .Include(p => p.Bed)
                .Include(p => p.AssignedDoctor)
                .Where(p => p.FirstName.ToLower().Contains(q) ||
                            p.LastName.ToLower().Contains(q)  ||
                            p.ContactNumber.Contains(q))
                .OrderByDescending(p => p.AdmissionDate)
                .ToListAsync();
        }

        public async Task<Patient?> GetByIdAsync(int id) =>
            await _db.Patients
                .Include(p => p.Ward)
                .Include(p => p.Bed)
                .Include(p => p.AssignedDoctor)
                .Include(p => p.PatientAllergies)
                .Include(p => p.MedicalConditions)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<Patient?> GetFullFolderAsync(int id) =>
            await _db.Patients
                .Include(p => p.Ward)
                .Include(p => p.Bed)
                .Include(p => p.AssignedDoctor)
                .Include(p => p.PatientAllergies)
                .Include(p => p.MedicalConditions)
                .Include(p => p.PatientMovements).ThenInclude(m => m.FromWard)
                .Include(p => p.PatientMovements).ThenInclude(m => m.ToWard)
                .Include(p => p.DoctorVisits).ThenInclude(dv => dv.Doctor)
                .Include(p => p.Prescriptions).ThenInclude(pr => pr.Medication)
                .Include(p => p.Prescriptions).ThenInclude(pr => pr.Doctor)
                .Include(p => p.VitalSigns)
                .Include(p => p.MedicationAdministrations).ThenInclude(ma => ma.Medication)
                .Include(p => p.DoctorInstructions).ThenInclude(di => di.Doctor)
                .FirstOrDefaultAsync(p => p.Id == id);

        /// <summary>
        /// BUG FIX: In the original PatientManagementController.Admit() there was a
        /// premature <c>return RedirectToAction</c> at line 82 that made the bed assignment,
        /// PatientFolder creation, and movement recording unreachable (dead code).
        /// This implementation executes all steps correctly in a single transaction.
        /// </summary>
        public async Task<Patient> AdmitAsync(AdmitPatientViewModel vm, string admittedBy)
        {
            // Verify the selected bed is still available
            var bed = await _db.Beds.FindAsync(vm.BedId)
                      ?? throw new InvalidOperationException("Selected bed does not exist.");

            if (bed.PatientId.HasValue)
                throw new InvalidOperationException("The selected bed is already occupied. Please choose another bed.");

            // Build the patient record
            var patient = new Patient
            {
                FirstName              = vm.FirstName,
                LastName               = vm.LastName,
                DateOfBirth            = vm.DateOfBirth,
                Gender                 = vm.Gender,
                ContactNumber          = vm.ContactNumber,
                EmergencyContact       = vm.EmergencyContact,
                EmergencyContactNumber = vm.EmergencyContactNumber,
                Address                = vm.Address,
                NextOfKin              = vm.NextOfKin,
                NextOfKinContact       = vm.NextOfKinContact,
                BloodType              = vm.BloodType,
                ChronicMedications     = vm.ChronicMedications,
                MedicalHistory         = vm.MedicalHistory,
                Allergies              = vm.Allergies,
                AdmissionReason        = vm.AdmissionReason,
                AdmissionDate          = vm.AdmissionDate,
                WardId                 = vm.WardId,
                BedId                  = vm.BedId,
                AssignedDoctorId       = vm.AssignedDoctorId,
                PatientStatus          = "Admitted",
                IsActive               = true
            };

            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.Patients.Add(patient);
                await _db.SaveChangesAsync();   // generates patient.Id

                // Assign the bed
                bed.PatientId = patient.Id;
                _db.Beds.Update(bed);

                // Create the patient folder (admission record)
                _db.PatientFolders.Add(new PatientFolder
                {
                    PatientId        = patient.Id,
                    AdmissionDate    = vm.AdmissionDate,
                    AssignedDoctorId = vm.AssignedDoctorId,
                    WardId           = vm.WardId,
                    BedId            = vm.BedId,
                    PatientStatus    = "Admitted"
                });

                // Record initial ward movement (external → ward)
                _db.PatientMovements.Add(new PatientMovement
                {
                    PatientId      = patient.Id,
                    FromWardId     = vm.WardId,   // admission from external — use target ward as both
                    ToWardId       = vm.WardId,
                    MovementDate   = vm.AdmissionDate,
                    MovementType   = "Ward Transfer",
                    MovementReason = $"Initial admission to ward. Admitted by: {admittedBy}",
                    RecordedBy     = admittedBy,
                    IsActive       = true
                });

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Patient {PatientId} ({Name}) admitted to Ward {WardId}, Bed {BedId} by {User}",
                    patient.Id, patient.FullName, vm.WardId, vm.BedId, admittedBy);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return patient;
        }

        public async Task UpdateAsync(int id, EditPatientViewModel vm)
        {
            var existing = await _db.Patients
                .Include(p => p.Bed)
                .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new KeyNotFoundException($"Patient {id} not found.");

            // Handle bed reassignment
            if (existing.BedId != vm.BedId)
            {
                // Free old bed
                if (existing.BedId.HasValue)
                {
                    var oldBed = await _db.Beds.FindAsync(existing.BedId);
                    if (oldBed != null) oldBed.PatientId = null;
                }

                // Claim new bed (validate it is free)
                if (vm.BedId.HasValue)
                {
                    var newBed = await _db.Beds.FindAsync(vm.BedId)
                                 ?? throw new InvalidOperationException("New bed not found.");
                    if (newBed.PatientId.HasValue && newBed.PatientId != id)
                        throw new InvalidOperationException("The selected bed is already occupied.");
                    newBed.PatientId = id;
                }
            }

            existing.FirstName              = vm.FirstName;
            existing.LastName               = vm.LastName;
            existing.DateOfBirth            = vm.DateOfBirth;
            existing.Gender                 = vm.Gender;
            existing.ContactNumber          = vm.ContactNumber;
            existing.EmergencyContact       = vm.EmergencyContact;
            existing.EmergencyContactNumber = vm.EmergencyContactNumber;
            existing.Address                = vm.Address;
            existing.NextOfKin              = vm.NextOfKin;
            existing.NextOfKinContact       = vm.NextOfKinContact;
            existing.BloodType              = vm.BloodType;
            existing.ChronicMedications     = vm.ChronicMedications;
            existing.MedicalHistory         = vm.MedicalHistory;
            existing.Allergies              = vm.Allergies;
            existing.WardId                 = vm.WardId;
            existing.BedId                  = vm.BedId;
            existing.AssignedDoctorId       = vm.AssignedDoctorId;
            existing.PatientStatus          = vm.PatientStatus;

            await _db.SaveChangesAsync();
        }

        public async Task DischargeAsync(int id, DischargePatientViewModel vm, string dischargedBy)
        {
            var patient = await _db.Patients
                .Include(p => p.Bed)
                .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new KeyNotFoundException($"Patient {id} not found.");

            if (patient.PatientStatus == "Discharged")
                throw new InvalidOperationException("Patient is already discharged.");

            // Free the bed
            if (patient.BedId.HasValue)
            {
                var bed = await _db.Beds.FindAsync(patient.BedId);
                if (bed != null) bed.PatientId = null;
            }

            patient.DischargeDate    = vm.DischargeDate;
            patient.DischargeSummary = vm.DischargeSummary;
            patient.PatientStatus    = "Discharged";
            patient.BedId            = null;
            patient.WardId           = null;
            patient.AssignedDoctorId = null;

            // Update the patient folder
            var folder = await _db.PatientFolders.FirstOrDefaultAsync(f => f.PatientId == id);
            if (folder != null)
            {
                folder.DischargeDate    = vm.DischargeDate;
                folder.DischargeSummary = vm.DischargeSummary;
                folder.PatientStatus    = "Discharged";
            }

            await _db.SaveChangesAsync();

            _logger.LogInformation("Patient {PatientId} ({Name}) discharged by {User}",
                id, patient.FullName, dischargedBy);
        }

        public async Task SoftDeleteAsync(int id)
        {
            var patient = await _db.Patients.FindAsync(id)
                          ?? throw new KeyNotFoundException($"Patient {id} not found.");

            if (patient.BedId.HasValue)
            {
                var bed = await _db.Beds.FindAsync(patient.BedId);
                if (bed != null) bed.PatientId = null;
            }

            patient.IsActive = false;
            await _db.SaveChangesAsync();
        }
    }
}
