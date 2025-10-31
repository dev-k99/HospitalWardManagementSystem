using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WardSystemProject.Models
{
    public class VitalSign
    {
        [Key] // Primary Key
        public int Id { get; set; }

        [Required(ErrorMessage = "Patient ID is required.")]
        [ForeignKey("Patient")] // Foreign Key referencing Patient
        public int PatientId { get; set; } // Links to the patient

        public Patient Patient { get; set; } // Navigation property to the patient

        [Required(ErrorMessage = "Temperature is required.")]
        [Range(20.0, 50.0, ErrorMessage = "Temperature must be between 20°C and 50°C.")]
        public double Temperature { get; set; } // Temperature in Celsius

        [Required(ErrorMessage = "Pulse is required.")]
        [Range(30, 200, ErrorMessage = "Pulse must be between 30 and 200 bpm.")]
        public int Pulse { get; set; } // Pulse rate in beats per minute

        [Required(ErrorMessage = "Record date is required.")]
        [DataType(DataType.DateTime, ErrorMessage = "Invalid date/time format.")]
        public DateTime RecordDate { get; set; } // Date and time of recording

        // Additional vital signs properties
        public string BloodPressure { get; set; } // Blood pressure (e.g., "120/80")
        public int? HeartRate { get; set; } // Heart rate in bpm
        public int? RespiratoryRate { get; set; } // Respiratory rate in breaths per minute
        public int? OxygenSaturation { get; set; } // Oxygen saturation percentage
        public string Notes { get; set; } // Additional notes
        public string RecordedBy { get; set; } // Name of the person who recorded the vitals

        // Computed property for backward compatibility
        public DateTime RecordedDate => RecordDate;

        public bool IsActive { get; set; } = true;
    }
}


