using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoldManagementSystem.Models
{
    public class InventoryStocktake
    {
        public const string StatusCompleted = "Đã hoàn tất";

        [Key]
        public int Id { get; set; }

        [Required, StringLength(40)]
        public string StocktakeCode { get; set; } = string.Empty;

        public int WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }

        [Required]
        public string CreatedByUserId { get; set; } = string.Empty;
        public AppUser? CreatedByUser { get; set; }

        public DateTime CountedAt { get; set; } = DateTime.UtcNow;

        [Required, StringLength(50)]
        public string Status { get; set; } = StatusCompleted;

        public int TotalLines { get; set; }

        public int DifferenceLines { get; set; }

        [StringLength(1000)]
        public string? Note { get; set; }

        public ICollection<InventoryStocktakeDetail> Details { get; set; }
            = new List<InventoryStocktakeDetail>();
    }

    public class InventoryStocktakeDetail
    {
        [Key]
        public int Id { get; set; }

        public int InventoryStocktakeId { get; set; }
        public InventoryStocktake? InventoryStocktake { get; set; }

        public int InventoryItemId { get; set; }
        public InventoryItem? InventoryItem { get; set; }

        public int SystemQuantity { get; set; }

        public int ActualQuantity { get; set; }

        public int QuantityDifference { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SystemWeight { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ActualWeight { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? WeightDifference { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SystemCarat { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ActualCarat { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CaratDifference { get; set; }

        [StringLength(500)]
        public string? DifferenceNote { get; set; }
    }
}