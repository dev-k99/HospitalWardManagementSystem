using System.ComponentModel.DataAnnotations;

namespace WardSystemProject.Models
{
    public class Ward
    {
        [Key] // Primary Key
        public int Id { get; set; }

        [Required(ErrorMessage = "Ward name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Ward name must be between 2 and 100 characters.")]
        public string Name { get; set; } // Name of the ward

        [StringLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
        public string Description { get; set; } // Optional description of the ward

        // One-to-many relationship with Room (a ward can have multiple rooms)
        public ICollection<Room> Rooms { get; set; } // Collection of rooms in this ward
        public bool IsActive { get; set; } = true;
    }
}
