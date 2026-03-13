using System.ComponentModel.DataAnnotations;

namespace WardSystemProject.ViewModels
{
    public class AdministerMedicationViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Please select a patient.")]
        [Display(Name = "Patient")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "Please select a medication.")]
        [Display(Name = "Medication")]
        public int MedicationId { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Dosage Given")]
        public string Dosage { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Administration Method")]
        public string? AdministrationMethod { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
