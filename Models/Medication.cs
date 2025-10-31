using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WardSystemProject.Models
{
    public class Medication
    {
        [Key] // Primary Key
        public int Id { get; set; }

        [Required(ErrorMessage = "Medication name is required.")]
        [StringLength(100, ErrorMessage = "Name must not exceed 100 characters.")]
        public string Name { get; set; } // Name of the medication

        [StringLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
        public string Description { get; set; } // Optional description

        [Required(ErrorMessage = "Dosage is required.")]
        [StringLength(50, ErrorMessage = "Dosage must not exceed 50 characters.")]
        public string Dosage { get; set; } // Dosage instructions

        [Required(ErrorMessage = "Schedule is required.")]
        [Range(1, 10, ErrorMessage = "Schedule must be between 1 and 10.")]
        public int Schedule { get; set; } // Medication schedule (1-5+)

        public bool IsActive { get; set; } = true;
    }
}
