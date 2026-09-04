using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GoldManagementSystem.Models
{
    public static class PriceBookStatusOptions
    {
        public const string Draft = "Draft";
        public const string PendingApproval = "PendingApproval";
        public const string Published = "Published";
        public const string Expired = "Expired";
        public static readonly string[] All = { Draft, PendingApproval, Published, Expired };
    }

    public class PriceBook
    {
        [Key] public int Id { get; set; }
        [Required, StringLength(50)] public string Code { get; set; } = string.Empty;
        [Required, StringLength(200)] public string Name { get; set; } = string.Empty;
        [Required, StringLength(30)] public string Scope { get; set; } = PriceBookScopeOptions.General;
        public int? BranchId { get; set; }
        public virtual Branch Branch { get; set; }
        [Required, StringLength(30)] public string Status { get; set; } = PriceBookStatusOptions.Draft;
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        [StringLength(450)] public string CreatedByUserId { get; set; }
        public virtual AppUser CreatedByUser { get; set; }
        [StringLength(450)] public string SubmittedByUserId { get; set; }
        public virtual AppUser SubmittedByUser { get; set; }
        public DateTime? SubmittedAt { get; set; }
        [StringLength(450)] public string ApprovedByUserId { get; set; }
        public virtual AppUser ApprovedByUser { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [StringLength(1000)] public string Notes { get; set; }
        public virtual ICollection<PriceVersion> Versions { get; set; } = new List<PriceVersion>();
    }

    public class PriceVersion
    {
        [Key] public int Id { get; set; }
        public int PriceBookId { get; set; }
        public virtual PriceBook PriceBook { get; set; }
        [Required, StringLength(30)] public string Version { get; set; } = "1.0";
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        [StringLength(450)] public string CreatedByUserId { get; set; }
        public virtual AppUser CreatedByUser { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [StringLength(1000)] public string ChangeReason { get; set; }
        public virtual ICollection<PriceLine> Lines { get; set; } = new List<PriceLine>();
    }

    [Index(nameof(PriceVersionId), nameof(ProductId), IsUnique = true)]
    public class PriceLine
    {
        [Key] public int Id { get; set; }
        public int PriceVersionId { get; set; }
        public virtual PriceVersion PriceVersion { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal SellUnitPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal BuyUnitPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal ProcessingFee { get; set; }
        [Column(TypeName = "decimal(5,2)")] public decimal MaxDiscountRate { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class PriceSnapshot
    {
        [Key] public int Id { get; set; }
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        public int PriceBookId { get; set; }
        public int PriceVersionId { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal SellUnitPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal BuyUnitPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal ProcessingFee { get; set; }
        [Column(TypeName = "decimal(5,2)")] public decimal MaxDiscountRate { get; set; }
        public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
        [Required, StringLength(450)] public string CapturedByUserId { get; set; } = string.Empty;
        public virtual AppUser CapturedByUser { get; set; }
    }
}
