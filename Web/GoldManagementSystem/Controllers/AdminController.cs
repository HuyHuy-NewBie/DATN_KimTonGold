using GoldManagementSystem.Data;
using GoldManagementSystem.Models;
using GoldManagementSystem.Models.ViewModels;
using GoldManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoldManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Manager,Branch Owner,Staff")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AuthNotificationService _notificationService;

        public AdminController(ApplicationDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, AuthNotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _notificationService = notificationService;
        }

        // 1. Dashboard Quản lý
        [Authorize(Roles = "Admin,Manager,Branch Owner")]
        public async Task<IActionResult> Dashboard()
        {
            var totalOrders = await _context.Orders.CountAsync();
            var totalRevenue = await _context.Orders
                .Where(o => o.Status == "Hoàn thành")
                .Select(o => (decimal?)o.TotalAmount)
                .SumAsync() ?? 0m;
            var totalUsers = await _userManager.Users.CountAsync();
            var totalProducts = await _context.Products.CountAsync();

            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.RecentOrders = recentOrders;

            return View();
        }

        [Authorize(Roles = "Admin,Manager,Branch Owner")]
        public async Task<IActionResult> BranchManagement()
        {
            return View(await BuildBranchManagementViewModelAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Branch Owner")]
        public async Task<IActionResult> CreateBranch(BranchManagementViewModel model)
        {
            model.BranchName = NormalizeOrEmpty(model.BranchName);
            model.Address = NormalizeOrEmpty(model.Address);
            model.OwnerUserId = NormalizeOrEmpty(model.OwnerUserId);
            model.ManagerUserId = NormalizeOrEmpty(model.ManagerUserId);

            var hasExistingBranch = await _context.Branches.AnyAsync(branch =>
                branch.BranchName.ToLower() == model.BranchName.ToLower());

            if (hasExistingBranch)
            {
                ModelState.AddModelError(nameof(model.BranchName), "Tên chi nhánh này đã tồn tại.");
            }

            if (!string.IsNullOrWhiteSpace(model.OwnerUserId)
                && string.Equals(model.OwnerUserId, model.ManagerUserId, StringComparison.Ordinal))
            {
                ModelState.AddModelError(nameof(model.ManagerUserId), "Chủ chi nhánh và quản lí không thể là cùng một tài khoản.");
            }

            if (!ModelState.IsValid)
            {
                var viewModel = await BuildBranchManagementViewModelAsync(model);
                return View(nameof(BranchManagement), viewModel);
            }

            _context.Branches.Add(new Branch
            {
                BranchName = model.BranchName,
                Address = model.Address,
                PhoneNumber = string.Empty,
                IsActive = true
            });

            await _context.SaveChangesAsync();
            var createdBranch = await _context.Branches
                .OrderByDescending(branch => branch.Id)
                .FirstAsync(branch => branch.BranchName == model.BranchName);

            await AssignBranchToUserIfSelectedAsync(model.OwnerUserId, createdBranch.Id, RoleCatalog.BranchOwner);
            await AssignBranchToUserIfSelectedAsync(model.ManagerUserId, createdBranch.Id, RoleCatalog.Manager);

            TempData["SuccessMessage"] = $"Đã thêm chi nhánh {model.BranchName}.";
            return RedirectToAction(nameof(BranchManagement));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Branch Owner")]
        public async Task<IActionResult> ToggleBranchStatus(int branchId)
        {
            var branch = await _context.Branches.FindAsync(branchId);
            if (branch == null)
            {
                return NotFound();
            }

            branch.IsActive = !branch.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = branch.IsActive
                ? $"Đã kích hoạt lại chi nhánh {branch.BranchName}."
                : $"Đã tạm khóa chi nhánh {branch.BranchName}.";

            return RedirectToAction(nameof(BranchManagement));
        }

        [Authorize(Roles = "Admin,Manager,Branch Owner")]
        public async Task<IActionResult> BranchTeam(int branchId)
        {
            var actor = await GetCurrentActorAsync();
            var branch = await _context.Branches.FirstOrDefaultAsync(item => item.Id == branchId);
            if (branch == null)
            {
                return NotFound();
            }

            var manageableRoles = GetManageableBranchRoles(actor, branchId);
            if (manageableRoles.Count == 0)
            {
                return Forbid();
            }

            return View(await BuildBranchTeamViewModelAsync(branch, actor, manageableRoles));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Branch Owner")]
        public async Task<IActionResult> AddExistingMember(int branchId, BranchTeamViewModel model)
        {
            var actor = await GetCurrentActorAsync();
            var branch = await _context.Branches.FirstOrDefaultAsync(item => item.Id == branchId);
            if (branch == null)
            {
                return NotFound();
            }

            var manageableRoles = GetManageableBranchRoles(actor, branchId);
            if (manageableRoles.Count == 0)
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(model.ExistingUserId))
            {
                TempData["ErrorMessage"] = "Vui lòng chọn tài khoản cần thêm vào chi nhánh.";
                return RedirectToAction(nameof(BranchTeam), new { branchId });
            }

            var targetUser = await _userManager.FindByIdAsync(model.ExistingUserId);
            if (targetUser == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài khoản được chọn.";
                return RedirectToAction(nameof(BranchTeam), new { branchId });
            }

            var targetRoles = await _userManager.GetRolesAsync(targetUser);
            var highestRole = RoleCatalog.GetHighestRole(targetRoles);
            if (!manageableRoles.Contains(highestRole))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thêm tài khoản này vào chi nhánh.";
                return RedirectToAction(nameof(BranchTeam), new { branchId });
            }

            targetUser.BranchId = branchId;
            var updateResult = await _userManager.UpdateAsync(targetUser);
            if (!updateResult.Succeeded)
            {
                TempData["ErrorMessage"] = BuildIdentityErrorMessage(updateResult, "Không thể thêm tài khoản vào chi nhánh.");
                return RedirectToAction(nameof(BranchTeam), new { branchId });
            }

            TempData["SuccessMessage"] = $"Đã thêm {targetUser.FullName} vào chi nhánh {branch.BranchName}.";
            return RedirectToAction(nameof(BranchTeam), new { branchId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Branch Owner")]
        public async Task<IActionResult> CreateBranchMember(int branchId, BranchTeamViewModel model)
        {
            model.NewMemberFullName = NormalizeOrEmpty(model.NewMemberFullName);
            model.NewMemberEmail = NormalizeOrEmpty(model.NewMemberEmail);
            model.NewMemberRole = NormalizeOrEmpty(model.NewMemberRole);

            var actor = await GetCurrentActorAsync();
            var branch = await _context.Branches.FirstOrDefaultAsync(item => item.Id == branchId);
            if (branch == null)
            {
                return NotFound();
            }

            var manageableRoles = GetManageableBranchRoles(actor, branchId);
            if (manageableRoles.Count == 0)
            {
                return Forbid();
            }

            if (!manageableRoles.Contains(model.NewMemberRole))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền tạo vai trò này cho chi nhánh.";
                return RedirectToAction(nameof(BranchTeam), new { branchId });
            }

            if (string.IsNullOrWhiteSpace(model.NewMemberFullName)
                || string.IsNullOrWhiteSpace(model.NewMemberEmail)
                || string.IsNullOrWhiteSpace(model.NewMemberPassword))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ họ tên, email và mật khẩu khởi tạo.";
                return RedirectToAction(nameof(BranchTeam), new { branchId });
            }

            if (await _userManager.FindByEmailAsync(model.NewMemberEmail) != null)
            {
                TempData["ErrorMessage"] = "Email này đã tồn tại trong hệ thống.";
                return RedirectToAction(nameof(BranchTeam), new { branchId });
            }

            var user = new AppUser
            {
                UserName = model.NewMemberEmail,
                Email = model.NewMemberEmail,
                FullName = model.NewMemberFullName,
                BranchId = branchId,
                IsActive = true,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user, model.NewMemberPassword);
            if (!createResult.Succeeded)
            {
                TempData["ErrorMessage"] = BuildIdentityErrorMessage(createResult, "Không thể tạo tài khoản mới cho chi nhánh.");
                return RedirectToAction(nameof(BranchTeam), new { branchId });
            }

            var roleResult = await _userManager.AddToRoleAsync(user, model.NewMemberRole);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                TempData["ErrorMessage"] = BuildIdentityErrorMessage(roleResult, "Không thể gán vai trò cho tài khoản mới.");
                return RedirectToAction(nameof(BranchTeam), new { branchId });
            }

            TempData["SuccessMessage"] = $"Đã tạo tài khoản {user.Email} cho chi nhánh {branch.BranchName}.";
            return RedirectToAction(nameof(BranchTeam), new { branchId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Branch Owner")]
        public async Task<IActionResult> RemoveBranchMember(int branchId, string userId)
        {
            var actor = await GetCurrentActorAsync();
            var branch = await _context.Branches.FirstOrDefaultAsync(item => item.Id == branchId);
            if (branch == null)
            {
                return NotFound();
            }

            var manageableRoles = GetManageableBranchRoles(actor, branchId);
            if (manageableRoles.Count == 0)
            {
                return Forbid();
            }

            var targetUser = await _userManager.FindByIdAsync(userId);
            if (targetUser == null || targetUser.BranchId != branchId)
            {
                TempData["ErrorMessage"] = "Không tìm thấy nhân sự thuộc chi nhánh này.";
                return RedirectToAction(nameof(BranchTeam), new { branchId });
            }

            if (!actor.IsAdmin && string.Equals(targetUser.Id, actor.User?.Id, StringComparison.Ordinal))
            {
                TempData["ErrorMessage"] = "Bạn không thể tự xóa chính mình khỏi chi nhánh hiện tại.";
                return RedirectToAction(nameof(BranchTeam), new { branchId });
            }

            var targetRoles = await _userManager.GetRolesAsync(targetUser);
            var highestRole = RoleCatalog.GetHighestRole(targetRoles);
            if (!manageableRoles.Contains(highestRole))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xóa tài khoản này khỏi chi nhánh.";
                return RedirectToAction(nameof(BranchTeam), new { branchId });
            }

            targetUser.BranchId = null;
            var updateResult = await _userManager.UpdateAsync(targetUser);
            if (!updateResult.Succeeded)
            {
                TempData["ErrorMessage"] = BuildIdentityErrorMessage(updateResult, "Không thể xóa tài khoản khỏi chi nhánh.");
                return RedirectToAction(nameof(BranchTeam), new { branchId });
            }

            TempData["SuccessMessage"] = $"Đã gỡ {targetUser.FullName} khỏi chi nhánh {branch.BranchName}.";
            return RedirectToAction(nameof(BranchTeam), new { branchId });
        }

        // 2. Quản lý Đơn hàng
        public async Task<IActionResult> OrderManagement()
        {
            await CancelExpiredDepositOrdersAsync();
            var actor = await GetCurrentActorAsync();
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Branch)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.PendingConfirmationCount = orders.Count(order =>
                order.Status == Order.StatusPendingConfirmation && CanConfirmOrder(actor, order));
            ViewBag.ActorRoles = actor.Roles;
            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string newStatus, string cancelReason = null)
        {
            await CancelExpiredDepositOrdersAsync();

            var order = await _context.Orders
                .Include(item => item.OrderDetails)
                    .ThenInclude(detail => detail.Product)
                .FirstOrDefaultAsync(item => item.Id == orderId);
            if (order == null) return NotFound();

            var actor = await GetCurrentActorAsync();
            var allowedStatuses = new[]
            {
                Order.StatusPendingConfirmation, // Allow manual cash deposit confirmation
                Order.StatusConfirmed,
                Order.StatusProcessing,
                Order.StatusShipping,
                Order.StatusCompleted,
                Order.StatusCancelled
            };

            if (!allowedStatuses.Contains(newStatus))
            {
                TempData["ErrorMessage"] = "Trạng thái đơn hàng không hợp lệ.";
                return RedirectToAction(nameof(OrderManagement));
            }

            if (newStatus == Order.StatusPendingConfirmation)
            {
                if (order.Status != Order.StatusUnpaidDeposit && order.Status != Order.StatusAwaitingDepositPayment)
                {
                    TempData["ErrorMessage"] = "Chỉ đơn hàng chưa thanh toán cọc mới có thể xác nhận đã nhận cọc.";
                    return RedirectToAction(nameof(OrderManagement));
                }
            }

            if (newStatus == Order.StatusConfirmed && !CanConfirmOrder(actor, order))
            {
                TempData["ErrorMessage"] = order.TotalAmount > 50_000_000m
                    ? "Đơn trên 50 triệu chỉ quản lí hoặc admin được xác nhận."
                    : "Bạn không có quyền xác nhận đơn hàng này.";
                return RedirectToAction(nameof(OrderManagement));
            }

            if (newStatus == Order.StatusConfirmed && order.Status != Order.StatusPendingConfirmation)
            {
                TempData["ErrorMessage"] = "Chỉ đơn đã thanh toán cọc và đang chờ xác nhận mới được xác nhận.";
                return RedirectToAction(nameof(OrderManagement));
            }

            order.Status = newStatus;
            if (newStatus == Order.StatusPendingConfirmation)
            {
                order.DepositPaidAt = DateTime.UtcNow;
            }

            if (newStatus == Order.StatusConfirmed)
            {
                order.ConfirmedAt = DateTime.UtcNow;
            }

            if (newStatus == Order.StatusCancelled)
            {
                order.CancelReason = !string.IsNullOrWhiteSpace(cancelReason) 
                    ? cancelReason.Trim() 
                    : "Đơn hàng bị hủy bởi nhân sự xử lý.";
                RestoreProductsForCancelledOrder(order);
            }

            await _context.SaveChangesAsync();

            // Trigger notifications
            if (newStatus == Order.StatusPendingConfirmation)
            {
                await NotifyAuthorizedStaffForPendingOrderAsync(order);
            }
            else if (newStatus == Order.StatusConfirmed)
            {
                try
                {
                    var customer = await _userManager.FindByIdAsync(order.UserId);
                    var destination = customer != null && !string.IsNullOrWhiteSpace(customer.Email)
                        ? customer.Email
                        : order.CustomerPhone;

                    if (!string.IsNullOrWhiteSpace(destination))
                    {
                        await _notificationService.SendOrderConfirmedNotificationAsync(
                            destination,
                            order.CustomerName ?? customer?.FullName ?? "Quý khách",
                            order.OrderNumber);
                    }
                }
                catch (Exception)
                {
                    // Ignore exceptions for safety
                }
            }
            else if (newStatus == Order.StatusCancelled)
            {
                try
                {
                    var customer = await _userManager.FindByIdAsync(order.UserId);
                    var destination = customer != null && !string.IsNullOrWhiteSpace(customer.Email)
                        ? customer.Email
                        : order.CustomerPhone;

                    if (!string.IsNullOrWhiteSpace(destination))
                    {
                        await _notificationService.SendOrderRejectedNotificationAsync(
                            destination,
                            order.CustomerName ?? customer?.FullName ?? "Quý khách",
                            order.OrderNumber,
                            order.CancelReason);
                    }
                }
                catch (Exception)
                {
                    // Ignore exceptions for safety
                }
            }

            TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn hàng #{order.OrderNumber} thành {newStatus}.";
            return RedirectToAction(nameof(OrderManagement));
        }

        // 3. Quản lý Người dùng
        [Authorize(Roles = "Admin,Manager,Branch Owner")]
        public async Task<IActionResult> UserManagement()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    Roles = roles
                });
            }

            // Gửi danh sách Role hệ thống xuống View để lọc hoặc cấp
            ViewBag.AllRoles = (await _roleManager.Roles.Select(r => r.Name).ToListAsync())
                .OrderByDescending(RoleCatalog.GetPriority)
                .ToList();
            return View(userViewModels);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Branch Owner")]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            var targetUser = await _userManager.FindByIdAsync(userId);
            if (targetUser == null) return NotFound();

            var targetRoles = await _userManager.GetRolesAsync(targetUser);
            var actor = await GetCurrentActorAsync();

            // Logic khóa
            if (targetUser.IsActive
                && string.Equals(targetUser.Id, actor.User?.Id, StringComparison.Ordinal)
                && (actor.IsAdmin || actor.IsManager))
            {
                TempData["ErrorMessage"] = "Admin và quản lí không thể tự khóa chính tài khoản của mình.";
                return RedirectToAction(nameof(UserManagement));
            }

            if (targetRoles.Contains(RoleCatalog.Admin) && !actor.IsAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền khóa tài khoản Admin.";
                return RedirectToAction(nameof(UserManagement));
            }

            if ((targetRoles.Contains(RoleCatalog.BranchOwner) || targetRoles.Contains(RoleCatalog.Manager))
                && actor.IsManager
                && !actor.IsAdmin)
            {
                TempData["ErrorMessage"] = "Quản lý (Manager) không thể khóa tài khoản đồng cấp hoặc cấp trên.";
                return RedirectToAction(nameof(UserManagement));
            }

            // Toggle logic
            targetUser.IsActive = !targetUser.IsActive;
            var updateResult = await _userManager.UpdateAsync(targetUser);
            if (!updateResult.Succeeded)
            {
                TempData["ErrorMessage"] = BuildIdentityErrorMessage(updateResult, "Không thể cập nhật trạng thái tài khoản.");
                return RedirectToAction(nameof(UserManagement));
            }

            TempData["SuccessMessage"] = targetUser.IsActive
                ? $"Đã mở khóa tài khoản {targetUser.Email}"
                : $"Đã khóa tài khoản {targetUser.Email}. Tài khoản này sẽ tự bị đăng xuất ở lần tải trang tiếp theo.";
            return RedirectToAction(nameof(UserManagement));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Branch Owner")]
        public async Task<IActionResult> UpdateUserRole(string userId, string newRole)
        {
            var targetUser = await _userManager.FindByIdAsync(userId);
            if (targetUser == null) return NotFound();

            var actor = await GetCurrentActorAsync();
            if (!CanAssignRole(actor, newRole))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền gán vai trò này.";
                return RedirectToAction(nameof(UserManagement));
            }

            // Tiến hành cập nhật
            var existingRoles = await _userManager.GetRolesAsync(targetUser);
            
            // Xóa role cũ
            if (existingRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(targetUser, existingRoles);
                if (!removeResult.Succeeded)
                {
                    TempData["ErrorMessage"] = BuildIdentityErrorMessage(removeResult, "Không thể gỡ vai trò cũ của tài khoản.");
                    return RedirectToAction(nameof(UserManagement));
                }
            }
            
            // Thêm role mới
            var addResult = await _userManager.AddToRoleAsync(targetUser, newRole);
            if (!addResult.Succeeded)
            {
                TempData["ErrorMessage"] = BuildIdentityErrorMessage(addResult, "Không thể cập nhật vai trò tài khoản.");
                return RedirectToAction(nameof(UserManagement));
            }

            TempData["SuccessMessage"] = $"Đã cập nhật vai trò của {targetUser.Email} thành {RoleCatalog.GetVietnameseLabel(newRole)}.";
            return RedirectToAction(nameof(UserManagement));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Branch Owner")]
        public async Task<IActionResult> CreateUser(string FullName, string Email, string Password, string Role)
        {
            FullName = NormalizeOrEmpty(FullName);
            Email = NormalizeOrEmpty(Email);
            Role = NormalizeOrEmpty(Role);

            var actor = await GetCurrentActorAsync();
            if (!CanAssignRole(actor, Role))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền tạo tài khoản với vai trò này.";
                return RedirectToAction(nameof(UserManagement));
            }

            var newUser = new AppUser
            {
                UserName = Email,
                Email = Email,
                FullName = FullName,
                IsActive = true,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(newUser, Password);
            if (createResult.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(newUser, Role);
                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(newUser);
                    TempData["ErrorMessage"] = BuildIdentityErrorMessage(roleResult, "Không thể gán vai trò cho tài khoản mới.");
                    return RedirectToAction(nameof(UserManagement));
                }

                TempData["SuccessMessage"] = $"Thêm mới tài khoản {Email} thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = BuildIdentityErrorMessage(createResult, "Không thể tạo tài khoản mới.");
            }

            return RedirectToAction(nameof(UserManagement));
        }
        private async Task<BranchManagementViewModel> BuildBranchManagementViewModelAsync(BranchManagementViewModel source = null)
        {
            var branchUsers = await _userManager.Users
                .Where(user => user.BranchId != null)
                .OrderBy(user => user.FullName)
                .ToListAsync();

            var roleLookup = new Dictionary<string, IList<string>>(StringComparer.Ordinal);
            foreach (var branchUser in branchUsers)
            {
                roleLookup[branchUser.Id] = await _userManager.GetRolesAsync(branchUser);
            }

            var branches = await _context.Branches
                .OrderBy(branch => branch.BranchName)
                .Select(branch => new BranchManagementItemViewModel
                {
                    Id = branch.Id,
                    BranchName = branch.BranchName,
                    Address = branch.Address,
                    IsActive = branch.IsActive,
                    ProductCount = branch.Products.Count(),
                    OrderCount = branch.Orders.Count()
                })
                .ToListAsync();

            foreach (var branch in branches)
            {
                var members = branchUsers
                    .Where(user => user.BranchId == branch.Id)
                    .ToList();

                branch.OwnerSummary = BuildBranchRoleSummary(members, roleLookup, RoleCatalog.BranchOwner);
                branch.ManagerSummary = BuildBranchRoleSummary(members, roleLookup, RoleCatalog.Manager);
                branch.StaffCount = members.Count(user => roleLookup.TryGetValue(user.Id, out var roles) && roles.Contains(RoleCatalog.Staff));
                branch.CanManageMembers = true;
            }

            var ownerOptions = await BuildRoleOptionsAsync(RoleCatalog.BranchOwner, source?.OwnerUserId);
            var managerOptions = await BuildRoleOptionsAsync(RoleCatalog.Manager, source?.ManagerUserId);

            return new BranchManagementViewModel
            {
                BranchName = source?.BranchName ?? string.Empty,
                Address = source?.Address ?? string.Empty,
                OwnerUserId = source?.OwnerUserId ?? string.Empty,
                ManagerUserId = source?.ManagerUserId ?? string.Empty,
                OwnerOptions = ownerOptions,
                ManagerOptions = managerOptions,
                Branches = branches
            };
        }

        private async Task<BranchTeamViewModel> BuildBranchTeamViewModelAsync(
            Branch branch,
            ActorContext actor,
            IReadOnlyList<string> manageableRoles)
        {
            var users = await _userManager.Users
                .Where(user => user.BranchId == branch.Id)
                .OrderBy(user => user.FullName)
                .ToListAsync();

            var roleLookup = new Dictionary<string, IList<string>>(StringComparer.Ordinal);
            foreach (var user in users)
            {
                roleLookup[user.Id] = await _userManager.GetRolesAsync(user);
            }

            var members = users
                .Select(user =>
                {
                    var highestRole = RoleCatalog.GetHighestRole(roleLookup[user.Id]);
                    return new BranchTeamMemberViewModel
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email ?? string.Empty,
                        Role = highestRole,
                        IsActive = user.IsActive,
                        CanRemove = manageableRoles.Contains(highestRole)
                            && (actor.IsAdmin || !string.Equals(user.Id, actor.User?.Id, StringComparison.Ordinal))
                    };
                })
                .OrderByDescending(item => RoleCatalog.GetPriority(item.Role))
                .ThenBy(item => item.FullName)
                .ToList();

            return new BranchTeamViewModel
            {
                BranchId = branch.Id,
                BranchName = branch.BranchName,
                Address = branch.Address ?? string.Empty,
                IsActive = branch.IsActive,
                CanManageOwners = manageableRoles.Contains(RoleCatalog.BranchOwner),
                CanManageManagers = manageableRoles.Contains(RoleCatalog.Manager),
                CanManageStaff = manageableRoles.Contains(RoleCatalog.Staff),
                ExistingUserOptions = await BuildAssignableUserOptionsAsync(branch.Id, actor, manageableRoles),
                NewMemberRoleOptions = BuildAssignableRoleOptions(manageableRoles),
                NewMemberRole = manageableRoles.FirstOrDefault() ?? string.Empty,
                Members = members
            };
        }

        private async Task<IReadOnlyList<SelectListItem>> BuildRoleOptionsAsync(string roleName, string selectedUserId)
        {
            var users = await _userManager.GetUsersInRoleAsync(roleName);
            return new[]
                {
                    new SelectListItem
                    {
                        Value = string.Empty,
                        Text = "-- Chưa chọn --",
                        Selected = string.IsNullOrWhiteSpace(selectedUserId)
                    }
                }
                .Concat(users
                    .OrderBy(user => user.FullName)
                    .Select(user => new SelectListItem
                    {
                        Value = user.Id,
                        Text = $"{user.FullName} ({user.Email})",
                        Selected = string.Equals(user.Id, selectedUserId, StringComparison.Ordinal)
                    }))
                .ToList();
        }

        private async Task<IReadOnlyList<SelectListItem>> BuildAssignableUserOptionsAsync(
            int branchId,
            ActorContext actor,
            IReadOnlyList<string> manageableRoles)
        {
            var options = new Dictionary<string, SelectListItem>(StringComparer.Ordinal);

            foreach (var role in manageableRoles)
            {
                var users = await _userManager.GetUsersInRoleAsync(role);
                foreach (var user in users)
                {
                    if (!actor.IsAdmin && string.Equals(user.Id, actor.User?.Id, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (user.BranchId == branchId)
                    {
                        continue;
                    }

                    options[user.Id] = new SelectListItem
                    {
                        Value = user.Id,
                        Text = $"{user.FullName} - {RoleCatalog.GetVietnameseLabel(role)} ({user.Email})"
                    };
                }
            }

            return new[]
                {
                    new SelectListItem
                    {
                        Value = string.Empty,
                        Text = "-- Chọn tài khoản có sẵn --",
                        Selected = true
                    }
                }
                .Concat(options.Values.OrderBy(item => item.Text))
                .ToList();
        }

        private static IReadOnlyList<SelectListItem> BuildAssignableRoleOptions(IEnumerable<string> roles)
        {
            return roles
                .Select(role => new SelectListItem
                {
                    Value = role,
                    Text = RoleCatalog.GetVietnameseLabel(role)
                })
                .ToList();
        }

        private async Task<bool> IsEligibleBranchAssigneeAsync(string userId, string requiredRole)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return true;
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            return (await _userManager.GetRolesAsync(user)).Contains(requiredRole);
        }

        private async Task AssignBranchToUserIfSelectedAsync(string userId, int branchId, string requiredRole)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return;
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(requiredRole))
            {
                return;
            }

            user.BranchId = branchId;
            await _userManager.UpdateAsync(user);
        }

        private async Task<ActorContext> GetCurrentActorAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = user == null
                ? new List<string>()
                : await _userManager.GetRolesAsync(user);

            return new ActorContext
            {
                User = user,
                Roles = roles
            };
        }

        private static IReadOnlyList<string> GetManageableBranchRoles(ActorContext actor, int branchId)
        {
            if (actor.IsAdmin)
            {
                return new[] { RoleCatalog.BranchOwner, RoleCatalog.Manager, RoleCatalog.Staff };
            }

            if (actor.IsBranchOwner && actor.User?.BranchId == branchId)
            {
                return new[] { RoleCatalog.Manager, RoleCatalog.Staff };
            }

            if (actor.IsManager && actor.User?.BranchId == branchId)
            {
                return new[] { RoleCatalog.Staff };
            }

            return Array.Empty<string>();
        }

        private static bool CanAssignRole(ActorContext actor, string newRole)
        {
            if (actor.IsAdmin)
            {
                return true;
            }

            if (string.Equals(newRole, RoleCatalog.Admin, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (actor.IsBranchOwner)
            {
                return !string.Equals(newRole, RoleCatalog.BranchOwner, StringComparison.OrdinalIgnoreCase);
            }

            if (actor.IsManager)
            {
                return string.Equals(newRole, RoleCatalog.Staff, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(newRole, RoleCatalog.Accountant, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(newRole, RoleCatalog.Customer, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static bool CanConfirmOrder(ActorContext actor, Order order)
        {
            if (order.Status != Order.StatusPendingConfirmation)
            {
                return false;
            }

            if (actor.IsAdmin || actor.IsManager || actor.IsBranchOwner)
            {
                return true;
            }

            return actor.IsStaff && order.TotalAmount <= 50_000_000m;
        }

        private async Task CancelExpiredDepositOrdersAsync()
        {
            var now = DateTime.UtcNow;
            var expiredOrders = await _context.Orders
                .Include(order => order.OrderDetails)
                    .ThenInclude(detail => detail.Product)
                .Where(order =>
                    (order.Status == Order.StatusAwaitingDepositPayment || order.Status == Order.StatusUnpaidDeposit)
                    && order.DepositDueAt.HasValue
                    && order.DepositDueAt.Value <= now)
                .ToListAsync();

            if (expiredOrders.Count == 0)
            {
                return;
            }

            foreach (var order in expiredOrders)
            {
                order.Status = Order.StatusCancelled;
                order.CancelReason = "Đơn hàng tự hủy vì khách chưa thanh toán cọc trong 1 giờ 30 phút.";
                RestoreProductsForCancelledOrder(order);

                // Notify customer of auto-cancellation
                try
                {
                    var customer = await _userManager.FindByIdAsync(order.UserId);
                    var destination = customer != null && !string.IsNullOrWhiteSpace(customer.Email)
                        ? customer.Email
                        : order.CustomerPhone;

                    if (!string.IsNullOrWhiteSpace(destination))
                    {
                        await _notificationService.SendOrderCancelledDueToNoDepositNotificationAsync(
                            destination,
                            order.CustomerName ?? customer?.FullName ?? "Quý khách",
                            order.OrderNumber);
                    }
                }
                catch (Exception)
                {
                    // Ignore exceptions to avoid breaking background loops
                }
            }

            await _context.SaveChangesAsync();
        }

        private static void RestoreProductsForCancelledOrder(Order order)
        {
            foreach (var detail in order.OrderDetails ?? Enumerable.Empty<OrderDetail>())
            {
                if (detail.Product != null && detail.Product.Status == "Đã bán")
                {
                    detail.Product.Status = "Còn hàng";
                }
            }
        }

        private static string BuildBranchRoleSummary(
            IEnumerable<AppUser> members,
            IReadOnlyDictionary<string, IList<string>> roleLookup,
            string roleName)
        {
            var names = members
                .Where(user => roleLookup.TryGetValue(user.Id, out var roles) && roles.Contains(roleName))
                .Select(user => user.FullName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct()
                .ToList();

            return names.Count == 0 ? "--" : string.Join(", ", names);
        }

        private static string BuildIdentityErrorMessage(IdentityResult result, string fallbackMessage)
        {
            var errorMessages = result.Errors
                .Select(error => error.Description)
                .Where(description => !string.IsNullOrWhiteSpace(description))
                .ToList();

            return errorMessages.Count > 0
                ? string.Join(" ", errorMessages)
                : fallbackMessage;
        }

        private static string NormalizeOrEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private sealed class ActorContext
        {
            public AppUser User { get; set; }
            public IList<string> Roles { get; set; } = new List<string>();
            public bool IsAdmin => Roles.Contains(RoleCatalog.Admin);
            public bool IsBranchOwner => Roles.Contains(RoleCatalog.BranchOwner);
            public bool IsManager => Roles.Contains(RoleCatalog.Manager);
            public bool IsStaff => Roles.Contains(RoleCatalog.Staff);
        }

        private async Task NotifyAuthorizedStaffForPendingOrderAsync(Order order)
        {
            try
            {
                var admins = await _userManager.GetUsersInRoleAsync(RoleCatalog.Admin);
                var owners = await _userManager.GetUsersInRoleAsync(RoleCatalog.BranchOwner);
                var managers = await _userManager.GetUsersInRoleAsync(RoleCatalog.Manager);
                var staff = await _userManager.GetUsersInRoleAsync(RoleCatalog.Staff);

                var targetUsers = new List<AppUser>();
                targetUsers.AddRange(admins);
                targetUsers.AddRange(owners.Where(u => u.BranchId == order.BranchId));
                targetUsers.AddRange(managers.Where(u => u.BranchId == order.BranchId));
                if (order.TotalAmount <= 50_000_000m)
                {
                    targetUsers.AddRange(staff.Where(u => u.BranchId == order.BranchId));
                }

                var uniqueUsers = targetUsers.GroupBy(u => u.Id).Select(g => g.First()).ToList();

                foreach (var u in uniqueUsers)
                {
                    var destination = !string.IsNullOrWhiteSpace(u.Email) ? u.Email : u.PhoneNumber;
                    if (!string.IsNullOrWhiteSpace(destination))
                    {
                        await _notificationService.SendOrderPendingConfirmationNotificationAsync(destination, u.FullName, order);
                    }
                }
            }
            catch (Exception)
            {
                // Ignore errors
            }
        }
    }
}
