using System.ComponentModel.DataAnnotations;

namespace WardSystemProject.ViewModels
{
    /// <summary>
    /// ViewModel for the patient admission form.
    /// Domain entity (Patient) is never bound directly to POST forms —
    /// this prevents mass-assignment attacks and keeps form concerns separate.
    /// </summary>
    public class AdmitPatientViewModel
    {
        // ── Personal Details ──────────────────────────────────────────────
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

        // ── Contact ───────────────────────────────────────────────────────
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

        // ── Medical ───────────────────────────────────────────────────────
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

        [StringLength(500)]
        [Display(Name = "Admission Reason")]
        public string? AdmissionReason { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Admission Date")]
        public DateTime AdmissionDate { get; set; } = DateTime.Now;

        // ── Ward / Bed Assignment ─────────────────────────────────────────
        [Required(ErrorMessage = "Please select a ward.")]
        [Display(Name = "Ward")]
        public int WardId { get; set; }

        [Required(ErrorMessage = "Please select an available bed.")]
        [Display(Name = "Bed")]
        public int BedId { get; set; }

        [Required(ErrorMessage = "Please assign an attending doctor.")]
        [Display(Name = "Assigned Doctor")]
        public int AssignedDoctorId { get; set; }
    }
}
