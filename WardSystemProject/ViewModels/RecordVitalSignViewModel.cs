using System.ComponentModel.DataAnnotations;

namespace WardSystemProject.ViewModels
{
    public class RecordVitalSignViewModel
    {
        public int? Id { get; set; }  // null = new record

        [Required(ErrorMessage = "Please select a patient.")]
        [Display(Name = "Patient")]
        public int PatientId { get; set; }

        [Required]
        [Range(20.0, 50.0, ErrorMessage = "Temperature must be between 20°C and 50°C.")]
        [Display(Name = "Temperature (°C)")]
        public double Temperature { get; set; }

        [Required]
        [Range(30, 250, ErrorMessage = "Pulse rate must be between 30 and 250 bpm.")]
        [Display(Name = "Pulse (bpm)")]
        public int Pulse { get; set; }

        [StringLength(20)]
        [Display(Name = "Blood Pressure (e.g. 120/80)")]
        public string? BloodPressure { get; set; }

        [Range(30, 250)]
        [Display(Name = "Heart Rate (bpm)")]
        public int? HeartRate { get; set; }

        [Range(1, 60)]
        [Display(Name = "Respiratory Rate (breaths/min)")]
        public int? RespiratoryRate { get; set; }

        [Range(50, 100)]
        [Display(Name = "O₂ Saturation (%)")]
        public int? OxygenSaturation { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
