using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        /// <summary>
        /// Links this Staff record to the ASP.NET Core Identity user.
        /// Nullable — records created before this column was added fall back to email lookup.
        /// </summary>
        [StringLength(450)]
        public string? IdentityUserId { get; set; }

        /// <summary>
        /// Ward assignment for Nurses/Nursing Sisters — used to scope patient access.
        /// Null means the staff member has no ward restriction (admin, doctors, etc.).
        /// </summary>
        public int? WardId { get; set; }

        [ForeignKey(nameof(WardId))]
        public Ward? Ward { get; set; }

        // Computed property for full name
        public string FullName => $"{FirstName} {LastName}".Trim();

        public bool IsActive { get; set; } = true;
    }
}
