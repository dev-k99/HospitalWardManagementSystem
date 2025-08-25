using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WardSystemProject.Models
{
    public class Consumable
    {
        [Key] // Primary Key
        public int Id { get; set; }

        [Required(ErrorMessage = "Consumable name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Consumable name must be between 2 and 100 characters.")]
        public string Name { get; set; } // Name of the consumable item

        [StringLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
        public string? Description { get; set; } // Description of the consumable

        [Required(ErrorMessage = "Quantity on hand is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity on hand must be a non-negative number.")]
        public int QuantityOnHand { get; set; } // Current stock level

        [Required(ErrorMessage = "Reorder level is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Reorder level must be a non-negative number.")]
        public int ReorderLevel { get; set; } // Minimum stock level before reordering

        [Required(ErrorMessage = "Unit is required.")]
        [StringLength(20, ErrorMessage = "Unit must not exceed 20 characters.")]
        public string Unit { get; set; } // Unit of measurement (e.g., boxes, pieces, rolls)

        [Required(ErrorMessage = "Ward ID is required.")]
        [ForeignKey("Ward")] // Foreign Key referencing Ward
        public int WardId { get; set; } // Links to the ward where the consumable is stored

        public Ward Ward { get; set; } // Navigation property to the associated ward

        [DataType(DataType.DateTime)]
        public DateTime? LastUpdated { get; set; } // Last time stock was updated

        [DataType(DataType.DateTime)]
        public DateTime? LastStockTake { get; set; } // Last stock take date

        public bool IsActive { get; set; } = true;

        // Computed property for stock status
        public string StockStatus
        {
            get
            {
                if (QuantityOnHand <= 0)
                    return "Out of Stock";
                else if (QuantityOnHand <= ReorderLevel)
                    return "Low Stock";
                else
                    return "Well Stocked";
            }
        }
    }
}
