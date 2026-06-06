using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MobileApi.Data;
using MobileApi.Models;

namespace MobileApi.Services;

public record ActorContext(AppUser User, IReadOnlyList<string> Roles)
{
    public bool IsAdmin => Roles.Contains(RoleCatalog.Admin);
    public bool IsBranchOwner => Roles.Contains(RoleCatalog.BranchOwner);
    public bool IsManager => Roles.Contains(RoleCatalog.Manager);
    public bool IsStaff => Roles.Contains(RoleCatalog.Staff);
    public bool IsAccountant => Roles.Contains(RoleCatalog.Accountant);
    public string HighestRole => RoleCatalog.GetHighestRole(Roles);
};

public class PermissionService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ApplicationDbContext _context;

    public PermissionService(UserManager<AppUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<ActorContext?> GetActorAsync(ClaimsPrincipal principal)
    {
        var userId = _userManager.GetUserId(principal);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var user = await _context.Users
            .Include(item => item.Branch)
            .FirstOrDefaultAsync(item => item.Id == userId);

        if (user == null || !user.IsActive)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        return new ActorContext(user, roles.ToList());
    }

    public IQueryable<Product> ApplyProductScope(IQueryable<Product> query, ActorContext actor)
    {
        if (actor.IsAdmin)
        {
            return query;
        }

        return actor.User.BranchId.HasValue
            ? query.Where(product => product.BranchId == actor.User.BranchId.Value)
            : query.Where(product => false);
    }

    public IQueryable<Order> ApplyOrderScope(IQueryable<Order> query, ActorContext actor)
    {
        if (actor.IsAdmin)
        {
            return query;
        }

        return actor.User.BranchId.HasValue
            ? query.Where(order => order.BranchId == actor.User.BranchId.Value)
            : query.Where(order => false);
    }

    public bool CanAssignRole(ActorContext actor, string role)
    {
        if (actor.IsAdmin)
        {
            return true;
        }

        if (string.Equals(role, RoleCatalog.Admin, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (actor.IsBranchOwner)
        {
            return !string.Equals(role, RoleCatalog.BranchOwner, StringComparison.OrdinalIgnoreCase);
        }

        if (actor.IsManager)
        {
            return string.Equals(role, RoleCatalog.Staff, StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, RoleCatalog.Accountant, StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, RoleCatalog.Customer, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    public IReadOnlyList<string> BuildPermissionCodes(ActorContext actor)
    {
        var permissions = new List<string> { "profile:read" };

        if (actor.IsAdmin || actor.IsBranchOwner || actor.IsManager || actor.IsStaff)
        {
            permissions.AddRange(new[] { "products:read", "products:write" });
        }

        if (actor.IsAdmin || actor.IsBranchOwner || actor.IsManager || actor.IsStaff || actor.IsAccountant)
        {
            permissions.AddRange(new[] { "branches:read", "orders:read" });
        }

        if (actor.IsAdmin || actor.IsBranchOwner || actor.IsManager)
        {
            permissions.AddRange(new[] { "orders:manage", "users:manage" });
        }

        if (actor.IsAdmin || actor.IsBranchOwner || actor.IsManager || actor.IsAccountant)
        {
            permissions.Add("reports:read");
        }

        if (actor.IsAdmin)
        {
            permissions.Add("branches:manage");
        }

        return permissions.Distinct(StringComparer.OrdinalIgnoreCase).Order().ToList();
    }
}
