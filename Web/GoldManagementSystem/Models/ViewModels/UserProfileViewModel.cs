using System;
using System.ComponentModel.DataAnnotations;

namespace GoldManagementSystem.Models.ViewModels
{
    public class UserProfileViewModel : IValidatableObject
    {
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự.")]
        public string FullName { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
        public string PhoneNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(16, ErrorMessage = "Mã xác nhận không hợp lệ.")]
        public string PhoneVerificationCode { get; set; }

        [StringLength(16, ErrorMessage = "Mã xác nhận không hợp lệ.")]
        public string EmailVerificationCode { get; set; }

        public string PendingEmail { get; set; }
        public string PendingPhoneNumber { get; set; }
        public string PendingEmailDisplay { get; set; }
        public string PendingPhoneDisplay { get; set; }

        public bool HasPendingEmailChange => !string.IsNullOrWhiteSpace(PendingEmail);
        public bool HasPendingPhoneChange => !string.IsNullOrWhiteSpace(PendingPhoneNumber);

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(PhoneNumber))
            {
                yield return new ValidationResult(
                    "Tài khoản cần có ít nhất email hoặc số điện thoại để đăng nhập và nhận xác nhận.",
                    new[] { nameof(Email), nameof(PhoneNumber) });
            }
        }
    }
}
