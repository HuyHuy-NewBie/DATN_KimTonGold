using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GoldManagementSystem.Models
{
    public static class PaymentStatusOptions { public const string Pending = "Pending"; public const string Confirmed = "Confirmed"; public const string Failed = "Failed"; public const string Reversed = "Reversed"; }
    public static class PaymentChannelOptions { public const string Cash = "Cash"; public const string BankTransfer = "BankTransfer"; public const string QR = "QR"; }
    public static class EInvoiceStatusOptions { public const string Pending = "Pending"; public const string Sent = "Sent"; public const string Failed = "Failed"; public const string Cancelled = "Cancelled"; }

    [Index(nameof(TransactionReference), IsUnique = true)]
    public class Payment
    {
        [Key] public int Id { get; set; }
        [Required, StringLength(40)] public string PaymentNumber { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public virtual Branch Branch { get; set; }
        [Required, StringLength(30)] public string Channel { get; set; } = PaymentChannelOptions.Cash;
        [Required, StringLength(30)] public string Status { get; set; } = PaymentStatusOptions.Pending;
        [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
        [Required, StringLength(100)] public string TransactionReference { get; set; } = string.Empty;
        [StringLength(100)] public string Provider { get; set; }
        [StringLength(500)] public string FailureReason { get; set; }
        [Required, StringLength(450)] public string CreatedByUserId { get; set; } = string.Empty;
        public virtual AppUser CreatedByUser { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [StringLength(450)] public string ConfirmedByUserId { get; set; }
        public virtual AppUser ConfirmedByUser { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public virtual ICollection<PaymentAllocation> Allocations { get; set; } = new List<PaymentAllocation>();
        public virtual ICollection<BankReconciliation> Reconciliations { get; set; } = new List<BankReconciliation>();
        public virtual ICollection<CashFundEntry> CashEntries { get; set; } = new List<CashFundEntry>();
    }

    public class PaymentAllocation
    {
        [Key] public int Id { get; set; }
        public int PaymentId { get; set; }
        public virtual Payment Payment { get; set; }
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
        public DateTime AllocatedAt { get; set; } = DateTime.UtcNow;
    }

    [Index(nameof(PaymentId), nameof(StatementReference), IsUnique = true)]
    public class BankReconciliation
    {
        [Key] public int Id { get; set; }
        public int PaymentId { get; set; }
        public virtual Payment Payment { get; set; }
        [Required, StringLength(100)] public string StatementReference { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18,2)")] public decimal StatementAmount { get; set; }
        [Required, StringLength(30)] public string Status { get; set; } = "Matched";
        public DateTime ReconciledAt { get; set; } = DateTime.UtcNow;
        [Required, StringLength(450)] public string ReconciledByUserId { get; set; } = string.Empty;
        public virtual AppUser ReconciledByUser { get; set; }
        [StringLength(500)] public string Note { get; set; }
    }

    public class CashFundEntry
    {
        [Key] public int Id { get; set; }
        public int PaymentId { get; set; }
        public virtual Payment Payment { get; set; }
        public int BranchId { get; set; }
        public virtual Branch Branch { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
        [Required, StringLength(30)] public string EntryType { get; set; } = "Receipt";
        [Required, StringLength(450)] public string CreatedByUserId { get; set; } = string.Empty;
        public virtual AppUser CreatedByUser { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    [Index(nameof(OrderId), IsUnique = true)]
    public class EInvoice
    {
        [Key] public int Id { get; set; }
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }
        [Required, StringLength(40)] public string InvoiceNumber { get; set; } = string.Empty;
        [Required, StringLength(30)] public string Status { get; set; } = EInvoiceStatusOptions.Pending;
        public int AttemptCount { get; set; }
        public DateTime? LastAttemptAt { get; set; }
        [StringLength(100)] public string ProviderReference { get; set; }
        [StringLength(1000)] public string LastError { get; set; }
        public DateTime? IssuedAt { get; set; }
        [Required, StringLength(450)] public string CreatedByUserId { get; set; } = string.Empty;
        public virtual AppUser CreatedByUser { get; set; }
    }

    /// <summary>
    /// Payload received from a payment provider. The raw request body is authenticated
    /// by the webhook endpoint before any value in this object is trusted.
    /// </summary>
    public sealed class PaymentWebhookNotification
    {
        [Required, StringLength(100)] public string Provider { get; set; } = string.Empty;
        [Required, StringLength(100)] public string EventId { get; set; } = string.Empty;
        [Range(1, int.MaxValue)] public int OrderId { get; set; }
        [Required, StringLength(100)] public string TransactionReference { get; set; } = string.Empty;
        [Range(typeof(decimal), "0.01", "9999999999999999")] public decimal Amount { get; set; }
        [Required, StringLength(30)] public string Status { get; set; } = string.Empty;
    }
}
