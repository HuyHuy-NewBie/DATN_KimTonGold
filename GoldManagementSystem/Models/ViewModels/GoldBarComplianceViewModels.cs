using System.ComponentModel.DataAnnotations;

namespace GoldManagementSystem.Models.ViewModels
{
    public sealed class GoldBarLocationInput
    {
        [Required, StringLength(30)] public string Code { get; set; }
        [Required, StringLength(200)] public string Name { get; set; }
        [Required, StringLength(500)] public string Address { get; set; }
        [Range(1, int.MaxValue)] public int BranchId { get; set; }
    }

    public sealed class GoldBarLicenseInput
    {
        [Range(1, int.MaxValue)] public int BusinessLocationId { get; set; }
        [Required, StringLength(100)] public string Number { get; set; }
        [Required] public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }

    public sealed class GoldBarSerialInput
    {
        [Range(1, int.MaxValue)] public int ProductId { get; set; }
        [Range(1, int.MaxValue)] public int BusinessLocationId { get; set; }
        [Required, StringLength(100)] public string SerialNumber { get; set; }
        [Required, StringLength(50)] public string PurityCode { get; set; }
        [Range(typeof(decimal), "0.0001", "999999999")] public decimal GrossWeight { get; set; }
        [Range(typeof(decimal), "0.0001", "999999999")] public decimal FineWeight { get; set; }
        [StringLength(100)] public string CertificateNumber { get; set; }
    }

    public sealed class RegisterGoldBarSaleInput
    {
        [Range(1, int.MaxValue)] public int OrderDetailId { get; set; }
        [Range(1, int.MaxValue)] public int GoldBarSerialId { get; set; }
        [Range(1, int.MaxValue)] public int CustomerKycProfileId { get; set; }
    }

    public sealed class CreateCustomerKycProfileInput
    {
        [Range(1, int.MaxValue)] public int BranchId { get; set; }
        [Required, StringLength(200)] public string FullName { get; set; }
        [Required, StringLength(30)] public string IdentityType { get; set; } = "CCCD";
        [Required, StringLength(50)] public string IdentityNumber { get; set; }
        [StringLength(30)] public string TaxCode { get; set; }
        [Required, StringLength(20)] public string Phone { get; set; }
        [Required, StringLength(500)] public string Address { get; set; }
        [Required] public DateTime? DateOfBirth { get; set; }
    }

    public sealed class VerifyCustomerKycProfileInput
    {
        [Range(1, int.MaxValue)] public int CustomerKycProfileId { get; set; }
        [Required, StringLength(100)] public string VerificationMethod { get; set; }
        [Required, StringLength(500)] public string VerificationReference { get; set; }
    }
}
