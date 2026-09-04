using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GoldManagementSystem.Models
{
    public static class PosDocumentStatusOptions
    {
        public const string Draft = "Draft";
        public const string Sent = "Sent";
        public const string Accepted = "Accepted";
        public const string Converted = "Converted";
        public const string Expired = "Expired";
        public const string Rejected = "Rejected";
    }

    public static class DiscountApprovalStatusOptions
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
    }

    public static class DeliveryStatusOptions
    {
        public const string Pending = "Pending";
        public const string Shipped = "Shipped";
        public const string Delivered = "Delivered";
        public const string Failed = "Failed";
        public const string Cancelled = "Cancelled";
    }

    public static class PosInventoryReservationStatusOptions
    {
        public const string Reserved = "Reserved";
        public const string Released = "Released";
        public const string Issued = "Issued";
    }

    public class PosQuote
    {
        [Key] public int Id { get; set; }
        [Required, StringLength(40)] public string QuoteNumber { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public virtual Branch Branch { get; set; }
        [Required, StringLength(200)] public string CustomerName { get; set; } = string.Empty;
        [Required, StringLength(20)] public string CustomerPhone { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18,2)")] public decimal Subtotal { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal DiscountAmount { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal TotalAmount { get; set; }
        [Required, StringLength(30)] public string Status { get; set; } = PosDocumentStatusOptions.Draft;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        [Required, StringLength(450)] public string CreatedByUserId { get; set; } = string.Empty;
        public virtual AppUser CreatedByUser { get; set; }
        [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();
        public virtual ICollection<PosQuoteLine> Lines { get; set; } = new List<PosQuoteLine>();
        public virtual ICollection<DiscountApproval> DiscountApprovals { get; set; } = new List<DiscountApproval>();
    }

    [Index(nameof(PosQuoteId), nameof(ProductId), IsUnique = true)]
    public class PosQuoteLine
    {
        [Key] public int Id { get; set; }
        public int PosQuoteId { get; set; }
        public virtual PosQuote PosQuote { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        public int? PriceSnapshotId { get; set; }
        public virtual PriceSnapshot PriceSnapshot { get; set; }
        public int PriceBookId { get; set; }
        public int PriceVersionId { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal UnitPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal ProcessingFee { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal DiscountAmount { get; set; }
        [Column(TypeName = "decimal(5,2)")] public decimal MaxDiscountRate { get; set; }
        [NotMapped] public decimal LineTotal => Math.Max(0, UnitPrice * Quantity + ProcessingFee * Quantity - DiscountAmount);
    }

    public class DiscountApproval
    {
        [Key] public int Id { get; set; }
        public int? PosQuoteId { get; set; }
        public virtual PosQuote PosQuote { get; set; }
        public int? OrderId { get; set; }
        public virtual Order Order { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal RequestedAmount { get; set; }
        [Column(TypeName = "decimal(5,2)")] public decimal RequestedRate { get; set; }
        [Required, StringLength(500)] public string Reason { get; set; } = string.Empty;
        [Required, StringLength(30)] public string Status { get; set; } = DiscountApprovalStatusOptions.Pending;
        [Required, StringLength(450)] public string RequestedByUserId { get; set; } = string.Empty;
        public virtual AppUser RequestedByUser { get; set; }
        [StringLength(450)] public string ApprovedByUserId { get; set; }
        public virtual AppUser ApprovedByUser { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }
    }

    [Index(nameof(OrderId), IsUnique = true)]
    public class OrderDelivery
    {
        [Key] public int Id { get; set; }
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }
        [Required, StringLength(200)] public string RecipientName { get; set; } = string.Empty;
        [Required, StringLength(20)] public string RecipientPhone { get; set; } = string.Empty;
        [Required, StringLength(500)] public string Address { get; set; } = string.Empty;
        [StringLength(100)] public string Carrier { get; set; }
        [StringLength(100)] public string TrackingNumber { get; set; }
        [Required, StringLength(30)] public string Status { get; set; } = DeliveryStatusOptions.Pending;
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        [StringLength(500)] public string FailureReason { get; set; }
        public virtual ICollection<DeliveryEvidence> Evidences { get; set; } = new List<DeliveryEvidence>();
    }

    public class DeliveryEvidence
    {
        [Key] public int Id { get; set; }
        public int OrderDeliveryId { get; set; }
        public virtual OrderDelivery OrderDelivery { get; set; }
        [Required, StringLength(30)] public string EvidenceType { get; set; } = "ProofOfDelivery";
        [Required, StringLength(1000)] public string FileUrl { get; set; } = string.Empty;
        [Required, StringLength(128)] public string FileHash { get; set; } = string.Empty;
        [Required, StringLength(450)] public string UploadedByUserId { get; set; } = string.Empty;
        public virtual AppUser UploadedByUser { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }

    [Index(nameof(OrderId), nameof(InventoryItemId), IsUnique = true)]
    public class PosInventoryReservation
    {
        [Key] public int Id { get; set; }
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }
        public int InventoryItemId { get; set; }
        public virtual InventoryItem InventoryItem { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal ReservedWeight { get; set; }
        [Required, StringLength(30)] public string Status { get; set; } = PosInventoryReservationStatusOptions.Reserved;
        [Required, StringLength(450)] public string CreatedByUserId { get; set; } = string.Empty;
        public virtual AppUser CreatedByUser { get; set; }
        public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReleasedAt { get; set; }
        public DateTime? IssuedAt { get; set; }
    }
}
