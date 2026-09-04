using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace GoldManagementSystem.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; } // E.g., Nhẫn, Dây chuyền

        [Required]
        [StringLength(100)]
        public string GoldType { get; set; } // E.g., Vàng 24K, 18K, 9999

        [Required]
        [StringLength(30)]
        public string ProductLine { get; set; } = ProductLineOptions.Gold;

        [Required, StringLength(20)]
        public string Material { get; set; } = ProductMaterialOptions.Gold;

        [Required, StringLength(30)]
        public string ProductForm { get; set; } = ProductFormOptions.Jewelry;

        [Required, StringLength(50)]
        public string ProductLegalClass { get; set; } = ProductLegalClassOptions.GoldJewelry;

        public int? PurityDefinitionId { get; set; }
        public virtual PurityDefinition PurityDefinition { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal PurityRate { get; set; }

        [Required, StringLength(20)]
        public string UnitOfMeasure { get; set; } = ProductUnitOfMeasureOptions.Piece;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Weight { get; set; } // Khối lượng vàng (chỉ/lượng)

        [Column(TypeName = "decimal(18,2)")]
        public decimal ProcessingFee { get; set; } // Tiền công (VNĐ)

        [Column(TypeName = "decimal(18,2)")]
        public decimal SellPrice { get; set; } // Giá bán tham khảo

        [Column(TypeName = "decimal(18,2)")]
        public decimal BuyPrice { get; set; } // Giá thu mua tham khảo

        public int BranchId { get; set; }
        public virtual Branch Branch { get; set; }

        public string ImagesUrl { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }

        [StringLength(50)]
        public string DiamondShape { get; set; }

        [StringLength(50)]
        public string DiamondCut { get; set; }

        [StringLength(50)]
        public string DiamondColor { get; set; }

        [StringLength(50)]
        public string DiamondClarity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiamondCarat { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiamondSize { get; set; }

        [StringLength(100)]
        public string DiamondCertificate { get; set; }

        [NotMapped]
        public IReadOnlyList<string> GalleryImages =>
            string.IsNullOrWhiteSpace(ImagesUrl)
                ? Array.Empty<string>()
                : ImagesUrl
                    .Split(new[] { ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(image => image.Trim())
                    .Where(image => !string.IsNullOrWhiteSpace(image))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

        [StringLength(50)]
        public string Status { get; set; } = "Còn hàng"; // Còn hàng, Hết hàng, Đã bán

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<ProductSpecVersion> SpecificationVersions { get; set; } = new List<ProductSpecVersion>();

        // Sản phẩm ưu tiên được đưa lên đầu các dashboard và danh mục bán hàng.
        public bool IsPriority { get; set; }
        public int PriorityOrder { get; set; }

        public virtual GoldProductCatalogEntry GoldCatalogEntry { get; set; }
        public virtual SilverProductCatalogEntry SilverCatalogEntry { get; set; }
        public virtual DiamondProductCatalogEntry DiamondCatalogEntry { get; set; }
        public virtual GoldSilverProductCatalogEntry GoldSilverCatalogEntry { get; set; }
        public virtual GoldDiamondProductCatalogEntry GoldDiamondCatalogEntry { get; set; }
        public virtual SilverDiamondProductCatalogEntry SilverDiamondCatalogEntry { get; set; }

        [NotMapped]
        public bool IsDiamondProduct => string.Equals(ProductLine, ProductLineOptions.Diamond, StringComparison.OrdinalIgnoreCase);
    }

    [Index(nameof(Code), IsUnique = true)]
    public class PurityDefinition
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(40)]
        public string Code { get; set; } = string.Empty;
        [Required, StringLength(20)]
        public string Material { get; set; } = ProductMaterialOptions.Gold;
        [Required, StringLength(120)]
        public string DisplayName { get; set; } = string.Empty;
        [Column(TypeName = "decimal(9,6)")]
        public decimal Rate { get; set; }
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Karat { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }

    [Index(nameof(ProductId), nameof(Version), IsUnique = true)]
    public class ProductSpecVersion
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        [Required, StringLength(30)]
        public string Version { get; set; } = "1.0";
        [Required, StringLength(20)]
        public string Material { get; set; } = ProductMaterialOptions.Gold;
        [Required, StringLength(30)]
        public string ProductForm { get; set; } = ProductFormOptions.Jewelry;
        [Required, StringLength(50)]
        public string ProductLegalClass { get; set; } = ProductLegalClassOptions.GoldJewelry;
        public int? PurityDefinitionId { get; set; }
        public virtual PurityDefinition PurityDefinition { get; set; }
        [Column(TypeName = "decimal(9,6)")]
        public decimal PurityRate { get; set; }
        [Required, StringLength(20)]
        public string UnitOfMeasure { get; set; } = ProductUnitOfMeasureOptions.Piece;
        [Column(TypeName = "decimal(18,4)")]
        public decimal GrossWeight { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal FineWeight { get; set; }
        [Required, StringLength(450)]
        public string CreatedByUserId { get; set; } = string.Empty;
        public virtual AppUser CreatedByUser { get; set; }
        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
        public DateTime? EffectiveTo { get; set; }
        [StringLength(1000)]
        public string ChangeReason { get; set; }
    }
}
