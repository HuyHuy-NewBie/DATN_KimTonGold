namespace GoldManagementSystem.Services
{
    public class AuthVerificationOptions
    {
        public bool RequireLoginVerification { get; set; } = true;
        public bool RequireRegistrationVerification { get; set; } = true;
        public bool RequireProfileContactVerification { get; set; } = true;
    }
}
