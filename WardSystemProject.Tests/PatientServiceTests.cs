using Microsoft.Extensions.Logging;
using Moq;
using WardSystemProject.Features.PatientManagement;
using WardSystemProject.Models;
using WardSystemProject.Tests.Helpers;
using WardSystemProject.ViewModels;
using Xunit;

namespace WardSystemProject.Tests
{
    /// <summary>
    /// Unit tests for <see cref="PatientService"/>.
    ///
    /// These tests verify three critical flows identified in the plan:
    ///   1. Admission succeeds when a bed is available — patient, bed record, folder and movement are created.
    ///   2. Admission fails when the selected bed is already occupied (critical bug in original controller, now fixed).
    ///   3. Discharge succeeds: status updated, bed freed, ward/doctor cleared.
    ///
    /// The EF InMemory provider is used so no SQL Server connection is required in CI.
    /// </summary>
    public sealed class PatientServiceTests
    {
        private static readonly Mock<ILogger<PatientService>> _mockLogger = new();

        private static PatientService CreateService(string? dbName = null) =>
            new PatientService(TestDbContextFactory.Create(dbName), _mockLogger.Object);

        // ── Helpers ──────────────────────────────────────────────────────────

        private static AdmitPatientViewModel BuildAdmitVm(int wardId, int bedId, int doctorId) => new()
        {
            FirstName              = "Lindiwe",
            LastName               = "Dlamini",
            DateOfBirth            = new DateTime(1988, 5, 14),
            Gender                 = "Female",
            ContactNumber          = "0823456789",
            EmergencyContact       = "Sipho Dlamini",
            EmergencyContactNumber = "0834567890",
            Address                = "12 Ntuli Street, Soweto",
            NextOfKin              = "Sipho Dlamini",
            NextOfKinContact       = "0834567890",
            AdmissionDate          = DateTime.Now,
            WardId                 = wardId,
            BedId                  = bedId,
            AssignedDoctorId       = doctorId,
            AdmissionReason        = "Chest pain and shortness of breath"
        };

        private static async Task<(Ward ward, Bed bed, Staff doctor)> SeedWardBedDoctorAsync(
            WardSystemProject.Data.WardSystemDBContext db)
        {
            var ward = new Ward { Name = "Cardiology Ward", IsActive = true };
            db.Wards.Add(ward);

            var room = new Room { RoomNumber = "C1", Ward = ward, IsActive = true };
            db.Rooms.Add(room);

            var bed = new Bed { BedNumber = "C1-01", Room = room, IsActive = true };
            db.Beds.Add(bed);

            var doctor = new Staff { FirstName = "Themba", LastName = "Nkosi", Role = "Doctor", Email = "tnkosi@hospital.co.za", IsActive = true };
            db.Staff.Add(doctor);

            await db.SaveChangesAsync();
            return (ward, bed, doctor);
        }

        // ── Test: Successful admission ────────────────────────────────────────

        [Fact]
        public async Task AdmitAsync_WhenBedIsAvailable_CreatesPatientAndAssignsBed()
        {
            // Arrange
            var dbName = nameof(AdmitAsync_WhenBedIsAvailable_CreatesPatientAndAssignsBed);
            var db     = TestDbContextFactory.Create(dbName);
            var (ward, bed, doctor) = await SeedWardBedDoctorAsync(db);

            var service = new PatientService(db, _mockLogger.Object);
            var vm      = BuildAdmitVm(ward.Id, bed.Id, doctor.Id);

            // Act
            var patient = await service.AdmitAsync(vm, "wardadmin");

            // Assert: patient created with correct details
            Assert.NotEqual(0, patient.Id);
            Assert.Equal("Lindiwe", patient.FirstName);
            Assert.Equal("Admitted", patient.PatientStatus);
            Assert.Equal(bed.Id, patient.BedId);
            Assert.Equal(ward.Id, patient.WardId);

            // Assert: bed is now occupied
            var updatedBed = await db.Beds.FindAsync(bed.Id);
            Assert.Equal(patient.Id, updatedBed!.PatientId);

            // Assert: patient folder was created
            var folder = db.PatientFolders.FirstOrDefault(f => f.PatientId == patient.Id);
            Assert.NotNull(folder);
            Assert.Equal("Admitted", folder.PatientStatus);

            // Assert: initial movement recorded
            var movement = db.PatientMovements.FirstOrDefault(m => m.PatientId == patient.Id);
            Assert.NotNull(movement);
            Assert.Equal("Ward Transfer", movement.MovementType);
        }

        // ── Test: Critical bug fix — bed already occupied ─────────────────────

        [Fact]
        public async Task AdmitAsync_WhenBedAlreadyOccupied_ThrowsInvalidOperationException()
        {
            // Arrange
            var dbName = nameof(AdmitAsync_WhenBedAlreadyOccupied_ThrowsInvalidOperationException);
            var db     = TestDbContextFactory.Create(dbName);
            var (ward, bed, doctor) = await SeedWardBedDoctorAsync(db);

            // Occupy the bed with another patient
            var existingPatient = new Patient
            {
                FirstName = "Other", LastName = "Patient", DateOfBirth = DateTime.Today.AddYears(-40),
                Gender = "Male", ContactNumber = "0800000001", EmergencyContact = "X",
                EmergencyContactNumber = "0800000002", Address = "X", NextOfKin = "X",
                NextOfKinContact = "0800000003", WardId = ward.Id, BedId = bed.Id,
                PatientStatus = "Admitted", IsActive = true
            };
            db.Patients.Add(existingPatient);
            bed.PatientId = existingPatient.Id;
            await db.SaveChangesAsync();

            var service = new PatientService(db, _mockLogger.Object);
            var vm      = BuildAdmitVm(ward.Id, bed.Id, doctor.Id);

            // Act & Assert — should throw because bed is occupied
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.AdmitAsync(vm, "wardadmin"));
        }

        // ── Test: Discharge ───────────────────────────────────────────────────

        [Fact]
        public async Task DischargeAsync_WhenPatientAdmitted_FreesBedsAndUpdateStatus()
        {
            // Arrange
            var dbName = nameof(DischargeAsync_WhenPatientAdmitted_FreesBedsAndUpdateStatus);
            var db     = TestDbContextFactory.Create(dbName);
            var (ward, bed, doctor) = await SeedWardBedDoctorAsync(db);

            // First admit the patient
            var service = new PatientService(db, _mockLogger.Object);
            var patient = await service.AdmitAsync(BuildAdmitVm(ward.Id, bed.Id, doctor.Id), "wardadmin");

            var dischargeVm = new DischargePatientViewModel
            {
                PatientId       = patient.Id,
                PatientName     = patient.FullName,
                DischargeDate   = DateTime.Now,
                DischargeSummary = "Patient recovered fully. Advised to follow up in 2 weeks."
            };

            // Act
            await service.DischargeAsync(patient.Id, dischargeVm, "wardadmin");

            // Assert: patient status changed
            var discharged = await db.Patients.FindAsync(patient.Id);
            Assert.Equal("Discharged", discharged!.PatientStatus);
            Assert.Null(discharged.BedId);
            Assert.Null(discharged.WardId);
            Assert.NotNull(discharged.DischargeDate);

            // Assert: bed is now free
            var freedBed = await db.Beds.FindAsync(bed.Id);
            Assert.Null(freedBed!.PatientId);
        }

        // ── Test: Double discharge ────────────────────────────────────────────

        [Fact]
        public async Task DischargeAsync_WhenAlreadyDischarged_ThrowsInvalidOperationException()
        {
            // Arrange
            var dbName = nameof(DischargeAsync_WhenAlreadyDischarged_ThrowsInvalidOperationException);
            var db     = TestDbContextFactory.Create(dbName);
            var (ward, bed, doctor) = await SeedWardBedDoctorAsync(db);

            var service = new PatientService(db, _mockLogger.Object);
            var patient = await service.AdmitAsync(BuildAdmitVm(ward.Id, bed.Id, doctor.Id), "wardadmin");

            var dischargeVm = new DischargePatientViewModel
            {
                PatientId        = patient.Id,
                DischargeDate    = DateTime.Now,
                DischargeSummary = "Fully recovered."
            };

            // First discharge
            await service.DischargeAsync(patient.Id, dischargeVm, "wardadmin");

            // Act & Assert — second discharge should throw
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.DischargeAsync(patient.Id, dischargeVm, "wardadmin"));
        }
    }
}
