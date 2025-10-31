using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WardSystemProject.Models
{
    public class Prescription
    {
        [Key] // Primary Key
        public int Id { get; set; }

        [Required(ErrorMessage = "Patient ID is required.")]
        [ForeignKey("Patient")] // Foreign Key referencing Patient
        public int PatientId { get; set; } // Links to the patient

        public Patient Patient { get; set; } // Navigation property to the patient

        [Required(ErrorMessage = "Doctor ID is required.")]
        [ForeignKey(nameof(Staff))] // Foreign Key referencing Staff with no cascade delete
        public int DoctorId { get; set; } // Links to the prescribing doctor

        public Staff Doctor { get; set; } // Navigation property to the doctor

        [Required(ErrorMessage = "Medication ID is required.")]
        [ForeignKey("Medication")] // Foreign Key referencing Medication
        public int MedicationId { get; set; } // Links to the prescribed medication

        public Medication Medication { get; set; } // Navigation property to the medication

        [Required(ErrorMessage = "Dosage instructions are required.")]
        [StringLength(100, ErrorMessage = "Dosage instructions must not exceed 100 characters.")]
        public string DosageInstructions { get; set; } // How the medication should be taken

        [Required(ErrorMessage = "Duration is required.")]
        [StringLength(50, ErrorMessage = "Duration must not exceed 50 characters.")]
        public string Duration { get; set; } // How long to take the medication

        [StringLength(500, ErrorMessage = "Instructions must not exceed 500 characters.")]
        public string Instructions { get; set; } // Additional instructions for the patient

        [Required(ErrorMessage = "Prescription date is required.")]
        [DataType(DataType.DateTime, ErrorMessage = "Invalid date/time format.")]
        public DateTime PrescriptionDate { get; set; } // Date and time of prescription

        public bool IsActive { get; set; } = true;
    }

}
