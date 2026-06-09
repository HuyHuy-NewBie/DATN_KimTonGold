using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobileApi.Contracts;
using MobileApi.Data;
using MobileApi.Models;
using MobileApi.Services;

namespace MobileApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly TokenService _tokenService;
    private readonly PermissionService _permissionService;

    public AuthController(
        ApplicationDbContext context,
        UserManager<AppUser> userManager,
        TokenService tokenService,
        PermissionService permissionService)
    {
        _context = context;
        _userManager = userManager;
        _tokenService = tokenService;
        _permissionService = permissionService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var identifier = Normalize(request.Identifier);
        var deviceId = Normalize(request.DeviceId);
        if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(deviceId))
        {
            return BadRequest(new ApiError("Vui lòng nhập đủ tài khoản, mật khẩu và mã thiết bị."));
        }

        var user = await FindUserByIdentifierAsync(identifier);
        if (user == null || !user.IsActive || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized(new ApiError("Email/SĐT hoặc mật khẩu không đúng."));
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Any(role => RoleCatalog.BackOfficeRoles.Contains(role, StringComparer.OrdinalIgnoreCase)))
        {
            return Forbid();
        }

        return await CreateAuthResponseAsync(user, deviceId, request.RememberDevice);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request)
    {
        var deviceId = Normalize(request.DeviceId);
        if (string.IsNullOrWhiteSpace(request.RefreshToken) || string.IsNullOrWhiteSpace(deviceId))
        {
            return BadRequest(new ApiError("Thiếu refresh token hoặc mã thiết bị."));
        }

        var tokenHash = TokenService.HashRefreshToken(request.RefreshToken);
        var existingToken = await _context.MobileRefreshTokens
            .Include(token => token.User)
            .ThenInclude(user => user!.Branch)
            .FirstOrDefaultAsync(token =>
                token.TokenHash == tokenHash
                && token.DeviceId == deviceId
                && token.RevokedAt == null);

        if (existingToken == null || existingToken.ExpiresAt <= DateTime.UtcNow || existingToken.User == null || !existingToken.User.IsActive)
        {
            return Unauthorized(new ApiError("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại."));
        }

        var rememberDevice = existingToken.ExpiresAt > DateTime.UtcNow.AddHours(13);
        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.LastUsedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await CreateAuthResponseAsync(existingToken.User, deviceId, rememberDevice);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserProfileDto>> Me()
    {
        var actor = await _permissionService.GetActorAsync(User);
        if (actor == null)
        {
            return Unauthorized(new ApiError("Tài khoản không còn hợp lệ."));
        }

        return DtoMapper.ToUserProfile(actor.User, actor.Roles, _permissionService.BuildPermissionCodes(actor));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(LogoutRequest request)
    {
        var actor = await _permissionService.GetActorAsync(User);
        if (actor == null)
        {
            return Unauthorized(new ApiError("Tài khoản không còn hợp lệ."));
        }

        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            var tokenHash = TokenService.HashRefreshToken(request.RefreshToken);
            var token = await _context.MobileRefreshTokens
                .FirstOrDefaultAsync(item => item.UserId == actor.User.Id && item.TokenHash == tokenHash && item.RevokedAt == null);
            if (token != null)
            {
                token.RevokedAt = DateTime.UtcNow;
            }
        }

        var deviceId = Normalize(request.DeviceId);
        if (!string.IsNullOrWhiteSpace(deviceId))
        {
            var device = await _context.MobileDeviceTokens
                .FirstOrDefaultAsync(item => item.UserId == actor.User.Id && item.DeviceId == deviceId);
            if (device != null)
            {
                device.IsActive = false;
            }
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    private async Task<ActionResult<AuthResponse>> CreateAuthResponseAsync(AppUser user, string deviceId, bool rememberDevice)
    {
        await _context.Entry(user).Reference(item => item.Branch).LoadAsync();
        var accessToken = await _tokenService.CreateAccessTokenAsync(user);
        var refreshToken = _tokenService.CreateRefreshToken(rememberDevice);
        var userAgent = Request.Headers.UserAgent.ToString();
        var now = DateTime.UtcNow;

        var activeDeviceTokens = await _context.MobileRefreshTokens
            .Where(item => item.UserId == user.Id && item.DeviceId == deviceId && item.RevokedAt == null)
            .ToListAsync();
        foreach (var token in activeDeviceTokens)
        {
            token.RevokedAt = now;
        }

        _context.MobileRefreshTokens.Add(new MobileRefreshToken
        {
            UserId = user.Id,
            DeviceId = deviceId,
            TokenHash = refreshToken.TokenHash,
            ExpiresAt = refreshToken.ExpiresAt,
            CreatedAt = now,
            UserAgent = string.IsNullOrWhiteSpace(userAgent) ? null : userAgent[..Math.Min(userAgent.Length, 300)]
        });

        await _context.SaveChangesAsync();

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var actor = new ActorContext(user, roles);
        return new AuthResponse(
            accessToken.AccessToken,
            accessToken.ExpiresAt,
            refreshToken.RefreshToken,
            refreshToken.ExpiresAt,
            DtoMapper.ToUserProfile(user, roles, _permissionService.BuildPermissionCodes(actor)));
    }

    private async Task<AppUser?> FindUserByIdentifierAsync(string identifier)
    {
        if (identifier.Contains('@'))
        {
            return await _context.Users
                .Include(user => user.Branch)
                .FirstOrDefaultAsync(user => user.NormalizedEmail == identifier.ToUpper());
        }

        return await _context.Users
            .Include(user => user.Branch)
            .FirstOrDefaultAsync(user => user.PhoneNumber == identifier);
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }
}
