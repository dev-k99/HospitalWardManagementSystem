using System.ComponentModel.DataAnnotations;

namespace WardSystemProject.ViewModels
{
    public class EditPatientViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public string Gender { get; set; } = string.Empty;

        [Required, Phone, StringLength(20)]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; } = string.Empty;

        [Required, StringLength(100)]
        [Display(Name = "Emergency Contact Name")]
        public string EmergencyContact { get; set; } = string.Empty;

        [Required, Phone, StringLength(20)]
        [Display(Name = "Emergency Contact Number")]
        public string EmergencyContactNumber { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required, StringLength(100)]
        [Display(Name = "Next of Kin")]
        public string NextOfKin { get; set; } = string.Empty;

        [Required, Phone, StringLength(20)]
        [Display(Name = "Next of Kin Contact")]
        public string NextOfKinContact { get; set; } = string.Empty;

        [StringLength(10)]
        [Display(Name = "Blood Type")]
        public string? BloodType { get; set; }

        [StringLength(500)]
        [Display(Name = "Chronic Medications")]
        public string? ChronicMedications { get; set; }

        [StringLength(1000)]
        [Display(Name = "Medical History")]
        public string? MedicalHistory { get; set; }

        [StringLength(500)]
        public string? Allergies { get; set; }

        [Display(Name = "Ward")]
        public int? WardId { get; set; }

        [Display(Name = "Bed")]
        public int? BedId { get; set; }

        [Display(Name = "Assigned Doctor")]
        public int? AssignedDoctorId { get; set; }

        [StringLength(20)]
        [Display(Name = "Patient Status")]
        public string PatientStatus { get; set; } = "Admitted";
    }
}
