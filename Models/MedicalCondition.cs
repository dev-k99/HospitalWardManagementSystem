using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WardSystemProject.Models
{
    public class MedicalCondition
    {
        [Key] // Primary Key
        public int Id { get; set; }

        [Required(ErrorMessage = "Patient ID is required.")]
        [ForeignKey("Patient")] // Foreign Key referencing Patient
        public int PatientId { get; set; } // Links to the patient

        public Patient Patient { get; set; } // Navigation property to the patient

        [Required(ErrorMessage = "Condition name is required.")]
        [StringLength(100, ErrorMessage = "Condition name must not exceed 100 characters.")]
        public string ConditionName { get; set; } // Name of the medical condition (e.g., Diabetes)

        public bool IsActive { get; set; } = true;
    }

}
