using GoldManagementSystem.Data;
using GoldManagementSystem.Models;
using GoldManagementSystem.Models.ViewModels;
using GoldManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GoldManagementSystem.Controllers
{
    [Authorize]
    public class ManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IManagementPermissionService _permissions;
        private readonly SystemNotificationService _notifications;

        public ManagementController(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IManagementPermissionService permissions,
            SystemNotificationService notifications)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _permissions = permissions;
            _notifications = notifications;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string tab = "overview",
            string subtab = null,
            string period = "month",
            int? branchId = null,
            DateTime? date = null,
            string permissionUserId = null,
            int? permissionBranchId = null,
            bool partial = false)
        {
            tab = NormalizeTab(tab);
            var isAdmin = User.IsInRole(RoleCatalog.Admin);
            if (IsSystemTab(tab) && !isAdmin) return Forbid();

            var branches = await _permissions.GetAccessibleBranchesAsync(User);
            var selectedBranch = branches.FirstOrDefault(branch => branch.Id == branchId)
                ?? branches.FirstOrDefault();

            if (!IsSystemTab(tab) && selectedBranch == null) return Forbid();

            var granted = await _permissions.GetGrantedKeysAsync(User, selectedBranch?.Id);
            if (!isAdmin && !CanOpenTab(tab, granted)) return Forbid();

            var localNow = VietnamNow();
            var selectedDate = (date ?? localNow).Date;
            var model = new ManagementPortalViewModel
            {
                Tab = tab,
                Subtab = NormalizeSubtab(tab, subtab),
                Period = period is "day" or "year" ? period : "month",
                SelectedDate = selectedDate,
                SelectedBranch = selectedBranch,
                Branches = branches,
                GrantedFeatures = granted,
                IsAdmin = isAdmin,
                PermissionUpdated = TempData["PermissionsUpdated"] as bool? == true
            };
            if (tab == "people")
            {
                if (model.Subtab == "employees" && !isAdmin && !granted.Contains(ManagementFeatureCatalog.PeopleView))
                    model.Subtab = granted.Contains(ManagementFeatureCatalog.PeopleShifts) ? "shifts" : "payroll";
                if (model.Subtab == "shifts" && !isAdmin && !granted.Contains(ManagementFeatureCatalog.PeopleShifts))
                    model.Subtab = granted.Contains(ManagementFeatureCatalog.PeopleView) ? "employees" : "payroll";
                if (model.Subtab == "payroll" && !isAdmin && !granted.Contains(ManagementFeatureCatalog.PeoplePayroll))
                    model.Subtab = granted.Contains(ManagementFeatureCatalog.PeopleView) ? "employees" : "shifts";
            }

            if (selectedBranch != null)
                await PopulateBranchSummaryAsync(model, selectedBranch.Id, localNow);

            switch (tab)
            {
                case "people":
                    await PopulatePeopleAsync(model);
                    break;
                case "warehouse":
                    await PopulateWarehouseAsync(model, selectedBranch.Id);
                    break;
                case "products":
                    model.Products = await _context.Products.AsNoTracking()
                        .Include(product => product.Branch)
                        .Where(product => product.BranchId == selectedBranch.Id && product.Status != "Đã xóa")
                        .OrderByDescending(product => product.IsPriority)
                        .ThenBy(product => product.PriorityOrder)
                        .ThenByDescending(product => product.CreatedAt)
                        .ToListAsync();
                    break;
                case "revenue":
                    model.RevenuePoints = await BuildRevenuePointsAsync(selectedBranch.Id, model.Period, selectedDate);
                    break;
                case "users":
                case "permissions":
                    model.Users = await BuildUsersAsync();
                    model.PermissionUserId = permissionUserId ?? model.Users.FirstOrDefault()?.Id;
                    model.PermissionBranchId = permissionBranchId ?? branches.FirstOrDefault()?.Id;
                    if (!string.IsNullOrWhiteSpace(model.PermissionUserId) && model.PermissionBranchId.HasValue)
                    {
                        model.PermissionSelection = (await _context.UserFeaturePermissions.AsNoTracking()
                            .Where(permission => permission.UserId == model.PermissionUserId
                                && permission.BranchId == model.PermissionBranchId
                                && permission.IsGranted)
                            .Select(permission => permission.FeatureKey)
                            .ToListAsync()).ToHashSet();
                    }
                    break;
                case "audit":
                    var todayUtc = ToUtc(selectedDate);
                    var tomorrowUtc = ToUtc(selectedDate.AddDays(1));
                    model.SalesAuditLogs = await _context.ManagementAuditLogs.AsNoTracking()
                        .Where(log => log.Area == "Sales" && log.CreatedAt >= todayUtc && log.CreatedAt < tomorrowUtc)
                        .OrderByDescending(log => log.CreatedAt).Take(250).ToListAsync();
                    model.ManagementAuditLogs = await _context.ManagementAuditLogs.AsNoTracking()
                        .Where(log => log.Area == "Management" && log.CreatedAt >= todayUtc && log.CreatedAt < tomorrowUtc)
                        .OrderByDescending(log => log.CreatedAt).Take(250).ToListAsync();
                    break;
                case "branches":
                    await PopulateBranchCreationOptionsAsync(model);
                    break;
            }

            return partial ? PartialView("_Dashboard", model) : View(model);
        }

        [HttpGet]
        public async Task<IActionResult> MyShifts()
        {
            var userId = _userManager.GetUserId(User);
            var fromDate = VietnamNow().Date.AddDays(-7);
            var assignments = await _context.ShiftAssignments.AsNoTracking()
                .Include(item => item.WorkShift).ThenInclude(shift => shift.Branch)
                .Where(item => item.UserId == userId && item.WorkShift.ShiftDate >= fromDate)
                .OrderBy(item => item.WorkShift.StartsAt)
                .Take(100)
                .ToListAsync();
            return View(assignments);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignShift(int branchId, DateTime shiftDate, string shiftType, string userId, string reason)
        {
            if (!await _permissions.CanAsync(User, ManagementFeatureCatalog.PeopleShifts, branchId)) return Forbid();
            var branch = await _context.Branches.FindAsync(branchId);
            var employee = await _userManager.FindByIdAsync(userId);
            if (branch == null || employee == null || !employee.IsActive) return NotFound();
            var employeeRoles = await _userManager.GetRolesAsync(employee);
            if (employeeRoles.Contains(RoleCatalog.Admin) || employeeRoles.Contains(RoleCatalog.Customer))
                return ErrorRedirect("Chỉ có thể xếp ca cho tài khoản nhân sự (không gồm Admin/khách hàng).", "people", "shifts", branchId, shiftDate);

            shiftType = string.Equals(shiftType, "Afternoon", StringComparison.OrdinalIgnoreCase) ? "Afternoon" : "Morning";
            var localDate = shiftDate.Date;
            var startsLocal = localDate.AddHours(shiftType == "Morning" ? 8 : 16);
            var endsLocal = shiftType == "Morning" ? localDate.AddHours(16) : localDate.AddDays(1);
            var startsUtc = ToUtc(startsLocal);
            var endsUtc = ToUtc(endsLocal);

            var assignedCount = await _context.ShiftAssignments
                .Where(assignment => assignment.UserId == userId && assignment.WorkShift.ShiftDate == localDate)
                .CountAsync();
            var existing = await _context.ShiftAssignments.AnyAsync(assignment =>
                assignment.UserId == userId
                && assignment.WorkShift.BranchId == branchId
                && assignment.WorkShift.ShiftDate == localDate
                && assignment.WorkShift.ShiftType == shiftType);
            if (existing) return ErrorRedirect("Nhân viên đã có trong ca này.", "people", "shifts", branchId, shiftDate);
            if (assignedCount >= 2) return ErrorRedirect("Mỗi nhân viên chỉ được xếp tối đa 2 ca trong một ngày, kể cả khác chi nhánh.", "people", "shifts", branchId, shiftDate);

            var isSupplemental = startsUtc <= DateTime.UtcNow.AddMinutes(30);
            if (isSupplemental && string.IsNullOrWhiteSpace(reason))
                return ErrorRedirect("Ca còn dưới 30 phút hoặc đã bắt đầu. Cần nhập lý do để lưu vào bảng chỉnh sửa bổ sung.", "people", "shifts", branchId, shiftDate);

            var shift = await _context.WorkShifts.FirstOrDefaultAsync(item =>
                item.BranchId == branchId && item.ShiftDate == localDate && item.ShiftType == shiftType);
            if (shift == null)
            {
                shift = new WorkShift { BranchId = branchId, ShiftDate = localDate, ShiftType = shiftType, StartsAt = startsUtc, EndsAt = endsUtc };
                _context.WorkShifts.Add(shift);
                await _context.SaveChangesAsync();
            }

            shift.Assignments.Add(new ShiftAssignment { UserId = userId, AttendanceStatus = "Scheduled" });
            shift.UpdatedAt = DateTime.UtcNow;
            if (isSupplemental)
                _context.ShiftChangeLogs.Add(new ShiftChangeLog
                {
                    WorkShiftId = shift.Id,
                    ChangedByUserId = _userManager.GetUserId(User),
                    ChangeType = "Add",
                    Details = $"Bổ sung {employee.FullName}: {reason.Trim()}"
                });
            await _context.SaveChangesAsync();

            await _notifications.SendAsync(userId, "Ca làm mới", $"Bạn được xếp {ShiftLabel(shiftType)} tại {branch.BranchName}, ngày {localDate:dd/MM/yyyy}.", "/Management/MyShifts", "Shift");
            TempData["SuccessMessage"] = "Đã xếp ca và gửi thông báo đến hộp thư của nhân viên.";
            return RedirectToDashboard("people", "shifts", branchId, shiftDate);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveShiftAssignment(int assignmentId, string reason)
        {
            var assignment = await _context.ShiftAssignments
                .Include(item => item.User).Include(item => item.WorkShift).ThenInclude(shift => shift.Branch)
                .FirstOrDefaultAsync(item => item.Id == assignmentId);
            if (assignment == null) return NotFound();
            var branchId = assignment.WorkShift.BranchId;
            if (!await _permissions.CanAsync(User, ManagementFeatureCatalog.PeopleShifts, branchId)) return Forbid();

            var isSupplemental = assignment.WorkShift.StartsAt <= DateTime.UtcNow.AddMinutes(30);
            if (isSupplemental && string.IsNullOrWhiteSpace(reason))
                return ErrorRedirect("Cần nhập lý do cho thay đổi sát giờ để lưu vào bảng bổ sung.", "people", "shifts", branchId, assignment.WorkShift.ShiftDate);

            if (isSupplemental)
                _context.ShiftChangeLogs.Add(new ShiftChangeLog
                {
                    WorkShiftId = assignment.WorkShiftId,
                    ChangedByUserId = _userManager.GetUserId(User),
                    ChangeType = "Remove",
                    Details = $"Gỡ {assignment.User.FullName}: {reason.Trim()}"
                });

            var userId = assignment.UserId;
            var employeeName = assignment.User.FullName;
            var shift = assignment.WorkShift;
            _context.ShiftAssignments.Remove(assignment);
            shift.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _notifications.SendAsync(userId, "Ca làm đã thay đổi", $"Bạn đã được gỡ khỏi {ShiftLabel(shift.ShiftType)} tại {shift.Branch.BranchName}, ngày {shift.ShiftDate:dd/MM/yyyy}.", "/Management/MyShifts", "ShiftChange");
            TempData["SuccessMessage"] = $"Đã cập nhật ca của {employeeName} và gửi thông báo.";
            return RedirectToDashboard("people", "shifts", branchId, shift.ShiftDate);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEmployeeNote(int branchId, string userId, string managerNote)
        {
            if (!await _permissions.CanAsync(User, ManagementFeatureCatalog.PeoplePayroll, branchId)) return Forbid();
            var note = await _context.EmployeeManagementNotes.FirstOrDefaultAsync(item => item.BranchId == branchId && item.UserId == userId);
            if (note == null)
            {
                note = new EmployeeManagementNote { BranchId = branchId, UserId = userId };
                _context.EmployeeManagementNotes.Add(note);
            }
            note.ManagerNote = (managerNote ?? string.Empty).Trim();
            note.UpdatedAt = DateTime.UtcNow;
            note.UpdatedByUserId = _userManager.GetUserId(User);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã lưu ghi chú quản lí.";
            return RedirectToDashboard("people", "payroll", branchId, VietnamNow());
        }

        [HttpGet]
        public async Task<IActionResult> AttendanceStatus()
        {
            if (!CanUseAttendance()) return Json(new { available = false });
            var userId = _userManager.GetUserId(User);
            var now = DateTime.UtcNow;
            var assignments = await _context.ShiftAssignments.AsNoTracking()
                .Include(item => item.WorkShift).ThenInclude(shift => shift.Branch)
                .Where(item => item.UserId == userId
                    && item.WorkShift.StartsAt <= now.AddMinutes(5)
                    && item.WorkShift.EndsAt >= now.AddHours(-12))
                .OrderBy(item => item.WorkShift.StartsAt).ToListAsync();
            var assignment = assignments.FirstOrDefault(item => item.CheckedInAt.HasValue && !item.CheckedOutAt.HasValue)
                ?? assignments.FirstOrDefault(item => !item.CheckedInAt.HasValue && now <= item.WorkShift.EndsAt);
            if (assignment == null) return Json(new { available = false });
            var action = assignment.CheckedInAt.HasValue ? "out" : "in";
            var canSubmit = action == "in" || now >= assignment.CheckedInAt.Value.AddMinutes(5);
            return Json(new
            {
                available = true,
                canSubmit,
                assignmentId = assignment.Id,
                action,
                label = action == "in" ? "Điểm danh vào ca" : "Điểm danh ra ca",
                branch = assignment.WorkShift.Branch.BranchName,
                shift = ShiftLabel(assignment.WorkShift.ShiftType)
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Attendance(int assignmentId)
        {
            if (!CanUseAttendance()) return Forbid();
            var userId = _userManager.GetUserId(User);
            var assignment = await _context.ShiftAssignments
                .Include(item => item.WorkShift).ThenInclude(shift => shift.Branch)
                .FirstOrDefaultAsync(item => item.Id == assignmentId && item.UserId == userId);
            if (assignment == null) return NotFound();
            var now = DateTime.UtcNow;

            if (!assignment.CheckedInAt.HasValue)
            {
                if (now < assignment.WorkShift.StartsAt.AddMinutes(-5)) return BadRequest("Chỉ có thể điểm danh trước giờ vào ca 5 phút.");
                if (now > assignment.WorkShift.EndsAt) return BadRequest("Ca làm đã kết thúc, không thể điểm danh vào ca.");
                assignment.CheckedInAt = now;
                if (now > assignment.WorkShift.StartsAt.AddMinutes(5))
                {
                    assignment.AttendanceStatus = "Late-Red";
                    AppendSystemNote(assignment, $"Điểm danh vào trễ {Math.Floor((now - assignment.WorkShift.StartsAt).TotalMinutes)} phút (đỏ).");
                }
                else assignment.AttendanceStatus = "OnTime";
            }
            else
            {
                if (assignment.CheckedOutAt.HasValue) return BadRequest("Ca làm này đã được điểm danh ra.");
                if (now < assignment.CheckedInAt.Value.AddMinutes(5)) return BadRequest("Điểm danh vào và ra phải cách nhau ít nhất 5 phút.");
                assignment.CheckedOutAt = now;
                if (now < assignment.WorkShift.EndsAt.AddMinutes(-5))
                {
                    assignment.AttendanceStatus = "EarlyOut-Red";
                    AppendSystemNote(assignment, $"Ra ca sớm {Math.Ceiling((assignment.WorkShift.EndsAt - now).TotalMinutes)} phút (đỏ).");
                }
                else if (now > assignment.WorkShift.EndsAt.AddMinutes(15))
                {
                    assignment.AttendanceStatus = assignment.AttendanceStatus.Contains("Red", StringComparison.OrdinalIgnoreCase)
                        ? $"{assignment.AttendanceStatus}|LateOut-Yellow"
                        : "LateOut-Yellow";
                    AppendSystemNote(assignment, $"Ra ca muộn {Math.Floor((now - assignment.WorkShift.EndsAt).TotalMinutes)} phút (vàng).");
                }
                else if (assignment.AttendanceStatus == "OnTime") assignment.AttendanceStatus = "Completed";
            }

            var employeeNote = await _context.EmployeeManagementNotes.FirstOrDefaultAsync(note =>
                note.UserId == userId && note.BranchId == assignment.WorkShift.BranchId);
            if (employeeNote == null)
            {
                employeeNote = new EmployeeManagementNote
                {
                    UserId = userId,
                    BranchId = assignment.WorkShift.BranchId
                };
                _context.EmployeeManagementNotes.Add(employeeNote);
            }
            employeeNote.SystemNote = assignment.SystemNote;
            employeeNote.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Json(new { success = true, status = assignment.AttendanceStatus, note = assignment.SystemNote });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = RoleCatalog.Admin)]
        public async Task<IActionResult> CreateBranch(string branchName, string address, int? warehouseId, string managerUserId)
        {
            branchName = (branchName ?? string.Empty).Trim();
            address = (address ?? string.Empty).Trim();
            managerUserId = (managerUserId ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(branchName) || branchName.Length > 150)
                return ErrorRedirect("Tên chi nhánh là bắt buộc và không được vượt quá 150 ký tự.", "branches");
            if (string.IsNullOrWhiteSpace(address) || address.Length > 300)
                return ErrorRedirect("Địa chỉ chi nhánh là bắt buộc và không được vượt quá 300 ký tự.", "branches");
            if (await _context.Branches.AnyAsync(branch => branch.BranchName.ToLower() == branchName.ToLower()))
                return ErrorRedirect("Tên chi nhánh này đã tồn tại.", "branches");

            var warehouse = warehouseId.HasValue
                ? await _context.Warehouses.FirstOrDefaultAsync(item => item.Id == warehouseId.Value && item.IsActive)
                : null;
            if (warehouse == null)
                return ErrorRedirect("Vui lòng chọn một kho đang hoạt động.", "branches");

            var manager = string.IsNullOrWhiteSpace(managerUserId)
                ? null
                : await _userManager.FindByIdAsync(managerUserId);
            if (manager == null
                || !manager.IsActive
                || !(await _userManager.GetRolesAsync(manager)).Contains(RoleCatalog.Manager))
                return ErrorRedirect("Chủ quản lí chi nhánh không hợp lệ hoặc đã bị khóa.", "branches");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var branch = new Branch
                {
                    BranchName = branchName,
                    Address = address,
                    PhoneNumber = string.Empty,
                    IsActive = true
                };
                _context.Branches.Add(branch);
                await _context.SaveChangesAsync();

                _context.BranchWarehouseAccesses.Add(new BranchWarehouseAccess
                {
                    BranchId = branch.Id,
                    WarehouseId = warehouse.Id,
                    IsPrimary = true
                });
                manager.BranchId = branch.Id;
                var managerResult = await _userManager.UpdateAsync(manager);
                if (!managerResult.Succeeded)
                    throw new InvalidOperationException(string.Join(" ", managerResult.Errors.Select(error => error.Description)));

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                TempData["SuccessMessage"] = $"Đã tạo chi nhánh {branchName}, gán kho {warehouse.Name} và chủ quản lí {manager.FullName}.";
            }
            catch
            {
                await transaction.RollbackAsync();
                return ErrorRedirect("Không thể tạo chi nhánh. Vui lòng kiểm tra dữ liệu và thử lại.", "branches");
            }

            return RedirectToAction(nameof(Index), new { tab = "branches" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = RoleCatalog.Admin)]
        public async Task<IActionResult> UpdatePermissions(string userId, int branchId, string[] featureKeys)
        {
            var target = await _userManager.FindByIdAsync(userId);
            if (target == null || !await _context.Branches.AnyAsync(branch => branch.Id == branchId)) return NotFound();
            if ((await _userManager.GetRolesAsync(target)).Contains(RoleCatalog.Admin))
                return ErrorRedirect("Admin luôn có toàn quyền và không cần cấu hình quyền.", "permissions", null, branchId, VietnamNow(), userId);

            var allowedKeys = ManagementFeatureCatalog.BranchFeatures.Select(feature => feature.Key).ToHashSet();
            var selected = (featureKeys ?? Array.Empty<string>()).Where(allowedKeys.Contains).Distinct().ToHashSet();
            var existing = await _context.UserFeaturePermissions
                .Where(permission => permission.UserId == userId && permission.BranchId == branchId)
                .ToListAsync();
            _context.UserFeaturePermissions.RemoveRange(existing);
            foreach (var key in selected)
                _context.UserFeaturePermissions.Add(new UserFeaturePermission
                {
                    UserId = userId,
                    BranchId = branchId,
                    FeatureKey = key,
                    IsGranted = true,
                    GrantedByUserId = _userManager.GetUserId(User),
                    UpdatedAt = DateTime.UtcNow
                });
            await _context.SaveChangesAsync();
            TempData["PermissionsUpdated"] = true;
            TempData["SuccessMessage"] = "Đã cập nhật quyền. Người dùng cần tải lại trang để áp dụng menu mới.";
            return RedirectToAction(nameof(Index), new { tab = "permissions", permissionUserId = userId, permissionBranchId = branchId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = RoleCatalog.Admin)]
        public async Task<IActionResult> CreateAccount(string fullName, string email, string phoneNumber, string password, string role, int? branchId)
        {
            if (!await _roleManager.RoleExistsAsync(role)) return ErrorRedirect("Vai trò không hợp lệ.", "users");
            var user = new AppUser { FullName = (fullName ?? string.Empty).Trim(), UserName = email?.Trim(), Email = email?.Trim(), PhoneNumber = phoneNumber?.Trim(), BranchId = branchId, IsActive = true, EmailConfirmed = true };
            var result = await _userManager.CreateAsync(user, password ?? string.Empty);
            if (!result.Succeeded) return ErrorRedirect(string.Join(" ", result.Errors.Select(error => error.Description)), "users");
            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return ErrorRedirect(string.Join(" ", roleResult.Errors.Select(error => error.Description)), "users");
            }
            TempData["SuccessMessage"] = "Đã tạo tài khoản người dùng.";
            return RedirectToAction(nameof(Index), new { tab = "users" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = RoleCatalog.Admin)]
        public async Task<IActionResult> UpdateAccount(string userId, string fullName, string email, string phoneNumber, string role, int? branchId)
        {
            var target = await _userManager.FindByIdAsync(userId);
            if (target == null) return NotFound();
            var actorId = _userManager.GetUserId(User);
            var targetRoles = await _userManager.GetRolesAsync(target);
            if (userId == actorId && targetRoles.Contains(RoleCatalog.Admin) && role != RoleCatalog.Admin)
                return ErrorRedirect("Không thể tự gỡ quyền Admin của tài khoản đang đăng nhập.", "users");
            if (!await _roleManager.RoleExistsAsync(role)) return ErrorRedirect("Vai trò không hợp lệ.", "users");

            target.FullName = (fullName ?? string.Empty).Trim();
            target.Email = email?.Trim();
            target.UserName = email?.Trim();
            target.NormalizedEmail = _userManager.NormalizeEmail(target.Email);
            target.NormalizedUserName = _userManager.NormalizeName(target.UserName);
            target.PhoneNumber = phoneNumber?.Trim();
            target.BranchId = branchId;
            var updateResult = await _userManager.UpdateAsync(target);
            if (!updateResult.Succeeded) return ErrorRedirect(string.Join(" ", updateResult.Errors.Select(error => error.Description)), "users");
            await _userManager.RemoveFromRolesAsync(target, targetRoles);
            await _userManager.AddToRoleAsync(target, role);
            TempData["SuccessMessage"] = "Đã cập nhật tài khoản.";
            return RedirectToAction(nameof(Index), new { tab = "users" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = RoleCatalog.Admin)]
        public async Task<IActionResult> DeleteAccount(string userId)
        {
            var target = await _userManager.FindByIdAsync(userId);
            if (target == null) return NotFound();
            if (userId == _userManager.GetUserId(User)) return ErrorRedirect("Không thể xóa tài khoản đang đăng nhập.", "users");
            var result = await _userManager.DeleteAsync(target);
            if (!result.Succeeded)
            {
                target.IsActive = false;
                await _userManager.UpdateAsync(target);
                TempData["ErrorMessage"] = "Tài khoản có dữ liệu liên quan nên đã được khóa thay vì xóa để bảo toàn lịch sử.";
            }
            else TempData["SuccessMessage"] = "Đã xóa tài khoản.";
            return RedirectToAction(nameof(Index), new { tab = "users" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = RoleCatalog.Admin)]
        public async Task<IActionResult> ToggleAccount(string userId)
        {
            var target = await _userManager.FindByIdAsync(userId);
            if (target == null) return NotFound();
            if (userId == _userManager.GetUserId(User))
                return ErrorRedirect("Không thể tự khóa tài khoản đang đăng nhập.", "users");
            target.IsActive = !target.IsActive;
            var result = await _userManager.UpdateAsync(target);
            if (!result.Succeeded) return ErrorRedirect(string.Join(" ", result.Errors.Select(error => error.Description)), "users");
            TempData["SuccessMessage"] = target.IsActive ? "Đã kích hoạt tài khoản." : "Đã khóa tài khoản.";
            return RedirectToAction(nameof(Index), new { tab = "users" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleProductPriority(int productId, int branchId)
        {
            if (!await _permissions.CanAsync(User, ManagementFeatureCatalog.ProductsEdit, branchId)) return Forbid();
            var product = await _context.Products.FirstOrDefaultAsync(item => item.Id == productId && item.BranchId == branchId);
            if (product == null) return NotFound();
            product.IsPriority = !product.IsPriority;
            if (product.IsPriority && product.PriorityOrder == 0)
                product.PriorityOrder = (await _context.Products.Where(item => item.BranchId == branchId).MaxAsync(item => (int?)item.PriorityOrder) ?? 0) + 1;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = product.IsPriority ? "Đã ưu tiên sản phẩm." : "Đã bỏ ưu tiên sản phẩm.";
            return RedirectToAction(nameof(Index), new { tab = "products", branchId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int productId, int branchId)
        {
            if (!await _permissions.CanAsync(User, ManagementFeatureCatalog.ProductsEdit, branchId)) return Forbid();
            var product = await _context.Products.FirstOrDefaultAsync(item => item.Id == productId && item.BranchId == branchId);
            if (product == null) return NotFound();
            var hasOrders = await _context.OrderDetails.AnyAsync(detail => detail.ProductId == productId);
            if (hasOrders)
            {
                product.Status = "Đã xóa";
                product.IsPriority = false;
                TempData["SuccessMessage"] = "Sản phẩm có giao dịch nên đã được ẩn để bảo toàn lịch sử.";
            }
            else
            {
                _context.Products.Remove(product);
                TempData["SuccessMessage"] = "Đã xóa sản phẩm.";
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { tab = "products", branchId });
        }

        private async Task PopulateBranchSummaryAsync(ManagementPortalViewModel model, int branchId, DateTime localNow)
        {
            var todayUtc = ToUtc(localNow.Date);
            var tomorrowUtc = ToUtc(localNow.Date.AddDays(1));
            var monthUtc = ToUtc(new DateTime(localNow.Year, localNow.Month, 1));
            var nextMonthUtc = ToUtc(new DateTime(localNow.Year, localNow.Month, 1).AddMonths(1));
            var yearUtc = ToUtc(new DateTime(localNow.Year, 1, 1));
            var nextYearUtc = ToUtc(new DateTime(localNow.Year + 1, 1, 1));
            var completed = _context.Orders.Where(order => order.BranchId == branchId && order.Status == Order.StatusCompleted);
            model.TodayRevenue = await completed.Where(order => order.OrderDate >= todayUtc && order.OrderDate < tomorrowUtc).SumAsync(order => (decimal?)order.TotalAmount) ?? 0;
            model.MonthRevenue = await completed.Where(order => order.OrderDate >= monthUtc && order.OrderDate < nextMonthUtc).SumAsync(order => (decimal?)order.TotalAmount) ?? 0;
            model.YearRevenue = await completed.Where(order => order.OrderDate >= yearUtc && order.OrderDate < nextYearUtc).SumAsync(order => (decimal?)order.TotalAmount) ?? 0;
            model.EmployeeCount = await _context.Users.CountAsync(user => user.BranchId == branchId && user.IsActive);
        }

        private async Task PopulatePeopleAsync(ManagementPortalViewModel model)
        {
            var branchId = model.SelectedBranch.Id;
            var users = await _context.Users.AsNoTracking().Where(user => user.BranchId == branchId).OrderBy(user => user.FullName).ToListAsync();
            var notes = await _context.EmployeeManagementNotes.AsNoTracking().Where(note => note.BranchId == branchId).ToDictionaryAsync(note => note.UserId);
            var items = new List<ManagementEmployeeItem>();
            var rolesByUser = new Dictionary<string, string>();
            foreach (var user in users)
            {
                var role = RoleCatalog.GetHighestRole(await _userManager.GetRolesAsync(user));
                rolesByUser[user.Id] = role;
                notes.TryGetValue(user.Id, out var note);
                items.Add(new ManagementEmployeeItem { UserId = user.Id, FullName = user.FullName, Email = user.Email, Role = role, IsActive = user.IsActive, SystemNote = note?.SystemNote, ManagerNote = note?.ManagerNote });
            }
            model.Employees = items;

            // Có thể xếp một nhân viên sang ca của chi nhánh khác; giới hạn 2 ca/ngày
            // vẫn được kiểm tra trên toàn hệ thống khi lưu.
            var availableUsers = await _context.Users.AsNoTracking()
                .Where(user => user.IsActive).OrderBy(user => user.FullName).ToListAsync();
            var availableItems = new List<ManagementEmployeeItem>();
            foreach (var user in availableUsers)
            {
                var role = RoleCatalog.GetHighestRole(await _userManager.GetRolesAsync(user));
                if (role is RoleCatalog.Admin or RoleCatalog.Customer) continue;
                availableItems.Add(new ManagementEmployeeItem
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = role,
                    IsActive = true
                });
            }
            model.AvailableEmployees = availableItems;

            var shifts = await _context.WorkShifts.AsNoTracking()
                .Include(shift => shift.Assignments).ThenInclude(assignment => assignment.User)
                .Where(shift => shift.BranchId == branchId && shift.ShiftDate == model.SelectedDate.Date)
                .OrderBy(shift => shift.StartsAt).ToListAsync();
            var shiftIds = shifts.Select(shift => shift.Id).ToList();
            var logs = await _context.ShiftChangeLogs.AsNoTracking().Where(log => shiftIds.Contains(log.WorkShiftId)).OrderByDescending(log => log.CreatedAt).ToListAsync();
            model.Shifts = shifts.Select(shift => new ManagementShiftItem
            {
                ShiftId = shift.Id,
                ShiftType = shift.ShiftType,
                ShiftLabel = ShiftLabel(shift.ShiftType),
                StartsAt = FromUtc(shift.StartsAt),
                EndsAt = FromUtc(shift.EndsAt),
                IsLockedWindow = shift.StartsAt <= DateTime.UtcNow.AddMinutes(30),
                Assignments = shift.Assignments.OrderBy(assignment => assignment.User.FullName).ToList(),
                SupplementalChanges = logs.Where(log => log.WorkShiftId == shift.Id).ToList()
            }).ToList();

            model.Payroll = await BuildPayrollAsync(users, rolesByUser, branchId, model.SelectedDate);
        }

        private async Task PopulateWarehouseAsync(ManagementPortalViewModel model, int branchId)
        {
            var warehouseIds = await _context.BranchWarehouseAccesses.AsNoTracking()
                .Where(access => access.BranchId == branchId)
                .Select(access => access.WarehouseId)
                .Concat(_context.Warehouses.AsNoTracking()
                    .Where(warehouse => warehouse.BranchId == branchId)
                    .Select(warehouse => warehouse.Id))
                .Distinct()
                .ToListAsync();

            model.Suppliers = await _context.Suppliers.AsNoTracking()
                .OrderByDescending(supplier => supplier.IsActive)
                .ThenBy(supplier => supplier.Name)
                .Take(100)
                .ToListAsync();
            model.PurchaseOrders = await _context.SupplierPurchaseOrders.AsNoTracking()
                .Include(order => order.Supplier)
                .Where(order => order.BranchId == branchId)
                .OrderByDescending(order => order.CreatedAt)
                .Take(50)
                .ToListAsync();
            model.GoodsReceipts = await _context.SupplierGoodsReceipts.AsNoTracking()
                .Include(receipt => receipt.SupplierPurchaseOrder).ThenInclude(order => order.Supplier)
                .Include(receipt => receipt.Warehouse)
                .Where(receipt => receipt.SupplierPurchaseOrder.BranchId == branchId)
                .OrderByDescending(receipt => receipt.ReceivedAt)
                .Take(50)
                .ToListAsync();
            model.Warehouses = await _context.Warehouses.AsNoTracking()
                .Where(warehouse => warehouseIds.Contains(warehouse.Id) && warehouse.IsActive)
                .OrderBy(warehouse => warehouse.Name)
                .ToListAsync();
            model.InventoryItems = await _context.InventoryItems.AsNoTracking()
                .Include(item => item.Warehouse)
                .Where(item => warehouseIds.Contains(item.WarehouseId))
                .OrderBy(item => item.Status)
                .ThenBy(item => item.ProductName)
                .Take(100)
                .ToListAsync();
        }

        private async Task PopulateBranchCreationOptionsAsync(ManagementPortalViewModel model)
        {
            var managers = await _userManager.GetUsersInRoleAsync(RoleCatalog.Manager);
            model.BranchManagerOptions = managers
                .Where(manager => manager.IsActive)
                .OrderBy(manager => manager.FullName)
                .Select(manager => new ManagementSelectOption
                {
                    Value = manager.Id,
                    Label = $"{manager.FullName} ({manager.Email})"
                })
                .ToList();
            model.WarehouseOptions = await _context.Warehouses.AsNoTracking()
                .Include(warehouse => warehouse.Branch)
                .Where(warehouse => warehouse.IsActive)
                .OrderBy(warehouse => warehouse.Branch.BranchName)
                .ThenBy(warehouse => warehouse.Name)
                .Select(warehouse => new ManagementSelectOption
                {
                    Value = warehouse.Id.ToString(),
                    Label = warehouse.Branch.BranchName + " · " + warehouse.Code + " - " + warehouse.Name
                })
                .ToListAsync();
        }

        private async Task<IReadOnlyList<PayrollItem>> BuildPayrollAsync(List<AppUser> users, Dictionary<string, string> roles, int branchId, DateTime selectedDate)
        {
            var monthStart = new DateTime(selectedDate.Year, selectedDate.Month, 1);
            var startUtc = ToUtc(monthStart);
            var endUtc = ToUtc(monthStart.AddMonths(1));
            var branchRevenue = await _context.Orders.Where(order => order.BranchId == branchId && order.Status == Order.StatusCompleted && order.OrderDate >= startUtc && order.OrderDate < endUtc).SumAsync(order => (decimal?)order.TotalAmount) ?? 0;
            var salesByUser = await _context.Orders.Where(order => order.BranchId == branchId && order.Status == Order.StatusCompleted && order.OrderDate >= startUtc && order.OrderDate < endUtc)
                .GroupBy(order => order.UserId).Select(group => new { UserId = group.Key, Revenue = group.Sum(order => order.TotalAmount) }).ToDictionaryAsync(item => item.UserId, item => item.Revenue);
            var violations = await _context.ShiftAssignments.Where(item => item.WorkShift.BranchId == branchId && item.WorkShift.ShiftDate >= monthStart && item.WorkShift.ShiftDate < monthStart.AddMonths(1) && (item.AttendanceStatus.Contains("Red") || item.AttendanceStatus == "Absent"))
                .Select(item => item.UserId).Distinct().ToListAsync();
            var result = new List<PayrollItem>();
            foreach (var user in users.Where(user => user.IsActive))
            {
                var role = roles[user.Id];
                decimal baseSalary;
                decimal responsibility = 0;
                decimal kpi = 0;
                if (role == RoleCatalog.Manager || role == RoleCatalog.BranchOwner)
                {
                    baseSalary = 17_000_000m;
                    var branchCount = await _context.UserFeaturePermissions.Where(permission => permission.UserId == user.Id && permission.IsGranted && permission.BranchId.HasValue).Select(permission => permission.BranchId).Distinct().CountAsync();
                    responsibility = 3_000_000m * Math.Max(1, branchCount);
                    kpi = branchRevenue * 0.005m;
                }
                else if (role == RoleCatalog.WarehouseManager)
                    baseSalary = 15_000_000m;
                else if (role == RoleCatalog.Accountant)
                {
                    baseSalary = 15_000_000m;
                    responsibility = 3_000_000m;
                }
                else
                {
                    baseSalary = 13_000_000m;
                    if (role == RoleCatalog.Sales || role == RoleCatalog.Staff)
                        kpi = (salesByUser.TryGetValue(user.Id, out var revenue) ? revenue : 0) * 0.01m;
                }
                var hasViolation = violations.Contains(user.Id);
                var attendance = hasViolation || role is RoleCatalog.Manager or RoleCatalog.BranchOwner ? 0 : (baseSalary + responsibility + kpi) * 0.01m;
                result.Add(new PayrollItem { UserId = user.Id, FullName = user.FullName, Role = role, BaseSalary = baseSalary, ResponsibilityBonus = responsibility, KpiBonus = kpi, AttendanceBonus = attendance, TotalSalary = baseSalary + responsibility + kpi + attendance, HasAttendanceViolation = hasViolation });
            }
            return result.OrderByDescending(item => RoleCatalog.GetPriority(item.Role)).ThenBy(item => item.FullName).ToList();
        }

        private async Task<IReadOnlyList<RevenuePoint>> BuildRevenuePointsAsync(int branchId, string period, DateTime date)
        {
            DateTime startLocal;
            DateTime endLocal;
            if (period == "day") { startLocal = date.Date; endLocal = startLocal.AddDays(1); }
            else if (period == "year") { startLocal = new DateTime(date.Year, 1, 1); endLocal = startLocal.AddYears(1); }
            else { startLocal = new DateTime(date.Year, date.Month, 1); endLocal = startLocal.AddMonths(1); }
            var orders = await _context.Orders.AsNoTracking().Where(order => order.BranchId == branchId && order.Status == Order.StatusCompleted && order.OrderDate >= ToUtc(startLocal) && order.OrderDate < ToUtc(endLocal)).ToListAsync();
            if (period == "day")
                return Enumerable.Range(0, 24).Select(hour => new RevenuePoint { Label = $"{hour:00}:00", Amount = orders.Where(order => FromUtc(order.OrderDate).Hour == hour).Sum(order => order.TotalAmount), OrderCount = orders.Count(order => FromUtc(order.OrderDate).Hour == hour) }).ToList();
            if (period == "year")
                return Enumerable.Range(1, 12).Select(month => new RevenuePoint { Label = $"Tháng {month}", Amount = orders.Where(order => FromUtc(order.OrderDate).Month == month).Sum(order => order.TotalAmount), OrderCount = orders.Count(order => FromUtc(order.OrderDate).Month == month) }).ToList();
            return Enumerable.Range(1, DateTime.DaysInMonth(date.Year, date.Month)).Select(day => new RevenuePoint { Label = day.ToString("00"), Amount = orders.Where(order => FromUtc(order.OrderDate).Day == day).Sum(order => order.TotalAmount), OrderCount = orders.Count(order => FromUtc(order.OrderDate).Day == day) }).ToList();
        }

        private async Task<IReadOnlyList<ManagementUserItem>> BuildUsersAsync()
        {
            var users = await _context.Users.AsNoTracking().Include(user => user.Branch).OrderBy(user => user.FullName).ToListAsync();
            var result = new List<ManagementUserItem>();
            foreach (var user in users)
                result.Add(new ManagementUserItem { Id = user.Id, FullName = user.FullName, Email = user.Email, PhoneNumber = user.PhoneNumber, Role = RoleCatalog.GetHighestRole(await _userManager.GetRolesAsync(user)), IsActive = user.IsActive, BranchId = user.BranchId, BranchName = user.Branch?.BranchName });
            return result;
        }

        private IActionResult RedirectToDashboard(string tab, string subtab, int branchId, DateTime date) => RedirectToAction(nameof(Index), new { tab, subtab, branchId, date = date.ToString("yyyy-MM-dd") });
        private IActionResult ErrorRedirect(string message, string tab, string subtab = null, int? branchId = null, DateTime? date = null, string permissionUserId = null)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Index), new { tab, subtab, branchId, date = date?.ToString("yyyy-MM-dd"), permissionUserId, permissionBranchId = branchId });
        }

        private static bool CanOpenTab(string tab, HashSet<string> grants) => tab switch
        {
            "people" => grants.Overlaps(new[] { ManagementFeatureCatalog.PeopleView, ManagementFeatureCatalog.PeopleShifts, ManagementFeatureCatalog.PeoplePayroll }),
            "warehouse" => grants.Overlaps(new[] { ManagementFeatureCatalog.WarehouseSuppliers, ManagementFeatureCatalog.WarehouseReceipts, ManagementFeatureCatalog.WarehouseApproval }),
            "products" => grants.Overlaps(new[] { ManagementFeatureCatalog.ProductsView, ManagementFeatureCatalog.ProductsEdit }),
            "revenue" => grants.Contains(ManagementFeatureCatalog.RevenueView),
            "overview" => grants.Count > 0,
            _ => false
        };
        private static bool IsSystemTab(string tab) => tab is "users" or "permissions" or "branches" or "audit";
        private static string NormalizeTab(string tab) => (tab ?? "overview").ToLowerInvariant() switch { "warehouse" => "warehouse", "people" => "people", "products" => "products", "revenue" => "revenue", "users" => "users", "permissions" => "permissions", "branches" => "branches", "audit" => "audit", _ => "overview" };
        private static string NormalizeSubtab(string tab, string subtab) => tab switch
        {
            "people" => subtab is "shifts" or "payroll" ? subtab : "employees",
            "warehouse" => subtab == "receipts" ? "receipts" : "suppliers",
            _ => string.Empty
        };
        private static string ShiftLabel(string type) => type == "Afternoon" ? "ca chiều (16:00–00:00)" : "ca sáng (08:00–16:00)";
        private static void AppendSystemNote(ShiftAssignment assignment, string note) => assignment.SystemNote = string.IsNullOrWhiteSpace(assignment.SystemNote) ? note : $"{assignment.SystemNote}\n{note}";
        private bool CanUseAttendance() => !User.IsInRole(RoleCatalog.Admin)
            && RoleCatalog.IsPrivilegedPersistentRole(User.Claims
                .Where(claim => claim.Type == System.Security.Claims.ClaimTypes.Role)
                .Select(claim => claim.Value));

        private static readonly TimeZoneInfo VietnamTimeZone = ResolveVietnamTimeZone();
        private static TimeZoneInfo ResolveVietnamTimeZone()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
            catch { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); }
        }
        private static DateTime VietnamNow() => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);
        private static DateTime ToUtc(DateTime local) => TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), VietnamTimeZone);
        private static DateTime FromUtc(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), VietnamTimeZone);
    }
}
