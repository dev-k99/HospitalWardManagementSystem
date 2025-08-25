using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WardSystemProject.Models
{
    public class StockTake
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Stock take date is required.")]
        [DataType(DataType.DateTime)]
        public DateTime StockTakeDate { get; set; }

        [Required(ErrorMessage = "Stock manager is required.")]
        [ForeignKey("StockManager")]
        public int StockManagerId { get; set; }

        public Staff StockManager { get; set; }

        [Required(ErrorMessage = "Ward is required.")]
        [ForeignKey("Ward")]
        public int WardId { get; set; }

        public Ward Ward { get; set; }

        [StringLength(1000, ErrorMessage = "Notes must not exceed 1000 characters.")]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
