using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WardSystemProject.Models
{

    public class PatientMovement
    {
        [Key] // Primary Key
        public int Id { get; set; }

        [Required(ErrorMessage = "Patient ID is required.")]
        [ForeignKey("Patient")] // Foreign Key referencing Patient
        public int PatientId { get; set; } // Links to the patient

        public Patient Patient { get; set; } // Navigation property to the patient

        [Required(ErrorMessage = "From ward ID is required.")]
        [ForeignKey("Ward")] // Foreign Key referencing Ward
        public int FromWardId { get; set; } // Origin ward

        public Ward FromWard { get; set; } // Navigation property to the origin ward

        [Required(ErrorMessage = "To ward ID is required.")]
        [ForeignKey("Ward")] // Foreign Key (same Ward entity, different instance)
        public int ToWardId { get; set; } // Destination ward

        public Ward ToWard { get; set; } // Navigation property to the destination ward

        [Required(ErrorMessage = "Movement date is required.")]
        [DataType(DataType.DateTime, ErrorMessage = "Invalid date/time format.")]
        public DateTime MovementDate { get; set; } // Date and time of movement

        public bool IsActive { get; set; } = true;
    }

}
    
