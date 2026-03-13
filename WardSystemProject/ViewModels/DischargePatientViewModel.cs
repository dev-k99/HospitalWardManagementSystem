using System.ComponentModel.DataAnnotations;

namespace WardSystemProject.ViewModels
{
    public class DischargePatientViewModel
    {
        public int PatientId { get; set; }

        // Display-only (not posted back, populated by service/controller)
        public string PatientName { get; set; } = string.Empty;
        public string? WardName   { get; set; }
        public string? BedNumber  { get; set; }
        public string? DoctorName { get; set; }

        [Required(ErrorMessage = "A discharge summary is required before discharging a patient.")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Please provide a meaningful discharge summary (at least 10 characters).")]
        [Display(Name = "Discharge Summary")]
        public string DischargeSummary { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Discharge Date & Time")]
        public DateTime DischargeDate { get; set; } = DateTime.Now;
    }
}
