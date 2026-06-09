using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobileApi.Contracts;
using MobileApi.Data;
using MobileApi.Models;
using MobileApi.Services;

namespace MobileApi.Controllers;

[ApiController]
[Route("api/devices")]
[Authorize(Policy = Policies.BackOffice)]
public class DevicesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly PermissionService _permissionService;

    public DevicesController(ApplicationDbContext context, PermissionService permissionService)
    {
        _context = context;
        _permissionService = permissionService;
    }

    [HttpPost("push-token")]
    public async Task<IActionResult> RegisterPushToken(RegisterDeviceRequest request)
    {
        var actor = await _permissionService.GetActorAsync(User);
        if (actor == null)
        {
            return Unauthorized(new ApiError("Tài khoản không còn hợp lệ."));
        }

        var deviceId = request.DeviceId?.Trim();
        var expoPushToken = request.ExpoPushToken?.Trim();
        if (string.IsNullOrWhiteSpace(deviceId) || string.IsNullOrWhiteSpace(expoPushToken))
        {
            return BadRequest(new ApiError("Thiếu mã thiết bị hoặc Expo push token."));
        }

        var existing = await _context.MobileDeviceTokens
            .FirstOrDefaultAsync(token => token.UserId == actor.User.Id && token.DeviceId == deviceId);

        if (existing == null)
        {
            _context.MobileDeviceTokens.Add(new MobileDeviceToken
            {
                UserId = actor.User.Id,
                DeviceId = deviceId,
                ExpoPushToken = expoPushToken,
                Platform = request.Platform?.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastSeenAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.ExpoPushToken = expoPushToken;
            existing.Platform = request.Platform?.Trim();
            existing.IsActive = true;
            existing.LastSeenAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }
}
