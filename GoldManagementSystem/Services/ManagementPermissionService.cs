using GoldManagementSystem.Data;
using GoldManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GoldManagementSystem.Services
{
    public interface IManagementPermissionService
    {
        Task<bool> CanAsync(ClaimsPrincipal principal, string featureKey, int? branchId = null);
        Task<HashSet<string>> GetGrantedKeysAsync(ClaimsPrincipal principal, int? branchId);
        Task<List<Branch>> GetAccessibleBranchesAsync(ClaimsPrincipal principal);
    }

    public sealed class ManagementPermissionService : IManagementPermissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ManagementPermissionService(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<bool> CanAsync(ClaimsPrincipal principal, string featureKey, int? branchId = null)
        {
            if (principal?.Identity?.IsAuthenticated != true) return false;
            if (principal.IsInRole(RoleCatalog.Admin)) return true;
            if (ManagementFeatureCatalog.IsSystemFeature(featureKey)) return false;

            var userId = _userManager.GetUserId(principal);
            if (string.IsNullOrWhiteSpace(userId)) return false;

            return await _context.UserFeaturePermissions.AsNoTracking().AnyAsync(permission =>
                permission.UserId == userId
                && permission.FeatureKey == featureKey
                && permission.IsGranted
                && (!branchId.HasValue || permission.BranchId == branchId.Value));
        }

        public async Task<HashSet<string>> GetGrantedKeysAsync(ClaimsPrincipal principal, int? branchId)
        {
            if (principal?.Identity?.IsAuthenticated != true) return new HashSet<string>();
            if (principal.IsInRole(RoleCatalog.Admin))
                return ManagementFeatureCatalog.All.Select(feature => feature.Key).ToHashSet();

            var userId = _userManager.GetUserId(principal);
            if (string.IsNullOrWhiteSpace(userId)) return new HashSet<string>();

            var query = _context.UserFeaturePermissions.AsNoTracking()
                .Where(permission => permission.UserId == userId && permission.IsGranted);
            if (branchId.HasValue) query = query.Where(permission => permission.BranchId == branchId.Value);
            return (await query.Select(permission => permission.FeatureKey).Distinct().ToListAsync()).ToHashSet();
        }

        public async Task<List<Branch>> GetAccessibleBranchesAsync(ClaimsPrincipal principal)
        {
            var branchQuery = _context.Branches.AsNoTracking().Where(branch => branch.IsActive);
            if (principal?.Identity?.IsAuthenticated != true) return new List<Branch>();
            if (principal.IsInRole(RoleCatalog.Admin))
                return await branchQuery.OrderBy(branch => branch.BranchName).ToListAsync();

            var userId = _userManager.GetUserId(principal);
            var branchIds = await _context.UserFeaturePermissions.AsNoTracking()
                .Where(permission => permission.UserId == userId
                    && permission.IsGranted
                    && permission.BranchId.HasValue
                    && !permission.FeatureKey.StartsWith("system."))
                .Select(permission => permission.BranchId.Value)
                .Distinct()
                .ToListAsync();

            return await branchQuery.Where(branch => branchIds.Contains(branch.Id))
                .OrderBy(branch => branch.BranchName).ToListAsync();
        }
    }
}
