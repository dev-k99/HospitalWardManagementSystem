using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WardSystemProject.Models
{
    public class Bed
    {
        [Key] // Primary Key
        public int Id { get; set; }

        [Required(ErrorMessage = "Bed number is required.")]
        [StringLength(20, ErrorMessage = "Bed number must not exceed 20 characters.")]
        public string BedNumber { get; set; } // Unique identifier for the bed

        [Required(ErrorMessage = "Room ID is required.")]
        [ForeignKey("Room")] // Foreign Key referencing Room
        public int RoomId { get; set; } // Links to the parent room

        public Room Room { get; set; } // Navigation property to the parent room

        // One-to-one relationship with Patient (a bed is assigned to one patient at a time)
        public int? PatientId { get; set; } // Nullable FK to Patient (null if unoccupied)
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; } // Navigation property to the assigned patient

        public bool IsActive { get; set; } = true;
    }
}
