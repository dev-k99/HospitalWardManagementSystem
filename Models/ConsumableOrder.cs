using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WardSystemProject.Models
{
    public class ConsumableOrder
    {
        [Key] // Primary Key
        public int Id { get; set; }

        [Required(ErrorMessage = "Consumable ID is required.")]
        [ForeignKey("Consumable")] // Foreign Key referencing Consumable
        public int ConsumableId { get; set; } // Links to the consumable

        public Consumable Consumable { get; set; } // Navigation property to the consumable

        [Required(ErrorMessage = "Stock manager ID is required.")]
        [ForeignKey("Staff")] // Foreign Key referencing Staff
        public int StockManagerId { get; set; } // Links to the staff managing the order

        public Staff StockManager { get; set; } // Navigation property to the staff

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000.")]
        public int Quantity { get; set; } // Number of items ordered

        [Required(ErrorMessage = "Order date is required.")]
        [DataType(DataType.DateTime, ErrorMessage = "Invalid date/time format.")]
        public DateTime OrderDate { get; set; } // Date the order was placed

        [DataType(DataType.DateTime, ErrorMessage = "Invalid date/time format.")]
        public DateTime? ReceivedDate { get; set; } // Nullable date items were received

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(50, ErrorMessage = "Status must not exceed 50 characters.")]
        public string Status { get; set; } // Order status (e.g., Pending, Delivered)

        public bool IsActive { get; set; } = true;
    }

}
