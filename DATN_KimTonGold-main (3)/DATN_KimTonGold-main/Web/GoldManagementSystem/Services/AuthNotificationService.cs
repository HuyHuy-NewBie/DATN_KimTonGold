using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace GoldManagementSystem.Services
{
    public class AuthNotificationService
    {
        private readonly NotificationOptions _options;
        private readonly ILogger<AuthNotificationService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthNotificationService(
            IOptions<NotificationOptions> options,
            ILogger<AuthNotificationService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _options = options.Value ?? new NotificationOptions();
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task SendLoginNotificationAsync(string emailOrPhone, string userFullName)
        {
            string message = $"Kính chào {userFullName}, tài khoản của bạn vừa đăng nhập thành công vào hệ thống. Nếu không phải bạn, vui lòng liên hệ Admin.";
            await SendNotificationAsync("[THÔNG BÁO BẢO MẬT]", "Thong bao bao mat GoldSys", ConsoleColor.Blue, emailOrPhone, message);
        }

        public async Task SendRegisterNotificationAsync(string emailOrPhone, string userFullName)
        {
            string message = $"Chúc mừng {userFullName} đã đăng ký tài khoản thành công. Tài khoản đang chờ cấp quyền.";
            await SendNotificationAsync("[THÔNG BÁO ĐĂNG KÝ PENDING]", "Dang ky tai khoan GoldSys thanh cong", ConsoleColor.DarkGreen, emailOrPhone, message);
        }

        public async Task SendRegisterVerificationCodeAsync(string emailOrPhone, string userFullName, string verificationCode)
        {
            string message = $"{userFullName}, mã xác nhận tạo tài khoản GoldSys của bạn là: {verificationCode}";
            await SendNotificationAsync("[XÁC NHẬN TẠO TÀI KHOẢN]", "Ma xac nhan tao tai khoan GoldSys", ConsoleColor.DarkCyan, emailOrPhone, message);
        }

        public async Task SendLoginVerificationCodeAsync(string emailOrPhone, string userFullName, string verificationCode)
        {
            string message = $"{userFullName}, mã xác nhận đăng nhập GoldSys của bạn là: {verificationCode}";
            await SendNotificationAsync("[XÁC NHẬN ĐĂNG NHẬP]", "Ma xac nhan dang nhap GoldSys", ConsoleColor.DarkBlue, emailOrPhone, message);
        }

        public async Task SendEmailChangeRequestedAsync(string currentEmail, string userFullName, string newEmail)
        {
            string message = $"{userFullName} vừa yêu cầu đổi email tài khoản sang {newEmail}. Nếu không phải bạn, vui lòng kiểm tra lại ngay.";
            await SendNotificationAsync("[YÊU CẦU ĐỔI EMAIL]", "Yeu cau doi email GoldSys", ConsoleColor.DarkYellow, currentEmail, message);
        }

        public async Task SendEmailChangeConfirmationAsync(string newEmail, string userFullName, string verificationCode)
        {
            string message = $"{userFullName}, mã xác nhận email mới của bạn là: {verificationCode}";
            await SendNotificationAsync("[XÁC NHẬN EMAIL MỚI]", "Xac nhan email moi GoldSys", ConsoleColor.DarkCyan, newEmail, message);
        }

        public async Task SendEmailChangedSuccessfullyAsync(string email, string userFullName, string newEmail)
        {
            string message = $"{userFullName}, email tài khoản của bạn đã được cập nhật thành công sang {newEmail}.";
            await SendNotificationAsync("[EMAIL ĐÃ ĐƯỢC CẬP NHẬT]", "Email tai khoan da duoc cap nhat", ConsoleColor.DarkMagenta, email, message);
        }

        public async Task SendPhoneChangeRequestedAsync(string accountEmail, string userFullName, string newPhoneNumber)
        {
            string message = $"{userFullName} vừa yêu cầu đổi số điện thoại sang {newPhoneNumber}. Hệ thống đã gửi mã xác nhận tới email tài khoản để xác thực yêu cầu này.";
            await SendNotificationAsync("[YÊU CẦU ĐỔI SỐ ĐIỆN THOẠI]", "Yeu cau doi so dien thoai GoldSys", ConsoleColor.DarkYellow, accountEmail, message);
        }

        public async Task SendPhoneVerificationCodeAsync(string newPhoneNumber, string userFullName, string verificationCode)
        {
            string message = $"{userFullName}, mã xác nhận số điện thoại mới của bạn là: {verificationCode}";
            await SendNotificationAsync("[MÃ XÁC NHẬN SỐ ĐIỆN THOẠI]", "Ma xac nhan so dien thoai GoldSys", ConsoleColor.DarkBlue, newPhoneNumber, message);
        }

        public async Task SendPhoneChangedEmailNotificationAsync(string accountEmail, string userFullName, string newPhoneNumber)
        {
            string message = $"{userFullName}, số điện thoại tài khoản của bạn đã được cập nhật thành công thành {newPhoneNumber}.";
            await SendNotificationAsync("[SỐ ĐIỆN THOẠI ĐÃ ĐƯỢC CẬP NHẬT]", "So dien thoai tai khoan da duoc cap nhat", ConsoleColor.DarkGreen, accountEmail, message);
        }

        public async Task SendPhoneChangeVerificationCodeByEmailAsync(string accountEmail, string userFullName, string newPhoneNumber, string verificationCode)
        {
            string message = $"{userFullName}, mã xác nhận đổi số điện thoại sang {newPhoneNumber} của bạn là: {verificationCode}";
            await SendNotificationAsync("[XÁC NHẬN ĐỔI SỐ ĐIỆN THOẠI]", "Ma xac nhan doi so dien thoai GoldSys", ConsoleColor.DarkBlue, accountEmail, message);
        }

        public async Task SendPhoneChangedSmsNotificationAsync(string newPhoneNumber, string userFullName)
        {
            string message = $"{userFullName}, số điện thoại này đã được liên kết thành công với tài khoản GoldSys.";
            await SendNotificationAsync("[XÁC NHẬN SỐ ĐIỆN THOẠI MỚI]", "So dien thoai moi da duoc xac nhan", ConsoleColor.DarkGreen, newPhoneNumber, message);
        }

        private async Task SendNotificationAsync(string title, string subject, ConsoleColor backgroundColor, string destination, string message)
        {
            var sentSuccessfully = LooksLikeEmail(destination)
                ? await TrySendEmailAsync(destination, subject, message)
                : await TrySendSmsAsync(destination, message);

            if (!sentSuccessfully)
            {
                await LogNotificationAsync(title, backgroundColor, destination, message);
            }
        }

        private async Task<bool> TrySendEmailAsync(string destination, string subject, string message)
        {
            if (!_options.Email.IsConfigured)
            {
                _logger.LogInformation("Email notification fallback because SMTP is not configured.");
                return false;
            }

            try
            {
                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_options.Email.FromAddress, _options.Email.FromName ?? "GoldSys"),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = false
                };
                mailMessage.To.Add(destination);

                using var smtpClient = new SmtpClient(_options.Email.SmtpHost, _options.Email.SmtpPort)
                {
                    EnableSsl = _options.Email.UseSsl
                };

                if (!string.IsNullOrWhiteSpace(_options.Email.Username))
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(_options.Email.Username, _options.Email.Password);
                }

                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to send email notification to {Destination}.", destination);
                return false;
            }
        }

        private async Task<bool> TrySendSmsAsync(string destination, string message)
        {
            if (!_options.Sms.IsConfigured)
            {
                _logger.LogInformation("SMS notification fallback because SMS Android Gateway is not configured.");
                return false;
            }

            try
            {
                var requestBody = new SmsGatewaySendRequest
                {
                    DeviceId = ContactUtility.Normalize(_options.Sms.DeviceId),
                    PhoneNumbers = new[] { destination },
                    TextMessage = new SmsGatewayTextMessage
                    {
                        Text = message
                    },
                    SimNumber = _options.Sms.SimNumber
                };

                using var client = _httpClientFactory.CreateClient();
                var requestUri = new Uri(new Uri(_options.Sms.BaseUrl.TrimEnd('/') + "/"), _options.Sms.SendMessagePath.TrimStart('/'));
                using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
                        {
                            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                        }),
                        Encoding.UTF8,
                        "application/json")
                };

                var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.Sms.Username}:{_options.Sms.Password}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);

                using var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Unable to send SMS notification to {Destination}. StatusCode={StatusCode}. Response={Response}",
                    destination,
                    (int)response.StatusCode,
                    errorBody);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to send SMS notification to {Destination}.", destination);
                return false;
            }
        }

        public bool LooksLikeEmail(string destination)
        {
            return ContactUtility.LooksLikeEmail(destination);
        }

        private static Task LogNotificationAsync(string title, ConsoleColor backgroundColor, string destination, string message)
        {
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n==============================================");
            Console.WriteLine($"{title} -> Gửi tới: {destination}");
            Console.WriteLine(message);
            Console.WriteLine("==============================================\n");
            Console.ResetColor();

            return Task.CompletedTask;
        }

        private sealed class SmsGatewaySendRequest
        {
            public string DeviceId { get; set; }
            public SmsGatewayTextMessage TextMessage { get; set; }
            public string[] PhoneNumbers { get; set; }
            public int? SimNumber { get; set; }
        }

        private sealed class SmsGatewayTextMessage
        {
            public string Text { get; set; }
        }
    }
}
