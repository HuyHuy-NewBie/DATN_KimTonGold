using System.ComponentModel.DataAnnotations;

namespace GoldManagementSystem.Models.ViewModels
{
    public sealed class CreatePosQuoteInput
    {
        [Required, StringLength(200)] public string CustomerName { get; set; }
        [Required, StringLength(20)] public string CustomerPhone { get; set; }
        public int BranchId { get; set; }
        public List<PosQuoteLineInput> Lines { get; set; } = new();
        public int ValidHours { get; set; } = 24;
    }

    public sealed class PosQuoteLineInput
    {
        public int ProductId { get; set; }
        [Range(1, 999)] public int Quantity { get; set; }
    }

    public sealed class RequestDiscountInput
    {
        public int QuoteId { get; set; }
        [Range(0, 100)] public decimal RequestedRate { get; set; }
        [Required, StringLength(500)] public string Reason { get; set; }
    }

    public sealed class CreateDeliveryInput
    {
        public int OrderId { get; set; }
        [Required, StringLength(200)] public string RecipientName { get; set; }
        [Required, StringLength(20)] public string RecipientPhone { get; set; }
        [Required, StringLength(500)] public string Address { get; set; }
        [StringLength(100)] public string Carrier { get; set; }
        [StringLength(100)] public string TrackingNumber { get; set; }
    }
}
