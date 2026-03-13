using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WardSystemProject.Models
{
    /// <summary>
    /// Tracks every physical movement of a patient (Theatre, X-Ray, Ward Transfer, Return).
    /// Required by the spec: "keep track of patient movement (e.g. between ward and theatre, x-rays etc.)"
    /// </summary>
    public class PatientMovement
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Patient is required.")]
        [ForeignKey("Patient")]
        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        [Required(ErrorMessage = "Origin ward is required.")]
        [ForeignKey("FromWard")]
        public int FromWardId { get; set; }
        public Ward FromWard { get; set; } = null!;

        [Required(ErrorMessage = "Destination ward is required.")]
        [ForeignKey("ToWard")]
        public int ToWardId { get; set; }
        public Ward ToWard { get; set; } = null!;

        [Required(ErrorMessage = "Movement date is required.")]
        [DataType(DataType.DateTime)]
        public DateTime MovementDate { get; set; }

        /// <summary>
        /// Why the patient was moved — required for clinical accountability.
        /// Values: Theatre, X-Ray, Ward Transfer, Return, Other.
        /// </summary>
        [Required(ErrorMessage = "Movement type is required.")]
        [StringLength(50)]
        public string MovementType { get; set; } = "Ward Transfer";

        /// <summary>Free-text reason or clinical notes for the movement.</summary>
        [StringLength(500)]
        public string? MovementReason { get; set; }

        /// <summary>Who authorised or recorded the movement.</summary>
        [StringLength(100)]
        public string? RecordedBy { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
    
