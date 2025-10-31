using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace WardSystemProject.Models
{
    public class PatientFolder
    {
        [Key]
        public int FolderId { get; set; }
        [Required]
        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }
        [Display(Name = "Admission Date")]
        [DataType(DataType.DateTime)]
        public DateTime? AdmissionDate { get; set; }
        [Display(Name = "Discharge Date")]
        [DataType(DataType.DateTime)]
        public DateTime? DischargeDate { get; set; }
        [Display(Name = "Discharge Summary")]
        public string DischargeSummary { get; set; }
        // Doctor Assignment
        [ForeignKey("AssignedDoctor")]
        public int? AssignedDoctorId { get; set; }
        public Staff AssignedDoctor { get; set; }
        // Ward and Bed Assignment
        [ForeignKey("Ward")]
        public int? WardId { get; set; }
        public Ward Ward { get; set; }
        [ForeignKey("Bed")]
        public int? BedId { get; set; }
        public Bed Bed { get; set; }
        // Patient Status
        [Display(Name = "Patient Status")]
        public string PatientStatus { get; set; } = "Admitted";
        public ICollection<PatientMovement> PatientMovements { get; set; }
        public ICollection<Allergy> PatientAllergies { get; set; }
        public ICollection<MedicalCondition> MedicalConditions { get; set; }
        public ICollection<DoctorVisit> DoctorVisits { get; set; }
        public ICollection<Prescription> Prescriptions { get; set; }
        public ICollection<MedicationAdministration> MedicationAdministrations { get; set; }
        public ICollection<VitalSign> VitalSigns { get; set; }
        public ICollection<DoctorInstruction> DoctorInstructions { get; set; }
        [NotMapped]
        public bool IsCurrentlyAdmitted => AdmissionDate.HasValue && !DischargeDate.HasValue;
    }
}

//using System.ComponentModel.DataAnnotations.Schema;
//using System.ComponentModel.DataAnnotations;

//namespace WardSystemProject.Models
//{
//    public class PatientFolder
//    {
//        [Key]
//        public int FolderId { get; set; }
//        [Display(Name = "Admission Date")]
//        [DataType(DataType.DateTime)]
//        public DateTime? AdmissionDate { get; set; }

//        [Display(Name = "Discharge Date")]
//        [DataType(DataType.DateTime)]
//        public DateTime? DischargeDate { get; set; }


//        [Display(Name = "Discharge Summary")]
//        public string DischargeSummary { get; set; }

//        // Doctor Assignment
//        [ForeignKey("AssignedDoctor")]
//        public int? AssignedDoctorId { get; set; }
//        public Staff AssignedDoctor { get; set; }

//        // Ward and Bed Assignment
//        [ForeignKey("Ward")]
//        public int? WardId { get; set; }
//        public Ward Ward { get; set; }

//        [ForeignKey("Bed")]
//        public int? BedId { get; set; }
//        public Bed Bed { get; set; }

//        // Patient Status
//        [Display(Name = "Patient Status")]
//        public string PatientStatus { get; set; } = "Admitted";
//        public ICollection<PatientMovement> PatientMovements { get; set; }
//        public ICollection<Allergy> PatientAllergies { get; set; }
//        public ICollection<MedicalCondition> MedicalConditions { get; set; }
//        public ICollection<DoctorVisit> DoctorVisits { get; set; }
//        public ICollection<Prescription> Prescriptions { get; set; }
//        public ICollection<MedicationAdministration> MedicationAdministrations { get; set; }
//        public ICollection<VitalSign> VitalSigns { get; set; }
//        public ICollection<DoctorInstruction> DoctorInstructions { get; set; }
//        [NotMapped]
//        public bool IsCurrentlyAdmitted => AdmissionDate.HasValue && !DischargeDate.HasValue;
//    }
//}
