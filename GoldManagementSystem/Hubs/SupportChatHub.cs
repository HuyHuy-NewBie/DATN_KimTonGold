using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GoldManagementSystem.Data;
using GoldManagementSystem.Models;
using GoldManagementSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GoldManagementSystem.Hubs
{
    /// <summary>
    /// Security helpers shared by the customer-care HTTP API and its SignalR hub.
    /// The raw guest capability is deliberately never persisted or sent in a JSON
    /// response; the browser receives it only as an HttpOnly same-site cookie.
    /// </summary>
    public static class SupportChatSecurity
    {
        public const string GuestSessionAccessCookieName = "GoldManagementSystem.CskhSessionAccess";
        public static readonly TimeSpan GuestSessionAccessLifetime = TimeSpan.FromDays(30);

        public static string CreateGuestAccessToken()
        {
            return Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        }

        public static string CreateSessionCode()
        {
            return "CHAT_" + Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        }

        public static string HashGuestAccessToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        }

        public static bool IsGuestAccessTokenValid(SupportChatSession session, string token, DateTime utcNow)
        {
            if (session == null
                || !string.IsNullOrWhiteSpace(session.CustomerId)
                || string.IsNullOrWhiteSpace(session.GuestAccessTokenHash)
                || !session.GuestAccessTokenExpiresAt.HasValue
                || session.GuestAccessTokenExpiresAt.Value <= utcNow
                || string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            byte[] storedHash;
            try
            {
                storedHash = Convert.FromHexString(session.GuestAccessTokenHash);
            }
            catch (FormatException)
            {
                return false;
            }

            var suppliedHash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return storedHash.Length == suppliedHash.Length
                && CryptographicOperations.FixedTimeEquals(storedHash, suppliedHash);
        }
    }

    /// <summary>
    /// The endpoint itself stays anonymous so a guest can receive live responses,
    /// but each group join is authorized against either the signed-in owner, the
    /// guest's opaque capability, or the staff member's branch permission.
    /// </summary>
    public class SupportChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IManagementPermissionService _permissions;

        public SupportChatHub(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            IManagementPermissionService permissions)
        {
            _context = context;
            _userManager = userManager;
            _permissions = permissions;
        }

        public static string GetSessionGroup(string sessionCode) => $"session:{sessionCode}";

        public static string GetStaffBranchGroup(string featureKey, int branchId) =>
            $"role:cskh:{featureKey}:branch:{branchId}";

        public static string GetStaffAdministratorGroup(string featureKey) =>
            $"role:cskh:{featureKey}:administrators";

        public static IReadOnlyList<string> GetStaffNotificationGroups(string featureKey, int? branchId)
        {
            var groups = new List<string> { GetStaffAdministratorGroup(featureKey) };
            if (branchId.HasValue && branchId.Value > 0)
            {
                groups.Add(GetStaffBranchGroup(featureKey, branchId.Value));
            }
            return groups;
        }

        public async Task JoinSessionGroup(string sessionCode)
        {
            var normalizedSessionCode = (sessionCode ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedSessionCode))
            {
                throw new HubException("Phiên chat không hợp lệ.");
            }

            var session = await _context.SupportChatSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.SessionCode == normalizedSessionCode);

            if (session == null || !await CanJoinSessionAsync(session))
            {
                // Do not distinguish an unknown session from one owned by another
                // visitor; doing so would turn the group API into an oracle.
                throw new HubException("Không có quyền truy cập phiên chat này.");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, GetSessionGroup(session.SessionCode));
        }

        public async Task LeaveSessionGroup(string sessionCode)
        {
            var normalizedSessionCode = (sessionCode ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(normalizedSessionCode))
            {
                // Removing a caller from a group is harmless even when they no
                // longer own the session, and avoids a database lookup on cleanup.
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetSessionGroup(normalizedSessionCode));
            }
        }

        public async Task JoinStaffGroup()
        {
            var currentUser = await GetAuthenticatedUserAsync();
            if (currentUser == null)
            {
                throw new HubException("Cần đăng nhập để nhận thông báo CSKH.");
            }

            var isAdministrator = await _userManager.IsInRoleAsync(currentUser, RoleCatalog.Admin);
            var isCustomerCare = await _userManager.IsInRoleAsync(currentUser, RoleCatalog.CustomerCare);
            if (!isAdministrator && !isCustomerCare)
            {
                throw new HubException("Tài khoản không có vai trò chăm sóc khách hàng.");
            }

            var joinedAnyGroup = false;
            foreach (var featureKey in new[]
            {
                ManagementFeatureCatalog.CustomerCareChat,
                ManagementFeatureCatalog.CustomerCareFeedback
            })
            {
                if (isAdministrator)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, GetStaffAdministratorGroup(featureKey));
                    joinedAnyGroup = true;
                    continue;
                }

                if (currentUser.BranchId.HasValue
                    && await _permissions.CanAsync(Context.User, featureKey, currentUser.BranchId.Value))
                {
                    await Groups.AddToGroupAsync(
                        Context.ConnectionId,
                        GetStaffBranchGroup(featureKey, currentUser.BranchId.Value));
                    joinedAnyGroup = true;
                }
            }

            if (!joinedAnyGroup)
            {
                throw new HubException("Tài khoản chưa được cấp quyền CSKH cho chi nhánh.");
            }
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrWhiteSpace(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
            }
            await base.OnConnectedAsync();
        }

        private async Task<bool> CanJoinSessionAsync(SupportChatSession session)
        {
            var currentUser = await GetAuthenticatedUserAsync();
            if (currentUser != null)
            {
                if (string.Equals(session.CustomerId, currentUser.Id, StringComparison.Ordinal))
                {
                    return true;
                }

                return await CanManageChatSessionAsync(currentUser, session);
            }

            var token = Context.GetHttpContext()?.Request.Cookies[SupportChatSecurity.GuestSessionAccessCookieName];
            return SupportChatSecurity.IsGuestAccessTokenValid(session, token, DateTime.UtcNow);
        }

        private async Task<bool> CanManageChatSessionAsync(AppUser currentUser, SupportChatSession session)
        {
            var isAdministrator = await _userManager.IsInRoleAsync(currentUser, RoleCatalog.Admin);
            if (isAdministrator) return true;

            if (!await _userManager.IsInRoleAsync(currentUser, RoleCatalog.CustomerCare)
                || !currentUser.BranchId.HasValue
                || !session.BranchId.HasValue
                || currentUser.BranchId.Value != session.BranchId.Value)
            {
                return false;
            }

            return await _permissions.CanAsync(
                Context.User,
                ManagementFeatureCatalog.CustomerCareChat,
                currentUser.BranchId.Value);
        }

        private async Task<AppUser> GetAuthenticatedUserAsync()
        {
            if (Context.User?.Identity?.IsAuthenticated != true) return null;
            return await _userManager.GetUserAsync(Context.User);
        }
    }
}
