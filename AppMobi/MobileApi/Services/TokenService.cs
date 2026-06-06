using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MobileApi.Models;

namespace MobileApi.Services;

public record AccessTokenResult(string AccessToken, DateTime ExpiresAt);
public record RefreshTokenResult(string RefreshToken, string TokenHash, DateTime ExpiresAt);

public class TokenService
{
    private readonly JwtOptions _options;
    private readonly UserManager<AppUser> _userManager;

    public TokenService(IOptions<JwtOptions> options, UserManager<AppUser> userManager)
    {
        _options = options.Value;
        _userManager = userManager;
    }

    public async Task<AccessTokenResult> CreateAccessTokenAsync(AppUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(_options.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? user.PhoneNumber ?? user.Id),
            new("fullName", user.FullName ?? string.Empty),
            new("branchId", user.BranchId?.ToString() ?? string.Empty)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: credentials);

        return new AccessTokenResult(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public RefreshTokenResult CreateRefreshToken(bool rememberDevice)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        var refreshToken = Convert.ToBase64String(randomBytes);
        var expiresAt = rememberDevice
            ? DateTime.UtcNow.AddDays(_options.RememberDeviceDays)
            : DateTime.UtcNow.AddHours(_options.SessionDeviceHours);

        return new RefreshTokenResult(refreshToken, HashRefreshToken(refreshToken), expiresAt);
    }

    public static string HashRefreshToken(string refreshToken)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToBase64String(hash);
    }
}
