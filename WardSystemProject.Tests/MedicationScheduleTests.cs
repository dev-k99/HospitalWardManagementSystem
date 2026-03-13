using Microsoft.Extensions.Logging;
using Moq;
using WardSystemProject.Features.PatientCare;
using WardSystemProject.Models;
using WardSystemProject.Tests.Helpers;
using WardSystemProject.ViewModels;
using Xunit;

namespace WardSystemProject.Tests
{
    /// <summary>
    /// Unit tests for medication schedule enforcement in <see cref="VitalSignService"/>.
    ///
    /// Business rule (from ONT3010 spec):
    ///   "A nurse is only allowed to dispense medication up to schedule 4.
    ///    Only a Nursing Sister may dispense any schedule 5 (or higher) medication."
    ///
    /// These tests prove that the rule is enforced in the service layer,
    /// independent of the HTTP pipeline or controller logic.
    /// </summary>
    public sealed class MedicationScheduleTests
    {
        private static readonly Mock<ILogger<VitalSignService>> _mockLogger = new();

        // ── Helpers ──────────────────────────────────────────────────────────

        private static async Task<(Patient patient, Medication schedule4Med, Medication schedule5Med)>
            SeedAsync(WardSystemProject.Data.WardSystemDBContext db)
        {
            var ward = new Ward { Name = "Test Ward", IsActive = true };
            db.Wards.Add(ward);

            var patient = new Patient
            {
                FirstName = "Test", LastName = "Patient",
                DateOfBirth = DateTime.Today.AddYears(-30), Gender = "Male",
                ContactNumber = "0800000001", EmergencyContact = "X",
                EmergencyContactNumber = "0800000002", Address = "X",
                NextOfKin = "X", NextOfKinContact = "0800000003",
                WardId = ward.Id, PatientStatus = "Admitted", IsActive = true
            };
            db.Patients.Add(patient);

            var schedule4 = new Medication { Name = "Paracetamol", Dosage = "500mg", Schedule = 4, IsActive = true };
            var schedule5 = new Medication { Name = "Codeine Phosphate", Dosage = "30mg", Schedule = 5, IsActive = true };
            db.Medications.AddRange(schedule4, schedule5);

            await db.SaveChangesAsync();
            return (patient, schedule4, schedule5);
        }

        private static AdministerMedicationViewModel BuildVm(int patientId, int medicationId) => new()
        {
            PatientId   = patientId,
            MedicationId = medicationId,
            Dosage      = "1 tablet",
            AdministrationMethod = "Oral"
        };

        // ── Test: Nurse CAN administer Schedule 4 ────────────────────────────

        [Fact]
        public async Task AdministerAsync_Nurse_CanAdminister_Schedule4()
        {
            var db     = TestDbContextFactory.Create();
            var (patient, schedule4, _) = await SeedAsync(db);
            var service = new VitalSignService(db, _mockLogger.Object);

            // Act — isNursingSister = false (regular Nurse)
            var record = await service.AdministerAsync(BuildVm(patient.Id, schedule4.Id), "nurse1", isNursingSister: false);

            // Assert
            Assert.NotEqual(0, record.Id);
            Assert.Equal(schedule4.Id, record.MedicationId);
        }

        // ── Test: Nurse CANNOT administer Schedule 5 ─────────────────────────

        [Fact]
        public async Task AdministerAsync_Nurse_CannotAdminister_Schedule5_ThrowsUnauthorized()
        {
            var db     = TestDbContextFactory.Create();
            var (patient, _, schedule5) = await SeedAsync(db);
            var service = new VitalSignService(db, _mockLogger.Object);

            // Act & Assert — Nurse (isNursingSister = false) attempts Schedule 5
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.AdministerAsync(BuildVm(patient.Id, schedule5.Id), "nurse1", isNursingSister: false));
        }

        // ── Test: Nursing Sister CAN administer Schedule 5 ───────────────────

        [Fact]
        public async Task AdministerAsync_NursingSister_CanAdminister_Schedule5()
        {
            var db     = TestDbContextFactory.Create();
            var (patient, _, schedule5) = await SeedAsync(db);
            var service = new VitalSignService(db, _mockLogger.Object);

            // Act — isNursingSister = true
            var record = await service.AdministerAsync(BuildVm(patient.Id, schedule5.Id), "sister1", isNursingSister: true);

            // Assert
            Assert.NotEqual(0, record.Id);
            Assert.Equal(schedule5.Id, record.MedicationId);
            Assert.Equal("sister1", record.AdministeredBy);
        }

        // ── Test: Error message is informative ───────────────────────────────

        [Fact]
        public async Task AdministerAsync_Nurse_Schedule5_ExceptionContainsMedicationName()
        {
            var db     = TestDbContextFactory.Create();
            var (patient, _, schedule5) = await SeedAsync(db);
            var service = new VitalSignService(db, _mockLogger.Object);

            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.AdministerAsync(BuildVm(patient.Id, schedule5.Id), "nurse1", isNursingSister: false));

            // Message must be staff-facing and meaningful
            Assert.Contains("Nursing Sister", ex.Message);
            Assert.Contains("Schedule 5", ex.Message);
        }
    }
}
