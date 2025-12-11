using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WardSystemProject.Models
{
    public class DoctorInstruction
    {
        [Key] // Primary Key
        public int Id { get; set; }

        [Required(ErrorMessage = "Patient ID is required.")]
        [ForeignKey("Patient")] // Foreign Key referencing Patient
        public int PatientId { get; set; } // Links to the patient

        public Patient Patient { get; set; } // Navigation property to the patient

        [Required(ErrorMessage = "Doctor ID is required.")]
        [ForeignKey("Staff")] // Foreign Key referencing Staff (assuming doctors are Staff)
        public int DoctorId { get; set; } // Links to the doctor (a staff member)

        public Staff Doctor { get; set; } // Navigation property to the doctor

        [Required(ErrorMessage = "Instruction text is required.")]
        [StringLength(1000, ErrorMessage = "Instruction must not exceed 1000 characters.")]
        public string Details { get; set; } // Details of the instruction

        [Required(ErrorMessage = "Instruction date is required.")]
        [DataType(DataType.DateTime, ErrorMessage = "Invalid date/time format.")]
        public DateTime InstructionDate { get; set; } // Date and time of instruction

        // Additional properties
        public string InstructionType { get; set; } // Type of instruction (Treatment, Medication, etc.)
        public string Priority { get; set; } // Priority level (Low, Medium, High)
        public string Status { get; set; } // Status (Pending, In Progress, Completed)
        public string Instructions { get; set; } // Detailed instructions

        //ForSoftDelete
        public bool IsActive { get; set; } = true;
    }
}
