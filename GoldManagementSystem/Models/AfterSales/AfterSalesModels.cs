using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GoldManagementSystem.Models;

public static class AfterSalesStatus
{
    public const string Draft = "Draft";
    public const string Quarantine = "Quarantine";
    public const string PendingApproval = "PendingApproval";
    public const string Approved = "Approved";
    public const string Paid = "Paid";
    public const string Rejected = "Rejected";
    public const string Requested = "Requested";
    public const string Received = "Received";
    public const string Inspecting = "Inspecting";
    public const string Repairing = "Repairing";
    public const string Completed = "Completed";
    public const string Returned = "Returned";
    public const string Cancelled = "Cancelled";
}

public static class AssayResultOptions
{
    public const string Pending = "Pending";
    public const string Passed = "Passed";
    public const string Failed = "Failed";
}

public static class RefundStatusOptions
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
}

[Index(nameof(CaseNumber), IsUnique = true)]
public class BuybackCase
{
    [Key] public int Id { get; set; }
    [Required, StringLength(40)] public string CaseNumber { get; set; } = $"BB-{Guid.NewGuid():N}"[..15].ToUpperInvariant();
    public int BranchId { get; set; }
    public virtual Branch Branch { get; set; }
    public int? ProductId { get; set; }
    public virtual Product Product { get; set; }
    public int? OrderDetailId { get; set; }
    public virtual OrderDetail OrderDetail { get; set; }
    [Required, StringLength(120)] public string CustomerName { get; set; } = string.Empty;
    [Required, StringLength(20)] public string CustomerPhone { get; set; } = string.Empty;
    [StringLength(30)] public string IdentityNumber { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal GrossWeight { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal FineWeight { get; set; }
    [Column(TypeName = "decimal(8,5)")] public decimal PurityRate { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal ProposedAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal ApprovedAmount { get; set; }
    [Required, StringLength(30)] public string Status { get; set; } = AfterSalesStatus.Draft;
    [Required, StringLength(30)] public string AssayStatus { get; set; } = AssayResultOptions.Pending;
    [Required, StringLength(450)] public string CreatedByUserId { get; set; } = string.Empty;
    public virtual AppUser CreatedByUser { get; set; }
    [StringLength(450)] public string ApprovedByUserId { get; set; }
    public virtual AppUser ApprovedByUser { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime RetainUntil { get; set; } = DateTime.UtcNow.AddYears(10);
    public virtual ICollection<BuybackAssay> Assays { get; set; } = new List<BuybackAssay>();
}

public class BuybackAssay
{
    [Key] public int Id { get; set; }
    public int BuybackCaseId { get; set; }
    public virtual BuybackCase BuybackCase { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal MeasuredGrossWeight { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal MeasuredFineWeight { get; set; }
    [Column(TypeName = "decimal(8,5)")] public decimal MeasuredPurityRate { get; set; }
    [Required, StringLength(20)] public string Result { get; set; } = AssayResultOptions.Pending;
    [Required, StringLength(450)] public string AssayedByUserId { get; set; } = string.Empty;
    public virtual AppUser AssayedByUser { get; set; }
    public DateTime AssayedAt { get; set; } = DateTime.UtcNow;
    [StringLength(500)] public string Note { get; set; }
}

[Index(nameof(CaseNumber), IsUnique = true)]
public class ReturnCase
{
    [Key] public int Id { get; set; }
    [Required, StringLength(40)] public string CaseNumber { get; set; } = $"RT-{Guid.NewGuid():N}"[..15].ToUpperInvariant();
    public int BranchId { get; set; }
    public virtual Branch Branch { get; set; }
    public int OrderId { get; set; }
    public virtual Order Order { get; set; }
    public int OrderDetailId { get; set; }
    public virtual OrderDetail OrderDetail { get; set; }
    [Required, StringLength(500)] public string Reason { get; set; } = string.Empty;
    [Required, StringLength(30)] public string Status { get; set; } = AfterSalesStatus.Requested;
    [Required, StringLength(450)] public string RequestedByUserId { get; set; } = string.Empty;
    [StringLength(450)] public string ApprovedByUserId { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime RetainUntil { get; set; } = DateTime.UtcNow.AddYears(10);
    public virtual Refund Refund { get; set; }
}

public class Refund
{
    [Key] public int Id { get; set; }
    [Required, StringLength(40)] public string RefundNumber { get; set; } = $"RF-{Guid.NewGuid():N}"[..15].ToUpperInvariant();
    public int ReturnCaseId { get; set; }
    public virtual ReturnCase ReturnCase { get; set; }
    public int? PaymentId { get; set; }
    public virtual Payment Payment { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
    [Required, StringLength(30)] public string Channel { get; set; } = PaymentChannelOptions.Cash;
    [Required, StringLength(30)] public string Status { get; set; } = RefundStatusOptions.Pending;
    [StringLength(100)] public string TransactionReference { get; set; }
    [Required, StringLength(450)] public string RequestedByUserId { get; set; } = string.Empty;
    [StringLength(450)] public string ProcessedByUserId { get; set; }
    public int RetryCount { get; set; }
    [StringLength(1000)] public string LastError { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}

[Index(nameof(CaseNumber), IsUnique = true)]
public class WarrantyCase
{
    [Key] public int Id { get; set; }
    [Required, StringLength(40)] public string CaseNumber { get; set; } = $"WA-{Guid.NewGuid():N}"[..15].ToUpperInvariant();
    public int BranchId { get; set; }
    public virtual Branch Branch { get; set; }
    public int OrderDetailId { get; set; }
    public virtual OrderDetail OrderDetail { get; set; }
    [Required, StringLength(120)] public string CustomerName { get; set; } = string.Empty;
    [Required, StringLength(20)] public string CustomerPhone { get; set; } = string.Empty;
    [Required, StringLength(1000)] public string IssueDescription { get; set; } = string.Empty;
    [Required, StringLength(30)] public string Status { get; set; } = AfterSalesStatus.Received;
    [Required, StringLength(450)] public string CreatedByUserId { get; set; } = string.Empty;
    public virtual AppUser CreatedByUser { get; set; }
    [StringLength(450)] public string AssignedToUserId { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DueAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime RetainUntil { get; set; } = DateTime.UtcNow.AddYears(10);
    public virtual ICollection<RepairCase> Repairs { get; set; } = new List<RepairCase>();
}

[Index(nameof(CaseNumber), IsUnique = true)]
public class RepairCase
{
    [Key] public int Id { get; set; }
    [Required, StringLength(40)] public string CaseNumber { get; set; } = $"RP-{Guid.NewGuid():N}"[..15].ToUpperInvariant();
    public int BranchId { get; set; }
    public virtual Branch Branch { get; set; }
    public int? WarrantyCaseId { get; set; }
    public virtual WarrantyCase WarrantyCase { get; set; }
    public int OrderDetailId { get; set; }
    public virtual OrderDetail OrderDetail { get; set; }
    [Required, StringLength(30)] public string Status { get; set; } = AfterSalesStatus.Received;
    [StringLength(1000)] public string Diagnosis { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal QuotedLaborCost { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal ApprovedAmount { get; set; }
    [Required, StringLength(450)] public string CreatedByUserId { get; set; } = string.Empty;
    public virtual AppUser CreatedByUser { get; set; }
    [StringLength(450)] public string ApprovedByUserId { get; set; }
    public virtual AppUser ApprovedByUser { get; set; }
    public DateTime? DueAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime RetainUntil { get; set; } = DateTime.UtcNow.AddYears(10);
}
