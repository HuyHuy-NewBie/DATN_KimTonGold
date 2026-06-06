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
[Route("api/users")]
[Authorize(Policy = Policies.UsersManage)]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly PermissionService _permissionService;

    public UsersController(
        ApplicationDbContext context,
        UserManager<AppUser> userManager,
        PermissionService permissionService)
    {
        _context = context;
        _userManager = userManager;
        _permissionService = permissionService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserProfileDto>>> GetUsers(
        string? search = null,
        string? role = null,
        int? branchId = null,
        bool includeInactive = true)
    {
        var actor = await _permissionService.GetActorAsync(User);
        if (actor == null)
        {
            return Unauthorized(new ApiError("Tài khoản không còn hợp lệ."));
        }

        var users = await ApplyUserScope(_context.Users.Include(user => user.Branch).AsNoTracking(), actor)
            .Where(user => includeInactive || user.IsActive)
            .OrderBy(user => user.FullName)
            .ThenBy(user => user.Email)
            .Take(500)
            .ToListAsync();

        if (branchId.HasValue)
        {
            users = users.Where(user => user.BranchId == branchId.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            users = users
                .Where(user =>
                    Contains(user.FullName, keyword)
                    || Contains(user.Email, keyword)
                    || Contains(user.PhoneNumber, keyword)
                    || Contains(user.Branch?.BranchName, keyword))
                .ToList();
        }

        var result = new List<UserProfileDto>();
        foreach (var user in users)
        {
            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            if (!string.IsNullOrWhiteSpace(role)
                && !roles.Contains(role.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            result.Add(DtoMapper.ToUserProfile(user, roles, Array.Empty<string>()));
        }

        return result
            .OrderByDescending(user => RoleCatalog.GetPriority(user.HighestRole))
            .ThenBy(user => user.FullName)
            .ToList();
    }

    [HttpPost]
    public async Task<ActionResult<UserProfileDto>> Create(CreateUserRequest request)
    {
        var actor = await _permissionService.GetActorAsync(User);
        if (actor == null)
        {
            return Unauthorized(new ApiError("Tài khoản không còn hợp lệ."));
        }

        var fullName = Normalize(request.FullName);
        var email = Normalize(request.Email);
        var role = Normalize(request.Role);
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new ApiError("Vui lòng nhập đầy đủ họ tên, email và mật khẩu."));
        }

        if (!_permissionService.CanAssignRole(actor, role))
        {
            return Forbid();
        }

        if (await _userManager.FindByEmailAsync(email) != null)
        {
            return Conflict(new ApiError("Email này đã tồn tại trong hệ thống."));
        }

        var branchId = ResolveAssignableBranchId(actor, request.BranchId);
        if (branchId.HasValue && !await _context.Branches.AnyAsync(branch => branch.Id == branchId.Value && branch.IsActive))
        {
            return BadRequest(new ApiError("Chi nhánh không hợp lệ hoặc đang bị khóa."));
        }

        var user = new AppUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            BranchId = branchId,
            IsActive = true,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return BadRequest(new ApiError(BuildIdentityErrorMessage(createResult, "Không thể tạo tài khoản mới.")));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return BadRequest(new ApiError(BuildIdentityErrorMessage(roleResult, "Không thể gán vai trò cho tài khoản mới.")));
        }

        await _context.Entry(user).Reference(item => item.Branch).LoadAsync();
        return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, DtoMapper.ToUserProfile(user, new[] { role }, Array.Empty<string>()));
    }

    [HttpPut("{id}/role")]
    public async Task<ActionResult<UserProfileDto>> UpdateRole(string id, UpdateUserRoleRequest request)
    {
        var actor = await _permissionService.GetActorAsync(User);
        if (actor == null)
        {
            return Unauthorized(new ApiError("Tài khoản không còn hợp lệ."));
        }

        var target = await _context.Users.Include(user => user.Branch).FirstOrDefaultAsync(user => user.Id == id);
        if (target == null || !CanTouchUser(actor, target))
        {
            return NotFound(new ApiError("Không tìm thấy tài khoản trong phạm vi quản lý."));
        }

        if (string.Equals(actor.User.Id, target.Id, StringComparison.Ordinal))
        {
            return BadRequest(new ApiError("Bạn không thể tự đổi vai trò của chính mình trên mobile."));
        }

        var role = Normalize(request.Role);
        if (!_permissionService.CanAssignRole(actor, role))
        {
            return Forbid();
        }

        var existingRoles = await _userManager.GetRolesAsync(target);
        if (existingRoles.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(target, existingRoles);
            if (!removeResult.Succeeded)
            {
                return BadRequest(new ApiError(BuildIdentityErrorMessage(removeResult, "Không thể gỡ vai trò cũ của tài khoản.")));
            }
        }

        var addResult = await _userManager.AddToRoleAsync(target, role);
        if (!addResult.Succeeded)
        {
            return BadRequest(new ApiError(BuildIdentityErrorMessage(addResult, "Không thể cập nhật vai trò tài khoản.")));
        }

        return DtoMapper.ToUserProfile(target, new[] { role }, Array.Empty<string>());
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<UserProfileDto>> UpdateStatus(string id, UpdateUserStatusRequest request)
    {
        var actor = await _permissionService.GetActorAsync(User);
        if (actor == null)
        {
            return Unauthorized(new ApiError("Tài khoản không còn hợp lệ."));
        }

        var target = await _context.Users.Include(user => user.Branch).FirstOrDefaultAsync(user => user.Id == id);
        if (target == null || !CanTouchUser(actor, target))
        {
            return NotFound(new ApiError("Không tìm thấy tài khoản trong phạm vi quản lý."));
        }

        if (!request.IsActive && string.Equals(actor.User.Id, target.Id, StringComparison.Ordinal))
        {
            return BadRequest(new ApiError("Bạn không thể tự khóa tài khoản đang đăng nhập."));
        }

        var targetRoles = (await _userManager.GetRolesAsync(target)).ToList();
        var highestRole = RoleCatalog.GetHighestRole(targetRoles);
        if (!actor.IsAdmin && !_permissionService.CanAssignRole(actor, highestRole))
        {
            return Forbid();
        }

        target.IsActive = request.IsActive;
        var updateResult = await _userManager.UpdateAsync(target);
        if (!updateResult.Succeeded)
        {
            return BadRequest(new ApiError(BuildIdentityErrorMessage(updateResult, "Không thể cập nhật trạng thái tài khoản.")));
        }

        return DtoMapper.ToUserProfile(target, targetRoles, Array.Empty<string>());
    }

    private static IQueryable<AppUser> ApplyUserScope(IQueryable<AppUser> query, ActorContext actor)
    {
        if (actor.IsAdmin)
        {
            return query;
        }

        return actor.User.BranchId.HasValue
            ? query.Where(user => user.BranchId == actor.User.BranchId.Value)
            : query.Where(user => false);
    }

    private static bool CanTouchUser(ActorContext actor, AppUser target)
    {
        return actor.IsAdmin
            || (actor.User.BranchId.HasValue && target.BranchId == actor.User.BranchId.Value);
    }

    private static int? ResolveAssignableBranchId(ActorContext actor, int? requestedBranchId)
    {
        if (actor.IsAdmin)
        {
            return requestedBranchId;
        }

        return actor.User.BranchId;
    }

    private static bool Contains(string? value, string keyword)
    {
        return value?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true;
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    private static string BuildIdentityErrorMessage(IdentityResult result, string fallbackMessage)
    {
        var errorMessages = result.Errors
            .Select(error => error.Description)
            .Where(description => !string.IsNullOrWhiteSpace(description))
            .ToList();

        return errorMessages.Count > 0 ? string.Join(" ", errorMessages) : fallbackMessage;
    }
}
