using System.ComponentModel.DataAnnotations;

namespace WardSystemProject.Models
{
    public class Staff
    {
        [Key] // Primary Key
        public int Id { get; set; }

        [Required(ErrorMessage = "Staff first name is required.")]
        [StringLength(50, ErrorMessage = "First name must not exceed 50 characters.")]
        public string FirstName { get; set; } // Staff member's first name

        [Required(ErrorMessage = "Staff last name is required.")]
        [StringLength(50, ErrorMessage = "Last name must not exceed 50 characters.")]
        public string LastName { get; set; } // Staff member's last name

        [Required(ErrorMessage = "Staff role is required.")]
        [StringLength(50, ErrorMessage = "Role must not exceed 50 characters.")]
        public string Role { get; set; } // Job role (e.g., Doctor, Nurse)

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(100, ErrorMessage = "Email must not exceed 100 characters.")]
        public string Email { get; set; } // Contact email for the staff

        // Computed property for full name
        public string FullName => $"{FirstName} {LastName}".Trim();

        public bool IsActive { get; set; } = true;
    }
}
