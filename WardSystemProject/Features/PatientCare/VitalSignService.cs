using Microsoft.EntityFrameworkCore;
using WardSystemProject.Core.Interfaces;
using WardSystemProject.Data;
using WardSystemProject.Models;
using WardSystemProject.ViewModels;

namespace WardSystemProject.Features.PatientCare
{
    /// <summary>
    /// Vital sign recording and medication administration logic.
    ///
    /// Business rule (from spec):
    ///   "A nurse is only allowed to dispense medication up to schedule 4.
    ///    Only a Nursing Sister may dispense any schedule 5 (or higher) medication."
    /// This rule is enforced here in the service — NOT in the controller.
    /// The service is unit-testable independently of the HTTP pipeline.
    /// </summary>
    public sealed class VitalSignService : IVitalSignService
    {
        private readonly WardSystemDBContext _db;
        private readonly ILogger<VitalSignService> _logger;

        public VitalSignService(WardSystemDBContext db, ILogger<VitalSignService> logger)
        {
            _db     = db;
            _logger = logger;
        }

        // ── Vital Signs ───────────────────────────────────────────────────────

        public async Task<IEnumerable<VitalSign>> GetAllAsync() =>
            await _db.VitalSigns
                .Include(v => v.Patient).ThenInclude(p => p.Ward)
                .OrderByDescending(v => v.RecordDate)
                .ToListAsync();

        public async Task<VitalSign?> GetByIdAsync(int id) =>
            await _db.VitalSigns
                .Include(v => v.Patient).ThenInclude(p => p.Ward)
                .FirstOrDefaultAsync(v => v.Id == id);

        public async Task<VitalSign> RecordAsync(RecordVitalSignViewModel vm, string recordedBy)
        {
            var vitalSign = new VitalSign
            {
                PatientId        = vm.PatientId,
                Temperature      = vm.Temperature,
                Pulse            = vm.Pulse,
                BloodPressure    = vm.BloodPressure,
                HeartRate        = vm.HeartRate,
                RespiratoryRate  = vm.RespiratoryRate,
                OxygenSaturation = vm.OxygenSaturation,
                Notes            = vm.Notes,
                RecordDate       = DateTime.UtcNow,
                RecordedBy       = recordedBy,
                IsActive         = true
            };

            _db.VitalSigns.Add(vitalSign);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Vital signs recorded for Patient {PatientId} by {User}", vm.PatientId, recordedBy);
            return vitalSign;
        }

        public async Task UpdateAsync(int id, RecordVitalSignViewModel vm)
        {
            var existing = await _db.VitalSigns.FindAsync(id)
                           ?? throw new KeyNotFoundException($"VitalSign {id} not found.");

            existing.Temperature      = vm.Temperature;
            existing.Pulse            = vm.Pulse;
            existing.BloodPressure    = vm.BloodPressure;
            existing.HeartRate        = vm.HeartRate;
            existing.RespiratoryRate  = vm.RespiratoryRate;
            existing.OxygenSaturation = vm.OxygenSaturation;
            existing.Notes            = vm.Notes;

            await _db.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(int id)
        {
            var vs = await _db.VitalSigns.FindAsync(id)
                     ?? throw new KeyNotFoundException($"VitalSign {id} not found.");
            vs.IsActive = false;
            await _db.SaveChangesAsync();
        }

        // ── Medication Administration ─────────────────────────────────────────

        public async Task<IEnumerable<MedicationAdministration>> GetAllAdministrationsAsync() =>
            await _db.MedicationAdministrations
                .Include(m => m.Patient).ThenInclude(p => p.Ward)
                .Include(m => m.Medication)
                .OrderByDescending(m => m.AdministrationDate)
                .ToListAsync();

        public async Task<MedicationAdministration?> GetAdministrationByIdAsync(int id) =>
            await _db.MedicationAdministrations
                .Include(m => m.Patient).ThenInclude(p => p.Ward)
                .Include(m => m.Medication)
                .FirstOrDefaultAsync(m => m.Id == id);

        /// <summary>
        /// Enforces the Schedule 4 / Schedule 5+ medication rule from the spec.
        /// Throws <see cref="UnauthorizedAccessException"/> when a Nurse (not Sister)
        /// tries to administer a Schedule 5+ medication.
        /// </summary>
        public async Task<MedicationAdministration> AdministerAsync(
            AdministerMedicationViewModel vm,
            string administeredBy,
            bool isNursingSister)
        {
            var medication = await _db.Medications.FindAsync(vm.MedicationId)
                             ?? throw new KeyNotFoundException("Medication not found.");

            // ── CRITICAL BUSINESS RULE ────────────────────────────────────────
            if (medication.Schedule > 4 && !isNursingSister)
            {
                throw new UnauthorizedAccessException(
                    $"'{medication.Name}' is a Schedule {medication.Schedule} medication. " +
                    "Only a Nursing Sister may administer Schedule 5 or higher medications. " +
                    "Please contact the Nursing Sister on duty.");
            }

            var record = new MedicationAdministration
            {
                PatientId            = vm.PatientId,
                MedicationId         = vm.MedicationId,
                Dosage               = vm.Dosage,
                AdministrationMethod = vm.AdministrationMethod,
                Notes                = vm.Notes,
                AdministrationDate   = DateTime.UtcNow,
                AdministeredBy       = administeredBy,
                IsActive             = true
            };

            _db.MedicationAdministrations.Add(record);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Medication {MedicationName} (Schedule {Schedule}) administered to Patient {PatientId} by {User}",
                medication.Name, medication.Schedule, vm.PatientId, administeredBy);

            return record;
        }

        public async Task UpdateAdministrationAsync(int id, AdministerMedicationViewModel vm, bool isNursingSister)
        {
            var existing = await _db.MedicationAdministrations.FindAsync(id)
                           ?? throw new KeyNotFoundException($"MedicationAdministration {id} not found.");

            var medication = await _db.Medications.FindAsync(vm.MedicationId)
                             ?? throw new KeyNotFoundException("Medication not found.");

            if (medication.Schedule > 4 && !isNursingSister)
                throw new UnauthorizedAccessException(
                    $"Only a Nursing Sister may administer Schedule {medication.Schedule} medications.");

            existing.PatientId            = vm.PatientId;
            existing.MedicationId         = vm.MedicationId;
            existing.Dosage               = vm.Dosage;
            existing.AdministrationMethod = vm.AdministrationMethod;
            existing.Notes                = vm.Notes;

            await _db.SaveChangesAsync();
        }

        public async Task SoftDeleteAdministrationAsync(int id)
        {
            var record = await _db.MedicationAdministrations.FindAsync(id)
                         ?? throw new KeyNotFoundException($"MedicationAdministration {id} not found.");
            record.IsActive = false;
            await _db.SaveChangesAsync();
        }
    }
}
