using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WardSystemProject.Models
{
    public class MedicationAdministration
    {
        [Key] // Primary Key
        public int Id { get; set; }

        [Required(ErrorMessage = "Patient ID is required.")]
        [ForeignKey("Patient")] // Foreign Key referencing Patient
        public int PatientId { get; set; } // Links to the patient

        public Patient Patient { get; set; } // Navigation property to the patient

        [Required(ErrorMessage = "Medication ID is required.")]
        [ForeignKey("Medication")] // Foreign Key referencing Medication
        public int MedicationId { get; set; } // Links to the medication

        public Medication Medication { get; set; } // Navigation property to the medication

        [Required(ErrorMessage = "Administration date is required.")]
        [DataType(DataType.DateTime, ErrorMessage = "Invalid date/time format.")]
        public DateTime AdministrationDate { get; set; } // Date and time of administration

        [Required(ErrorMessage = "Administering staff ID is required.")]
        [ForeignKey("Staff")] // Foreign Key referencing Staff
        public int AdministeringStaffId { get; set; } // Links to the staff member

        [Required(ErrorMessage = "Dosage is required.")]
        public string Dosage { get; set; }

        // Additional properties
        public string AdministrationMethod { get; set; } // Method of administration (Oral, IV, etc.)
        public string Notes { get; set; } // Additional notes about administration
        public string AdministeredBy { get; set; } // Name of the person who administered

        public Staff AdministeringStaff { get; set; } // Navigation property to the staff
        public bool IsActive { get; set; } = true;
    }
}
