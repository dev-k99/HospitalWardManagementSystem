using WardSystemProject.Models;
using WardSystemProject.ViewModels;

namespace WardSystemProject.Core.Interfaces
{
    /// <summary>
    /// Handles vital sign recording and medication administration,
    /// including the Schedule 4 / Schedule 5+ permission check.
    /// </summary>
    public interface IVitalSignService
    {
        Task<IEnumerable<VitalSign>> GetAllAsync();
        Task<VitalSign?> GetByIdAsync(int id);
        Task<VitalSign> RecordAsync(RecordVitalSignViewModel vm, string recordedBy);
        Task UpdateAsync(int id, RecordVitalSignViewModel vm);
        Task SoftDeleteAsync(int id);

        // ── Medication Administration ───────────────────────────────────────
        Task<IEnumerable<MedicationAdministration>> GetAllAdministrationsAsync();
        Task<MedicationAdministration?> GetAdministrationByIdAsync(int id);

        /// <summary>
        /// Records that a nurse/sister administered a medication.
        /// Throws <see cref="UnauthorizedAccessException"/> if a regular Nurse
        /// tries to administer a Schedule 5+ medication.
        /// </summary>
        Task<MedicationAdministration> AdministerAsync(
            AdministerMedicationViewModel vm,
            string administeredBy,
            bool isNursingSister);

        Task UpdateAdministrationAsync(int id, AdministerMedicationViewModel vm, bool isNursingSister);
        Task SoftDeleteAdministrationAsync(int id);
    }
}
