namespace GoldManagementSystem.Services
{
    /// <summary>
    /// Configuration for the provider callback endpoint. Keep SigningSecret outside
    /// source control (environment variable or Secret Manager) in deployed systems.
    /// </summary>
    public sealed class OnlinePaymentWebhookOptions
    {
        public bool Enabled { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string SigningSecret { get; set; } = string.Empty;
        public string SignatureHeaderName { get; set; } = "X-Payment-Signature";
        public int MaxRequestBodyBytes { get; set; } = 16 * 1024;

        public bool IsConfigured =>
            Enabled
            && !string.IsNullOrWhiteSpace(Provider)
            && !string.IsNullOrWhiteSpace(SigningSecret)
            && !string.IsNullOrWhiteSpace(SignatureHeaderName);
    }
}
