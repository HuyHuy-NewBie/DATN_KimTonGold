namespace MobileApi.Services;

public class JwtOptions
{
    public string Issuer { get; set; } = "GoldManagementMobileApi";
    public string Audience { get; set; } = "GoldManagementMobileApp";
    public string SigningKey { get; set; } = "CHANGE_ME_TO_A_LONG_RANDOM_SECRET_FOR_MOBILE_API";
    public int AccessTokenMinutes { get; set; } = 30;
    public int RememberDeviceDays { get; set; } = 30;
    public int SessionDeviceHours { get; set; } = 12;
}
