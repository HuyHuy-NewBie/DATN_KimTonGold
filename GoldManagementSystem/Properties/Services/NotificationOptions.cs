namespace GoldManagementSystem.Services
{
    public class NotificationOptions
    {
        public EmailNotificationOptions Email { get; set; } = new();
        public SmsGatewayNotificationOptions Sms { get; set; } = new();
    }

    public class EmailNotificationOptions
    {
        public bool Enabled { get; set; }
        public string FromAddress { get; set; }
        public string FromName { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; } = 587;
        public bool UseSsl { get; set; } = true;
        public string Username { get; set; }
        public string Password { get; set; }

        public bool IsConfigured =>
            Enabled &&
            !string.IsNullOrWhiteSpace(FromAddress) &&
            !string.IsNullOrWhiteSpace(SmtpHost) &&
            SmtpPort > 0 &&
            !string.IsNullOrWhiteSpace(Username) &&
            !string.IsNullOrWhiteSpace(Password);
    }

    public class SmsGatewayNotificationOptions
    {
        public bool Enabled { get; set; }
        public string BaseUrl { get; set; } = "https://api.sms-gate.app";
        public string SendMessagePath { get; set; } = "/3rdparty/v1/messages";
        public string Username { get; set; }
        public string Password { get; set; }
        public string DeviceId { get; set; }
        public int? SimNumber { get; set; }
        public bool PreferPhoneForVerification { get; set; } = true;

        public bool IsConfigured =>
            Enabled &&
            !string.IsNullOrWhiteSpace(BaseUrl) &&
            !string.IsNullOrWhiteSpace(SendMessagePath) &&
            !string.IsNullOrWhiteSpace(Username) &&
            !string.IsNullOrWhiteSpace(Password);
    }
}
