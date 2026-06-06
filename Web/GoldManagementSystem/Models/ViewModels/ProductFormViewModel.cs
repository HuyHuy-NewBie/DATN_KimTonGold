using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

namespace GoldManagementSystem.Models.ViewModels
{
    public class ProductFormViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Danh mục")]
        public string Category { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Dòng sản phẩm")]
        public string ProductLine { get; set; } = GoldManagementSystem.Models.ProductLineOptions.Gold;

        [Required]
        [StringLength(20)]
        [Display(Name = "Chế độ phân loại")]
        public string CatalogMode { get; set; } = GoldManagementSystem.Models.ProductCatalogModeOptions.Single;

        [Display(Name = "Thể loại hiển thị")]
        public List<string> AssignedProductLines { get; set; } = new();

        [Required]
        [StringLength(100)]
        [Display(Name = "Chất liệu")]
        public string GoldType { get; set; } = string.Empty;

        [Display(Name = "Khối lượng")]
        [Range(0, 9999)]
        public decimal Weight { get; set; }

        [Display(Name = "Phí chế tác")]
        [Range(0, 999999999)]
        public decimal ProcessingFee { get; set; }

        [Display(Name = "Giá bán")]
        [Range(0, 999999999999)]
        public decimal SellPrice { get; set; }

        [Display(Name = "Giá mua")]
        [Range(0, 999999999999)]
        public decimal BuyPrice { get; set; }

        [Display(Name = "Chi nhánh")]
        public int BranchId { get; set; }

        [Display(Name = "Ảnh sản phẩm 1")]
        [Url]
        public string ImageUrl1 { get; set; } = string.Empty;

        [Display(Name = "Ảnh sản phẩm 2")]
        [Url]
        public string ImageUrl2 { get; set; } = string.Empty;

        [Display(Name = "Ảnh sản phẩm 3")]
        [Url]
        public string ImageUrl3 { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Còn hàng";

        [StringLength(2000)]
        [Display(Name = "Mô tả sản phẩm")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Hình dạng kim cương")]
        public string DiamondShape { get; set; } = string.Empty;

        [Display(Name = "Giác cắt")]
        public string DiamondCut { get; set; } = string.Empty;

        [Display(Name = "Màu sắc")]
        public string DiamondColor { get; set; } = string.Empty;

        [Display(Name = "Độ tinh khiết")]
        public string DiamondClarity { get; set; } = string.Empty;

        [Display(Name = "Carat")]
        [Range(0, 99)]
        public decimal? DiamondCarat { get; set; }

        [Display(Name = "Kích thước ly")]
        [Range(0, 99)]
        public decimal? DiamondSize { get; set; }

        [Display(Name = "Chứng nhận")]
        public string DiamondCertificate { get; set; } = string.Empty;

        public string DuplicateAction { get; set; } = string.Empty;

        public bool IncludesDiamondLine =>
            string.Equals(ProductLine, GoldManagementSystem.Models.ProductLineOptions.Diamond, System.StringComparison.OrdinalIgnoreCase)
            || AssignedProductLines.Any(line => string.Equals(line, GoldManagementSystem.Models.ProductLineOptions.Diamond, System.StringComparison.OrdinalIgnoreCase));
    }
}
