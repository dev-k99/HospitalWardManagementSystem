using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WardSystemProject.Models
{
    public class PrescriptionOrder
    {
        [Key] // Primary Key
        public int Id { get; set; }

        [Required(ErrorMessage = "Prescription ID is required.")]
        [ForeignKey("Prescription")] // Foreign Key referencing Prescription
        public int PrescriptionId { get; set; } // Links to the prescription

        public Prescription Prescription { get; set; } // Navigation property to the prescription

        [Required(ErrorMessage = "Script manager ID is required.")]
        [ForeignKey(nameof(Staff))] // Foreign Key referencing Staff with no cascade delete
        public int ScriptManagerId { get; set; } // Links to the staff managing the order

        public Staff ScriptManager { get; set; } // Navigation property to the staff

        [Required(ErrorMessage = "Order date is required.")]
        [DataType(DataType.DateTime, ErrorMessage = "Invalid date/time format.")]
        public DateTime OrderDate { get; set; } = DateTime.Now; // Date the order was placed

        [DataType(DataType.DateTime, ErrorMessage = "Invalid date/time format.")]
        public DateTime? SentToPharmacy { get; set; } // Nullable date sent to pharmacy

        [DataType(DataType.DateTime, ErrorMessage = "Invalid date/time format.")]
        public DateTime? ReceivedInWard { get; set; } // Nullable date received in ward

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(50, ErrorMessage = "Status must not exceed 50 characters.")]
        public string Status { get; set; } // Order status (e.g., Pending, Completed)

        public bool IsActive { get; set; } = true;
    }
}
