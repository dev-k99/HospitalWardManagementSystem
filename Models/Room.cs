using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WardSystemProject.Models
{
    public class Room
    {
        [Key] // Primary Key
        public int Id { get; set; }

        [Required(ErrorMessage = "Room number is required.")]
        [StringLength(50, ErrorMessage = "Room number must not exceed 50 characters.")]
        public string RoomNumber { get; set; } // Unique identifier for the room

        [Required(ErrorMessage = "Ward ID is required.")]
        [ForeignKey("Ward")] // Foreign Key referencing Ward
        public int WardId { get; set; } // Links to the parent ward

        public Ward Ward { get; set; } // Navigation property to the parent ward

        // One-to-many relationship with Bed (a room can have multiple beds)
        public ICollection<Bed> Beds { get; set; } // Collection of beds in this room


        [Required]
        public bool IsActive { get; set; } = true;
    }
}
