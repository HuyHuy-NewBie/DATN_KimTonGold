using System.ComponentModel.DataAnnotations;

namespace GoldManagementSystem.Models.ViewModels
{
    public sealed class PriceBookFormViewModel
    {
        [Required, StringLength(50)] public string Code { get; set; } = string.Empty;
        [Required, StringLength(200)] public string Name { get; set; } = string.Empty;
        [Required, StringLength(30)] public string Scope { get; set; } = PriceBookScopeOptions.General;
        public int? BranchId { get; set; }
        [Required] public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
        public DateTime? EffectiveTo { get; set; }
        [StringLength(1000)] public string Notes { get; set; }
        [Required, StringLength(30)] public string Version { get; set; } = "1.0";
        [StringLength(1000)] public string ChangeReason { get; set; }
        public List<PriceLineFormViewModel> Lines { get; set; } = new();
    }

    public sealed class PriceLineFormViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        [Range(0, 999999999999)] public decimal SellUnitPrice { get; set; }
        [Range(0, 999999999999)] public decimal BuyUnitPrice { get; set; }
        [Range(0, 999999999999)] public decimal ProcessingFee { get; set; }
        [Range(0, 100)] public decimal MaxDiscountRate { get; set; }
    }
}
