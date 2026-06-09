using GoldManagementSystem.Data;
using GoldManagementSystem.Models;
using GoldManagementSystem.Models.ViewModels;
using GoldManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace GoldManagementSystem.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private const string ProfileTokenProvider = "Profile";
        private const string PendingEmailTokenName = "PendingEmail";
        private const string PendingEmailCodeTokenName = "PendingEmailCode";
        private const string PendingEmailCodeExpiryTokenName = "PendingEmailCodeExpiry";
        private const string PendingEmailAttemptCountTokenName = "PendingEmailAttemptCount";
        private const string PendingEmailLockedUntilTokenName = "PendingEmailLockedUntil";
        private const string PendingPhoneTokenName = "PendingPhone";
        private const string PendingPhoneCodeTokenName = "PendingPhoneCode";
        private const string PendingPhoneCodeExpiryTokenName = "PendingPhoneCodeExpiry";
        private const string PendingPhoneAttemptCountTokenName = "PendingPhoneAttemptCount";
        private const string PendingPhoneLockedUntilTokenName = "PendingPhoneLockedUntil";
        private const string DateOfBirthTokenName = "DateOfBirth";
        private static readonly TimeSpan VerificationCodeLifetime = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan VerificationLockoutDuration = TimeSpan.FromMinutes(30);
        private const int MaxVerificationAttempts = 5;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly AuthNotificationService _notificationService;
        private readonly AuthVerificationOptions _authVerificationOptions;

        public UserController(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            AuthNotificationService notificationService,
            IOptions<AuthVerificationOptions> authVerificationOptions)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _notificationService = notificationService;
            _authVerificationOptions = authVerificationOptions.Value ?? new AuthVerificationOptions();
        }

        // 1. Thông tin tài khoản
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            return View(await BuildProfileViewModelAsync(user));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UserProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            model.Email = ContactUtility.NormalizeEmail(model.Email);
            model.FullName = ContactUtility.Normalize(model.FullName);
            model.PhoneNumber = ContactUtility.NormalizePhone(model.PhoneNumber);
            model.DateOfBirth = model.DateOfBirth?.Date;
            var currentDateOfBirth = await GetDateOfBirthAsync(user);

            if (model.DateOfBirth.HasValue && model.DateOfBirth.Value.Date > DateTime.Today)
            {
                ModelState.AddModelError(nameof(model.DateOfBirth), "Ngày sinh không được lớn hơn ngày hiện tại.");
            }

            var emailChanged = !string.Equals(model.Email, user.Email, StringComparison.OrdinalIgnoreCase);
            var phoneChanged = !string.Equals(model.PhoneNumber, user.PhoneNumber, StringComparison.OrdinalIgnoreCase);

            if (emailChanged)
            {
                if (string.IsNullOrWhiteSpace(model.Email))
                {
                    ModelState.AddModelError(nameof(model.Email), "Chưa hỗ trợ xóa email khỏi tài khoản. Bạn có thể cập nhật sang email khác.");
                }

                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    ModelState.AddModelError(nameof(model.Email), "Email này đã được sử dụng bởi tài khoản khác.");
                }
            }

            if (phoneChanged && !string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                var existingPhoneOwner = await _userManager.Users
                    .FirstOrDefaultAsync(item => item.PhoneNumber == model.PhoneNumber);

                if (existingPhoneOwner != null && existingPhoneOwner.Id != user.Id)
                {
                    ModelState.AddModelError(nameof(model.PhoneNumber), "Số điện thoại này đã được sử dụng bởi tài khoản khác.");
                }
            }

            if (phoneChanged && string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                ModelState.AddModelError(nameof(model.PhoneNumber), "Chưa hỗ trợ xóa số điện thoại khỏi tài khoản. Bạn có thể cập nhật sang số khác.");
            }

            if (!ModelState.IsValid)
            {
                return View(await BuildProfileViewModelAsync(user, model));
            }

            var updates = new List<string>();
            var fullNameChanged = !string.Equals(model.FullName, user.FullName, StringComparison.Ordinal);
            var dateOfBirthChanged = model.DateOfBirth != currentDateOfBirth;

            if (fullNameChanged)
            {
                user.FullName = model.FullName;

                var profileResult = await _userManager.UpdateAsync(user);
                if (!profileResult.Succeeded)
                {
                    TempData["ErrorMessage"] = BuildIdentityErrorMessage(profileResult, "Không thể cập nhật thông tin cá nhân.");
                    return RedirectToAction(nameof(Profile));
                }

                updates.Add("Đã cập nhật họ và tên.");
                await _signInManager.RefreshSignInAsync(user);
            }

            if (dateOfBirthChanged)
            {
                await SetDateOfBirthAsync(user, model.DateOfBirth);
                updates.Add("Đã cập nhật ngày sinh.");
            }

            if (emailChanged && _authVerificationOptions.RequireProfileContactVerification)
            {
                var lockedEmailUntil = await GetActiveLockoutAsync(user, PendingEmailLockedUntilTokenName);
                if (lockedEmailUntil.HasValue)
                {
                    ModelState.AddModelError(nameof(model.Email), BuildLockoutMessage("xác nhận đổi email", lockedEmailUntil.Value));
                }
            }

            if (phoneChanged && _authVerificationOptions.RequireProfileContactVerification)
            {
                var lockedPhoneUntil = await GetActiveLockoutAsync(user, PendingPhoneLockedUntilTokenName);
                if (lockedPhoneUntil.HasValue)
                {
                    ModelState.AddModelError(nameof(model.PhoneNumber), BuildLockoutMessage("xác nhận đổi số điện thoại", lockedPhoneUntil.Value));
                }
            }

            if (!ModelState.IsValid)
            {
                return View(await BuildProfileViewModelAsync(user, model));
            }

            if (emailChanged)
            {
                if (_authVerificationOptions.RequireProfileContactVerification)
                {
                    var verificationCode = GenerateVerificationCode();

                    await _userManager.SetAuthenticationTokenAsync(user, ProfileTokenProvider, PendingEmailTokenName, model.Email);
                    await _userManager.SetAuthenticationTokenAsync(user, ProfileTokenProvider, PendingEmailCodeTokenName, verificationCode);
                    await _userManager.SetAuthenticationTokenAsync(
                        user,
                        ProfileTokenProvider,
                        PendingEmailCodeExpiryTokenName,
                        DateTimeOffset.UtcNow.Add(VerificationCodeLifetime).ToString("O"));
                    await _userManager.SetAuthenticationTokenAsync(user, ProfileTokenProvider, PendingEmailAttemptCountTokenName, "0");
                    await _userManager.RemoveAuthenticationTokenAsync(user, ProfileTokenProvider, PendingEmailLockedUntilTokenName);
                    await _notificationService.SendEmailChangeRequestedAsync(user.Email, user.FullName, model.Email);
                    await _notificationService.SendEmailChangeConfirmationAsync(model.Email, user.FullName, verificationCode);
                    updates.Add($"Đã gửi mã xác nhận tới {MaskEmail(model.Email)} để hoàn tất đổi email.");
                }
                else
                {
                    var updateEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                    if (!updateEmailResult.Succeeded)
                    {
                        TempData["ErrorMessage"] = BuildIdentityErrorMessage(updateEmailResult, "Không thể cập nhật email.");
                        return RedirectToAction(nameof(Profile));
                    }

                    var updateUserNameResult = await _userManager.SetUserNameAsync(user, model.Email);
                    if (!updateUserNameResult.Succeeded)
                    {
                        TempData["ErrorMessage"] = BuildIdentityErrorMessage(updateUserNameResult, "Email đã đổi nhưng không thể đồng bộ tên đăng nhập.");
                        return RedirectToAction(nameof(Profile));
                    }

                    user.EmailConfirmed = true;
                    var confirmEmailResult = await _userManager.UpdateAsync(user);
                    if (!confirmEmailResult.Succeeded)
                    {
                        TempData["ErrorMessage"] = BuildIdentityErrorMessage(confirmEmailResult, "Không thể hoàn tất cập nhật email.");
                        return RedirectToAction(nameof(Profile));
                    }

                    await ClearPendingEmailChangeAsync(user);
                    updates.Add($"Đã cập nhật email thành {model.Email}.");
                    await _signInManager.RefreshSignInAsync(user);
                }
            }

            if (phoneChanged)
            {
                if (_authVerificationOptions.RequireProfileContactVerification)
                {
                    var verificationCode = GenerateVerificationCode();
                    await _userManager.SetAuthenticationTokenAsync(user, ProfileTokenProvider, PendingPhoneTokenName, model.PhoneNumber);
                    await _userManager.SetAuthenticationTokenAsync(user, ProfileTokenProvider, PendingPhoneCodeTokenName, verificationCode);
                    await _userManager.SetAuthenticationTokenAsync(
                        user,
                        ProfileTokenProvider,
                        PendingPhoneCodeExpiryTokenName,
                        DateTimeOffset.UtcNow.Add(VerificationCodeLifetime).ToString("O"));
                    await _userManager.SetAuthenticationTokenAsync(user, ProfileTokenProvider, PendingPhoneAttemptCountTokenName, "0");
                    await _userManager.RemoveAuthenticationTokenAsync(user, ProfileTokenProvider, PendingPhoneLockedUntilTokenName);

                    if (!string.IsNullOrWhiteSpace(user.Email))
                    {
                        await _notificationService.SendPhoneChangeRequestedAsync(user.Email, user.FullName, model.PhoneNumber);
                    }

                    await _notificationService.SendPhoneVerificationCodeAsync(model.PhoneNumber, user.FullName, verificationCode);
                    updates.Add($"Đã gửi mã xác nhận tới {MaskPhone(model.PhoneNumber)} để hoàn tất đổi số điện thoại.");
                }
                else
                {
                    var updatePhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                    if (!updatePhoneResult.Succeeded)
                    {
                        TempData["ErrorMessage"] = BuildIdentityErrorMessage(updatePhoneResult, "Không thể cập nhật số điện thoại.");
                        return RedirectToAction(nameof(Profile));
                    }

                    user.PhoneNumberConfirmed = true;
                    var confirmPhoneResult = await _userManager.UpdateAsync(user);
                    if (!confirmPhoneResult.Succeeded)
                    {
                        TempData["ErrorMessage"] = BuildIdentityErrorMessage(confirmPhoneResult, "Không thể hoàn tất cập nhật số điện thoại.");
                        return RedirectToAction(nameof(Profile));
                    }

                    await ClearPendingPhoneChangeAsync(user);
                    updates.Add($"Đã cập nhật số điện thoại thành {MaskPhone(model.PhoneNumber)}.");
                    await _signInManager.RefreshSignInAsync(user);
                }
            }

            if (updates.Count == 0)
            {
                TempData["SuccessMessage"] = "Không có thay đổi nào cần cập nhật.";
            }
            else
            {
                TempData["SuccessMessage"] = string.Join(" ", updates);
            }

            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmEmailChange(UserProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var pendingEmail = await _userManager.GetAuthenticationTokenAsync(user, ProfileTokenProvider, PendingEmailTokenName);
            var pendingEmailCode = await _userManager.GetAuthenticationTokenAsync(user, ProfileTokenProvider, PendingEmailCodeTokenName);
            var pendingEmailCodeExpiry = await _userManager.GetAuthenticationTokenAsync(user, ProfileTokenProvider, PendingEmailCodeExpiryTokenName);

            if (string.IsNullOrWhiteSpace(pendingEmail) || string.IsNullOrWhiteSpace(pendingEmailCode))
            {
                TempData["ErrorMessage"] = "Hiện không có yêu cầu đổi email nào đang chờ xác nhận.";
                return RedirectToAction(nameof(Profile));
            }

            if (string.IsNullOrWhiteSpace(model.EmailVerificationCode))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập mã xác nhận đã gửi tới email mới.";
                return RedirectToAction(nameof(Profile));
            }

            var lockedEmailUntil = await GetActiveLockoutAsync(user, PendingEmailLockedUntilTokenName);
            if (lockedEmailUntil.HasValue)
            {
                TempData["ErrorMessage"] = BuildLockoutMessage("xác nhận đổi email", lockedEmailUntil.Value);
                return RedirectToAction(nameof(Profile));
            }

            if (IsVerificationCodeExpired(pendingEmailCodeExpiry))
            {
                await ClearPendingEmailChangeAsync(user);
                TempData["ErrorMessage"] = "Mã xác nhận email đã hết hạn. Vui lòng gửi lại yêu cầu đổi email.";
                return RedirectToAction(nameof(Profile));
            }

            if (!string.Equals(model.EmailVerificationCode?.Trim(), pendingEmailCode, StringComparison.Ordinal))
            {
                var failureResult = await RegisterFailureAsync(
                    user,
                    PendingEmailAttemptCountTokenName,
                    PendingEmailLockedUntilTokenName,
                    ClearPendingEmailChangeAsync);

                TempData["ErrorMessage"] = failureResult.IsLockedOut
                    ? BuildLockoutMessage("xác nhận đổi email", failureResult.LockedUntil)
                    : $"Mã xác nhận email không đúng. Bạn còn {failureResult.RemainingAttempts} lần thử trước khi bị khóa 30 phút.";
                return RedirectToAction(nameof(Profile));
            }

            var previousEmail = user.Email;

            var updateEmailResult = await _userManager.SetEmailAsync(user, pendingEmail);
            if (!updateEmailResult.Succeeded)
            {
                TempData["ErrorMessage"] = BuildIdentityErrorMessage(updateEmailResult, "Không thể xác nhận đổi email.");
                return RedirectToAction(nameof(Profile));
            }

            var updateUserNameResult = await _userManager.SetUserNameAsync(user, pendingEmail);
            if (!updateUserNameResult.Succeeded)
            {
                TempData["ErrorMessage"] = BuildIdentityErrorMessage(updateUserNameResult, "Email đã đổi nhưng không thể đồng bộ tên đăng nhập.");
                return RedirectToAction(nameof(Profile));
            }

            user.EmailConfirmed = true;
            var confirmResult = await _userManager.UpdateAsync(user);
            if (!confirmResult.Succeeded)
            {
                TempData["ErrorMessage"] = BuildIdentityErrorMessage(confirmResult, "Không thể hoàn tất xác nhận email.");
                return RedirectToAction(nameof(Profile));
            }

            await ClearPendingEmailChangeAsync(user);
            if (!string.IsNullOrWhiteSpace(previousEmail))
            {
                await _notificationService.SendEmailChangedSuccessfullyAsync(previousEmail, user.FullName, pendingEmail);
            }
            await _notificationService.SendEmailChangedSuccessfullyAsync(pendingEmail, user.FullName, pendingEmail);
            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] = $"Email tài khoản đã được cập nhật thành {pendingEmail}.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPhoneChange(UserProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var pendingPhoneNumber = await _userManager.GetAuthenticationTokenAsync(user, ProfileTokenProvider, PendingPhoneTokenName);
            if (string.IsNullOrWhiteSpace(pendingPhoneNumber))
            {
                TempData["ErrorMessage"] = "Hiện không có yêu cầu đổi số điện thoại nào đang chờ xác nhận.";
                return RedirectToAction(nameof(Profile));
            }

            if (string.IsNullOrWhiteSpace(model.PhoneVerificationCode))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập mã xác nhận đã gửi tới số điện thoại mới.";
                return RedirectToAction(nameof(Profile));
            }

            var pendingPhoneCode = await _userManager.GetAuthenticationTokenAsync(user, ProfileTokenProvider, PendingPhoneCodeTokenName);
            var pendingPhoneCodeExpiry = await _userManager.GetAuthenticationTokenAsync(user, ProfileTokenProvider, PendingPhoneCodeExpiryTokenName);

            var lockedPhoneUntil = await GetActiveLockoutAsync(user, PendingPhoneLockedUntilTokenName);
            if (lockedPhoneUntil.HasValue)
            {
                TempData["ErrorMessage"] = BuildLockoutMessage("xác nhận đổi số điện thoại", lockedPhoneUntil.Value);
                return RedirectToAction(nameof(Profile));
            }

            if (IsVerificationCodeExpired(pendingPhoneCodeExpiry))
            {
                await ClearPendingPhoneChangeAsync(user);
                TempData["ErrorMessage"] = "Mã xác nhận số điện thoại đã hết hạn. Vui lòng gửi lại yêu cầu đổi số điện thoại.";
                return RedirectToAction(nameof(Profile));
            }

            if (!string.Equals(model.PhoneVerificationCode?.Trim(), pendingPhoneCode, StringComparison.Ordinal))
            {
                var failureResult = await RegisterFailureAsync(
                    user,
                    PendingPhoneAttemptCountTokenName,
                    PendingPhoneLockedUntilTokenName,
                    ClearPendingPhoneChangeAsync);

                TempData["ErrorMessage"] = failureResult.IsLockedOut
                    ? BuildLockoutMessage("xác nhận đổi số điện thoại", failureResult.LockedUntil)
                    : $"Mã xác nhận số điện thoại không đúng. Bạn còn {failureResult.RemainingAttempts} lần thử trước khi bị khóa 30 phút.";
                return RedirectToAction(nameof(Profile));
            }

            var updatePhoneResult = await _userManager.SetPhoneNumberAsync(user, pendingPhoneNumber);
            if (!updatePhoneResult.Succeeded)
            {
                TempData["ErrorMessage"] = BuildIdentityErrorMessage(updatePhoneResult, "Không thể cập nhật số điện thoại.");
                return RedirectToAction(nameof(Profile));
            }

            user.PhoneNumberConfirmed = true;
            var confirmPhoneResult = await _userManager.UpdateAsync(user);
            if (!confirmPhoneResult.Succeeded)
            {
                TempData["ErrorMessage"] = BuildIdentityErrorMessage(confirmPhoneResult, "Không thể hoàn tất xác nhận số điện thoại.");
                return RedirectToAction(nameof(Profile));
            }

            await ClearPendingPhoneChangeAsync(user);
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                await _notificationService.SendPhoneChangedEmailNotificationAsync(user.Email, user.FullName, pendingPhoneNumber);
            }
            await _notificationService.SendPhoneChangedSmsNotificationAsync(pendingPhoneNumber, user.FullName);
            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] = $"Số điện thoại đã được cập nhật thành {MaskPhone(pendingPhoneNumber)}.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelEmailChange()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            await ClearPendingEmailChangeAsync(user);
            await _userManager.RemoveAuthenticationTokenAsync(user, ProfileTokenProvider, PendingEmailLockedUntilTokenName);
            TempData["SuccessMessage"] = "Đã hủy yêu cầu xác nhận đổi email.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelPhoneChange()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            await ClearPendingPhoneChangeAsync(user);
            await _userManager.RemoveAuthenticationTokenAsync(user, ProfileTokenProvider, PendingPhoneLockedUntilTokenName);
            TempData["SuccessMessage"] = "Đã hủy yêu cầu xác nhận đổi số điện thoại.";
            return RedirectToAction(nameof(Profile));
        }

        // 2. Đơn hàng của tôi
        public async Task<IActionResult> Orders()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // 3. Sản phẩm yêu thích
        public async Task<IActionResult> Favorites()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var favorites = await _context.FavoriteProducts
                .Include(f => f.Product)
                .ThenInclude(product => product.Branch)
                .Where(f => f.UserId == user.Id)
                .OrderByDescending(f => f.AddedAt)
                .ToListAsync();

            var products = favorites.Select(f => f.Product).ToList();
            return View(products);
        }

        // API Toggle Yêu thích
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var isAjaxRequest = string.Equals(
                Request.Headers["X-Requested-With"],
                "XMLHttpRequest",
                System.StringComparison.OrdinalIgnoreCase);
            var returnUrl = Request.Headers.Referer.ToString();
            IActionResult redirectResult = !string.IsNullOrWhiteSpace(returnUrl)
                ? Redirect(returnUrl)
                : RedirectToAction(nameof(Favorites));

            var existingFav = await _context.FavoriteProducts
                .FirstOrDefaultAsync(f => f.UserId == user.Id && f.ProductId == productId);

            if (existingFav != null)
            {
                _context.FavoriteProducts.Remove(existingFav);
                await _context.SaveChangesAsync();

                if (isAjaxRequest)
                {
                    return Json(new { success = true, isFavorite = false, message = "Đã bỏ yêu thích." });
                }

                TempData["SuccessMessage"] = "Đã bỏ sản phẩm khỏi danh sách yêu thích.";
                return redirectResult;
            }
            else
            {
                var newFav = new FavoriteProduct
                {
                    UserId = user.Id,
                    ProductId = productId
                };
                _context.FavoriteProducts.Add(newFav);
                await _context.SaveChangesAsync();

                if (isAjaxRequest)
                {
                    return Json(new { success = true, isFavorite = true, message = "Đã thêm vào yêu thích." });
                }

                TempData["SuccessMessage"] = "Đã thêm sản phẩm vào danh sách yêu thích.";
                return redirectResult;
            }
        }

        // 4. Đặt hàng (Checkout)
        [HttpGet]
        public async Task<IActionResult> Checkout(int id)
        {
            var product = await _context.Products
                .Include(p => p.Branch)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            if (product.Status == "Hết hàng" || product.Status == "Đã bán")
            {
                TempData["ErrorMessage"] = "Sản phẩm này hiện tại không khả dụng để đặt hàng.";
                return RedirectToAction("Details", "Products", new { id = id });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Lấy danh sách các chi nhánh đang hoạt động để người dùng có thể chọn nơi nhận hàng
            ViewBag.ActiveBranches = await _context.Branches
                .Where(b => b.IsActive)
                .OrderBy(b => b.BranchName)
                .ToListAsync();

            ViewBag.CustomerName = user.FullName;
            ViewBag.CustomerPhone = user.PhoneNumber;

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(int productId, string customerName, string customerPhone, int branchId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            if (product.Status == "Hết hàng" || product.Status == "Đã bán")
            {
                TempData["ErrorMessage"] = "Sản phẩm này đã được bán hoặc không còn hàng.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (string.IsNullOrWhiteSpace(customerName))
            {
                ModelState.AddModelError(nameof(customerName), "Vui lòng nhập tên người nhận.");
            }
            if (string.IsNullOrWhiteSpace(customerPhone))
            {
                ModelState.AddModelError(nameof(customerPhone), "Vui lòng nhập số điện thoại người nhận.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ActiveBranches = await _context.Branches
                    .Where(b => b.IsActive)
                    .OrderBy(b => b.BranchName)
                    .ToListAsync();
                ViewBag.CustomerName = customerName;
                ViewBag.CustomerPhone = customerPhone;
                return View("Checkout", product);
            }

            // Tạo đơn hàng mới
            var order = new Order
            {
                UserId = user.Id,
                BranchId = branchId,
                CustomerName = customerName.Trim(),
                CustomerPhone = customerPhone.Trim(),
                TotalAmount = product.SellPrice,
                Status = "Đang xử lý",
                OrderDate = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Tạo chi tiết đơn hàng
            var orderDetail = new OrderDetail
            {
                OrderId = order.Id,
                ProductId = product.Id,
                UnitPrice = product.SellPrice,
                Quantity = 1
            };

            _context.OrderDetails.Add(orderDetail);

            // Cập nhật trạng thái sản phẩm sang Đã bán
            product.Status = "Đã bán";
            _context.Products.Update(product);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đặt hàng thành công! Mã đơn hàng của bạn là #{order.OrderNumber}.";
            return RedirectToAction(nameof(Orders));
        }

        private async Task<UserProfileViewModel> BuildProfileViewModelAsync(AppUser user, UserProfileViewModel sourceModel = null)
        {
            var pendingEmail = await _userManager.GetAuthenticationTokenAsync(user, ProfileTokenProvider, PendingEmailTokenName);
            var pendingPhoneNumber = await _userManager.GetAuthenticationTokenAsync(user, ProfileTokenProvider, PendingPhoneTokenName);

            return new UserProfileViewModel
            {
                Email = sourceModel?.Email ?? user.Email,
                FullName = sourceModel?.FullName ?? user.FullName,
                PhoneNumber = sourceModel?.PhoneNumber ?? user.PhoneNumber,
                DateOfBirth = sourceModel?.DateOfBirth ?? await GetDateOfBirthAsync(user),
                EmailVerificationCode = sourceModel?.EmailVerificationCode,
                PhoneVerificationCode = sourceModel?.PhoneVerificationCode,
                PendingEmail = pendingEmail,
                PendingPhoneNumber = pendingPhoneNumber,
                PendingEmailDisplay = MaskEmail(pendingEmail),
                PendingPhoneDisplay = MaskPhone(pendingPhoneNumber)
            };
        }

        private IActionResult RedirectAfterProfileAction()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(nameof(Profile));
            }

            return RedirectToAction("Login", "Account");
        }

        private async Task<DateTime?> GetDateOfBirthAsync(AppUser user)
        {
            var rawValue = await _userManager.GetAuthenticationTokenAsync(user, ProfileTokenProvider, DateOfBirthTokenName);
            if (DateTime.TryParse(rawValue, out var parsedDate))
            {
                return parsedDate.Date;
            }

            return null;
        }

        private async Task SetDateOfBirthAsync(AppUser user, DateTime? dateOfBirth)
        {
            if (dateOfBirth.HasValue)
            {
                await _userManager.SetAuthenticationTokenAsync(
                    user,
                    ProfileTokenProvider,
                    DateOfBirthTokenName,
                    dateOfBirth.Value.ToString("yyyy-MM-dd"));
                return;
            }

            await _userManager.RemoveAuthenticationTokenAsync(user, ProfileTokenProvider, DateOfBirthTokenName);
        }

        private async Task ClearPendingEmailChangeAsync(AppUser user)
        {
            await _userManager.RemoveAuthenticationTokenAsync(user, ProfileTokenProvider, PendingEmailTokenName);
            await _userManager.RemoveAuthenticationTokenAsync(user, ProfileTokenProvider, PendingEmailCodeTokenName);
            await _userManager.RemoveAuthenticationTokenAsync(user, ProfileTokenProvider, PendingEmailCodeExpiryTokenName);
            await _userManager.RemoveAuthenticationTokenAsync(user, ProfileTokenProvider, PendingEmailAttemptCountTokenName);
        }

        private async Task ClearPendingPhoneChangeAsync(AppUser user)
        {
            await _userManager.RemoveAuthenticationTokenAsync(user, ProfileTokenProvider, PendingPhoneTokenName);
            await _userManager.RemoveAuthenticationTokenAsync(user, ProfileTokenProvider, PendingPhoneCodeTokenName);
            await _userManager.RemoveAuthenticationTokenAsync(user, ProfileTokenProvider, PendingPhoneCodeExpiryTokenName);
            await _userManager.RemoveAuthenticationTokenAsync(user, ProfileTokenProvider, PendingPhoneAttemptCountTokenName);
        }

        private static bool IsVerificationCodeExpired(string rawExpiry)
        {
            return !DateTimeOffset.TryParse(rawExpiry, out var expiresAt) || expiresAt < DateTimeOffset.UtcNow;
        }

        private static string GenerateVerificationCode()
        {
            return RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        }

        private async Task<DateTimeOffset?> GetActiveLockoutAsync(AppUser user, string lockTokenName)
        {
            var rawValue = await _userManager.GetAuthenticationTokenAsync(user, ProfileTokenProvider, lockTokenName);
            if (!DateTimeOffset.TryParse(rawValue, out var lockedUntil))
            {
                return null;
            }

            if (lockedUntil <= DateTimeOffset.UtcNow)
            {
                await _userManager.RemoveAuthenticationTokenAsync(user, ProfileTokenProvider, lockTokenName);
                return null;
            }

            return lockedUntil;
        }

        private async Task<VerificationFailureState> RegisterFailureAsync(
            AppUser user,
            string attemptTokenName,
            string lockTokenName,
            Func<AppUser, Task> clearPendingAction)
        {
            var rawAttempts = await _userManager.GetAuthenticationTokenAsync(user, ProfileTokenProvider, attemptTokenName);
            var failedAttempts = int.TryParse(rawAttempts, out var parsedAttempts) ? parsedAttempts : 0;
            failedAttempts++;

            if (failedAttempts >= MaxVerificationAttempts)
            {
                var lockedUntil = DateTimeOffset.UtcNow.Add(VerificationLockoutDuration);
                await clearPendingAction(user);
                await _userManager.SetAuthenticationTokenAsync(user, ProfileTokenProvider, lockTokenName, lockedUntil.ToString("O"));

                return new VerificationFailureState
                {
                    IsLockedOut = true,
                    RemainingAttempts = 0,
                    LockedUntil = lockedUntil
                };
            }

            await _userManager.SetAuthenticationTokenAsync(user, ProfileTokenProvider, attemptTokenName, failedAttempts.ToString());
            return new VerificationFailureState
            {
                IsLockedOut = false,
                RemainingAttempts = MaxVerificationAttempts - failedAttempts
            };
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

        private static string BuildLockoutMessage(string actionLabel, DateTimeOffset lockedUntil)
        {
            return $"Bạn đã nhập sai mã 5 lần. Chức năng {actionLabel} tạm khóa đến {lockedUntil.ToLocalTime():HH:mm dd/MM/yyyy}.";
        }

        private static string MaskEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            {
                return email;
            }

            var parts = email.Split('@', 2);
            var localPart = parts[0];
            if (localPart.Length <= 2)
            {
                return $"{localPart[0]}*@{parts[1]}";
            }

            return $"{localPart[0]}{new string('*', Math.Max(1, localPart.Length - 2))}{localPart[^1]}@{parts[1]}";
        }

        private static string MaskPhone(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length <= 4)
            {
                return phoneNumber;
            }

            return $"{phoneNumber[..2]}{new string('*', Math.Max(1, phoneNumber.Length - 4))}{phoneNumber[^2..]}";
        }

        private class VerificationFailureState
        {
            public bool IsLockedOut { get; set; }
            public int RemainingAttempts { get; set; }
            public DateTimeOffset LockedUntil { get; set; }
        }
    }
}
