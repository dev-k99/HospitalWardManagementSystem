using WardSystemProject.Models;
using WardSystemProject.ViewModels;

namespace WardSystemProject.Core.Interfaces
{
    /// <summary>
    /// Encapsulates all patient lifecycle operations: admission, updates, discharge, folder.
    /// Extracted from PatientManagementController to enable unit testing and reuse.
    /// </summary>
    public interface IPatientService
    {
        Task<IEnumerable<Patient>> GetAllActiveAsync();
        Task<IEnumerable<Patient>> SearchAsync(string query);
        Task<Patient?> GetByIdAsync(int id);
        Task<Patient?> GetFullFolderAsync(int id);   // includes all clinical data for the folder view

        /// <summary>
        /// Admits a patient: creates Patient record, assigns bed, creates PatientFolder,
        /// and records the initial ward movement.
        /// </summary>
        Task<Patient> AdmitAsync(AdmitPatientViewModel vm, string admittedBy);

        Task UpdateAsync(int id, EditPatientViewModel vm);

        /// <summary>
        /// Discharges a patient: sets status, records discharge date/summary,
        /// frees the bed, clears ward and doctor assignments.
        /// </summary>
        Task DischargeAsync(int id, DischargePatientViewModel vm, string dischargedBy);

        Task SoftDeleteAsync(int id);
    }
}
