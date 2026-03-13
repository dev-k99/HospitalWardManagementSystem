using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WardSystemProject.Models
{
    /// <summary>
    /// Stores the per-consumable line items for a weekly stock take.
    /// Captures the system quantity at the time of the take and the
    /// physically counted quantity so that variances can be identified.
    /// </summary>
    public class StockTakeDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("StockTake")]
        public int StockTakeId { get; set; }
        public StockTake StockTake { get; set; } = null!;

        [Required]
        [ForeignKey("Consumable")]
        public int ConsumableId { get; set; }
        public Consumable Consumable { get; set; } = null!;

        /// <summary>Snapshot of QuantityOnHand at the moment the stock take was performed.</summary>
        [Required]
        public int SystemQuantity { get; set; }

        /// <summary>Quantity physically counted by the stock manager.</summary>
        [Required]
        [Range(0, 100000)]
        public int CountedQuantity { get; set; }

        /// <summary>Positive = surplus; negative = shortage.</summary>
        [NotMapped]
        public int Variance => CountedQuantity - SystemQuantity;
    }
}
