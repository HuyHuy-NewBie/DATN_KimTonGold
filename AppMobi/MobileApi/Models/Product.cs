using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MobileApi.Models;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string GoldType { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string ProductLine { get; set; } = ProductLineOptions.Gold;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Weight { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ProcessingFee { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SellPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal BuyPrice { get; set; }

    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public string? ImagesUrl { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? DiamondShape { get; set; }

    [StringLength(50)]
    public string? DiamondCut { get; set; }

    [StringLength(50)]
    public string? DiamondColor { get; set; }

    [StringLength(50)]
    public string? DiamondClarity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? DiamondCarat { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? DiamondSize { get; set; }

    [StringLength(100)]
    public string? DiamondCertificate { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = "Còn hàng";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public GoldProductCatalogEntry? GoldCatalogEntry { get; set; }
    public SilverProductCatalogEntry? SilverCatalogEntry { get; set; }
    public DiamondProductCatalogEntry? DiamondCatalogEntry { get; set; }
    public GoldSilverProductCatalogEntry? GoldSilverCatalogEntry { get; set; }
    public GoldDiamondProductCatalogEntry? GoldDiamondCatalogEntry { get; set; }
    public SilverDiamondProductCatalogEntry? SilverDiamondCatalogEntry { get; set; }
}
