using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WardSystemProject.Models
{
    public class DoctorVisit
    {
        [Key] // Primary Key
        public int Id { get; set; }

        [Required(ErrorMessage = "Patient ID is required.")]
        [ForeignKey("Patient")] // Foreign Key referencing Patient
        public int PatientId { get; set; } // Links to the patient

        public Patient Patient { get; set; } // Navigation property to the patient

        [Required(ErrorMessage = "Doctor ID is required.")]
        [ForeignKey("Staff")] // Foreign Key referencing Staff (assuming doctors are Staff)
        public int DoctorId { get; set; } // Links to the doctor

        public Staff Doctor { get; set; } // Navigation property to the doctor

        [Required(ErrorMessage = "Visit date is required.")]
        [DataType(DataType.DateTime, ErrorMessage = "Invalid date/time format.")]
        public DateTime VisitDate { get; set; } // Date and time of the visit

        [Required(ErrorMessage = "Visit type is required.")]
        [StringLength(100, ErrorMessage = "Visit type must not exceed 100 characters.")]
        public string VisitType { get; set; } // Type of visit (Initial, Follow-up, Emergency, etc.)

        [StringLength(500, ErrorMessage = "Notes must not exceed 500 characters.")]
        public string Notes { get; set; } // Optional notes from the visit

        [DataType(DataType.DateTime, ErrorMessage = "Invalid date/time format.")]
        public DateTime? NextVisitDate { get; set; } // Optional next visit date

        public bool IsActive { get; set; } = true;
    }

}
