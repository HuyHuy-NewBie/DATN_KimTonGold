using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GoldManagementSystem.Models
{
    public static class PriceBookScopeOptions
    {
        public const string General = "General";
        public const string GoldBar = "GoldBar";
    }

    public static class GoldBarSerialStatusOptions
    {
        public const string Available = "Available";
        public const string Reserved = "Reserved";
        public const string Sold = "Sold";
        public const string Voided = "Voided";
    }

    public static class NhnnSubmissionStatusOptions
    {
        public const string NotReady = "NotReady";
        public const string Ready = "Ready";
        public const string Submitted = "Submitted";
        public const string Failed = "Failed";
    }

    public class BusinessLocation
    {
        [Key] public int Id { get; set; }
        public int BranchId { get; set; }
        public virtual Branch Branch { get; set; }
        [Required, StringLength(30)] public string Code { get; set; } = string.Empty;
        [Required, StringLength(200)] public string Name { get; set; } = string.Empty;
        [Required, StringLength(500)] public string Address { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public virtual ICollection<BusinessLicense> Licenses { get; set; } = new List<BusinessLicense>();
        public virtual ICollection<GoldBarSerial> GoldBarSerials { get; set; } = new List<GoldBarSerial>();
    }

    [Index(nameof(Number), IsUnique = true)]
    public class BusinessLicense
    {
        [Key] public int Id { get; set; }
        public int BusinessLocationId { get; set; }
        public virtual BusinessLocation BusinessLocation { get; set; }
        [Required, StringLength(50)] public string LicenseType { get; set; } = "GoldBarTrading";
        [Required, StringLength(100)] public string Number { get; set; } = string.Empty;
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        [StringLength(450)] public string VerifiedByUserId { get; set; }
        public virtual AppUser VerifiedByUser { get; set; }
    }

    public class CustomerKycProfile
    {
        [Key] public int Id { get; set; }
        public int BranchId { get; set; }
        public virtual Branch Branch { get; set; }
        [Required, StringLength(200)] public string FullName { get; set; } = string.Empty;
        [Required, StringLength(30)] public string IdentityType { get; set; } = "CCCD";
        [Required, StringLength(50)] public string IdentityNumber { get; set; } = string.Empty;
        [StringLength(30)] public string TaxCode { get; set; }
        [StringLength(20)] public string Phone { get; set; }
        [StringLength(500)] public string Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        [StringLength(450)] public string VerifiedByUserId { get; set; }
        public virtual AppUser VerifiedByUser { get; set; }
        [StringLength(100)] public string VerificationMethod { get; set; }
        [StringLength(500)] public string VerificationReference { get; set; }
        [Required, StringLength(450)] public string CreatedByUserId { get; set; } = string.Empty;
        public virtual AppUser CreatedByUser { get; set; }
        public DateTime RetainUntil { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    [Index(nameof(SerialNumber), IsUnique = true)]
    public class GoldBarSerial
    {
        [Key] public int Id { get; set; }
        [Required, StringLength(100)] public string SerialNumber { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        public int BusinessLocationId { get; set; }
        public virtual BusinessLocation BusinessLocation { get; set; }
        [Required, StringLength(50)] public string PurityCode { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18,4)")] public decimal GrossWeight { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal FineWeight { get; set; }
        [StringLength(100)] public string CertificateNumber { get; set; }
        [Required, StringLength(30)] public string Status { get; set; } = GoldBarSerialStatusOptions.Available;
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
        public DateTime RetainUntil { get; set; }
        [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    [Index(nameof(GoldBarSerialId), IsUnique = true)]
    [Index(nameof(OrderDetailId), IsUnique = true)]
    public class GoldBarSaleRecord
    {
        [Key] public int Id { get; set; }
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }
        public int OrderDetailId { get; set; }
        public virtual OrderDetail OrderDetail { get; set; }
        public int GoldBarSerialId { get; set; }
        public virtual GoldBarSerial GoldBarSerial { get; set; }
        public int CustomerKycProfileId { get; set; }
        public virtual CustomerKycProfile CustomerKycProfile { get; set; }
        public int BusinessLocationId { get; set; }
        public virtual BusinessLocation BusinessLocation { get; set; }
        public int PriceSnapshotId { get; set; }
        public virtual PriceSnapshot PriceSnapshot { get; set; }
        public DateTime SoldAt { get; set; } = DateTime.UtcNow;
        [Required, StringLength(30)] public string NhnnSubmissionStatus { get; set; } = NhnnSubmissionStatusOptions.NotReady;
        [StringLength(100)] public string NhnnReference { get; set; }
        [StringLength(1000)] public string NhnnFailureReason { get; set; }
        [Required, StringLength(450)] public string CreatedByUserId { get; set; } = string.Empty;
        public virtual AppUser CreatedByUser { get; set; }
        public DateTime RetainUntil { get; set; }
    }
}
