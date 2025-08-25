using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WardSystemProject.Models
{
    public class Patient
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name must not exceed 50 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name must not exceed 50 characters.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Date of birth is required.")]
        [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Contact number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "Emergency contact is required.")]
        public string EmergencyContact { get; set; }

        [Required(ErrorMessage = "Emergency contact number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string EmergencyContactNumber { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Next of kin is required.")]
        public string NextOfKin { get; set; }

        [Required(ErrorMessage = "Next of kin contact is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string NextOfKinContact { get; set; }

        // Medical Information
        [Display(Name = "Blood Type")]
        public string BloodType { get; set; }

        [Display(Name = "Chronic Medications")]
        public string ChronicMedications { get; set; }

        [Display(Name = "Medical History")]
        public string MedicalHistory { get; set; }

        [Display(Name = "Allergies")]
        public string Allergies { get; set; }

        // Admission Information
        [Display(Name = "Admission Date")]
        [DataType(DataType.DateTime)]
        public DateTime? AdmissionDate { get; set; }

        [Display(Name = "Discharge Date")]
        [DataType(DataType.DateTime)]
        public DateTime? DischargeDate { get; set; }

        [Display(Name = "Admission Reason")]
        public string AdmissionReason { get; set; }

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
        public string PatientStatus { get; set; } = "Admitted"; // Admitted, Discharged, Transferred, etc.

        // Navigation Properties
        public ICollection<PatientMovement> PatientMovements { get; set; }
        public ICollection<Allergy> PatientAllergies { get; set; }
        public ICollection<MedicalCondition> MedicalConditions { get; set; }
        public ICollection<DoctorVisit> DoctorVisits { get; set; }
        public ICollection<Prescription> Prescriptions { get; set; }
        public ICollection<MedicationAdministration> MedicationAdministrations { get; set; }
        public ICollection<VitalSign> VitalSigns { get; set; }
        public ICollection<DoctorInstruction> DoctorInstructions { get; set; }

        public bool IsActive { get; set; } = true;

        // Computed Properties
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [NotMapped]
        public int Age => DateTime.Now.Year - DateOfBirth.Year - (DateTime.Now.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);

        [NotMapped]
        public bool IsCurrentlyAdmitted => AdmissionDate.HasValue && !DischargeDate.HasValue;
    }
}