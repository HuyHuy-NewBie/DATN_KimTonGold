using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace GoldManagementSystem.Models.ViewModels
{
    public static class VerificationChannelOptions
    {
        public const string Email = "email";
        public const string Phone = "phone";

        public static bool IsValid(string value)
        {
            return string.Equals(value, Email, StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, Phone, StringComparison.OrdinalIgnoreCase);
        }
    }

    public class RegisterViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự.")]
        public string FullName { get; set; }

        public string Identifier { get; set; }

        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn nơi nhận mã xác nhận.")]
        public string VerificationChannel { get; set; } = VerificationChannelOptions.Email;

        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã xác nhận gồm 6 chữ số.")]
        public string VerificationCode { get; set; }

        public bool RequiresVerification { get; set; }
        public string VerificationFlowId { get; set; }
        public string VerificationDestinationDisplay { get; set; }
        public DateTimeOffset? VerificationExpiresAt { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Identifier)
                && string.IsNullOrWhiteSpace(Email)
                && string.IsNullOrWhiteSpace(PhoneNumber))
            {
                yield return new ValidationResult(
                    "Vui lòng nhập email hoặc số điện thoại để tạo tài khoản.",
                    new[] { nameof(Identifier) });
            }
        }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email hoặc số điện thoại.")]
        public string Identifier { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn nơi nhận mã xác nhận.")]
        public string VerificationChannel { get; set; } = VerificationChannelOptions.Email;

        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã xác nhận gồm 6 chữ số.")]
        public string VerificationCode { get; set; }

        public bool RequiresVerification { get; set; }
        public string VerificationFlowId { get; set; }
        public string VerificationDestinationDisplay { get; set; }
        public DateTimeOffset? VerificationExpiresAt { get; set; }
    }
}
