using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GoldManagementSystem.Models;
using GoldManagementSystem.Models.ViewModels;
using GoldManagementSystem.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GoldManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly AuthNotificationService _notificationService;
        private readonly PendingAccountVerificationService _pendingVerificationService;
        private readonly IPasswordValidator<AppUser>[] _passwordValidators;
        private readonly AuthVerificationOptions _authVerificationOptions;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            AuthNotificationService notificationService,
            PendingAccountVerificationService pendingVerificationService,
            System.Collections.Generic.IEnumerable<IPasswordValidator<AppUser>> passwordValidators,
            IOptions<AuthVerificationOptions> authVerificationOptions)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _notificationService = notificationService;
            _pendingVerificationService = pendingVerificationService;
            _passwordValidators = passwordValidators.ToArray();
            _authVerificationOptions = authVerificationOptions.Value ?? new AuthVerificationOptions();
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            if (Request.Query.TryGetValue("accountLocked", out var lockedFlag)
                && string.Equals(lockedFlag, "1", StringComparison.Ordinal))
            {
                ViewBag.AccountLockedMessage = "Tài khoản của bạn đã bị khóa hoặc đã ngừng hoạt động. Hệ thống đã đăng xuất và chặn truy cập cho đến khi quản trị viên mở khóa.";
            }

            return View(new LoginViewModel
            {
                VerificationChannel = VerificationChannelOptions.Phone,
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            model.Identifier = ContactUtility.NormalizeIdentifier(model.Identifier);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await FindUserByIdentifierAsync(model.Identifier);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email/SĐT hoặc mật khẩu không đúng.");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.");
                return View(model);
            }

            if (_pendingVerificationService.TryGetLoginLockout(user.Id, out var loginLockedUntil))
            {
                ModelState.AddModelError(string.Empty, BuildLockoutMessage("xác nhận đăng nhập", loginLockedUntil));
                return View(model);
            }

            var signInCheck = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);
            if (!signInCheck.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Email/SĐT hoặc mật khẩu không đúng.");
                return View(model);
            }
            
            if (!_authVerificationOptions.RequireLoginVerification)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var usePersistentCookie = model.RememberMe || RoleCatalog.IsPrivilegedPersistentRole(userRoles);
                await _signInManager.SignInAsync(user, usePersistentCookie);

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                if (userRoles.Contains(RoleCatalog.Admin))
                {
                    return RedirectToAction("Index", "Management");
                }

                return RedirectToAction("Index", "Home");
            }

            model.VerificationChannel = ResolvePreferredVerificationChannel(user.Email, user.PhoneNumber);
            var destination = ResolveVerificationDestination(user, model.VerificationChannel);
            if (string.IsNullOrWhiteSpace(destination))
            {
                ModelState.AddModelError(
                    string.Empty,
                    string.Equals(model.VerificationChannel, VerificationChannelOptions.Phone, StringComparison.OrdinalIgnoreCase)
                        ? "Tài khoản này chưa có số điện thoại để nhận mã xác nhận."
                        : "Tài khoản này chưa có email để nhận mã xác nhận.");
                return View(model);
            }

            var verification = _pendingVerificationService.CreateLoginVerification(
                user.Id,
                model.RememberMe,
                model.VerificationChannel,
                destination);

            await _notificationService.SendLoginVerificationCodeAsync(destination, user.FullName, verification.VerificationCode);

            return View(BuildLoginVerificationViewModel(model, destination, verification));
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel
            {
                VerificationChannel = VerificationChannelOptions.Phone
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            model.FullName = ContactUtility.Normalize(model.FullName);
            model.Identifier = ContactUtility.Normalize(model.Identifier);
            model.Email = null;
            model.PhoneNumber = null;

            if (!string.IsNullOrWhiteSpace(model.Identifier))
            {
                if (ContactUtility.LooksLikeEmail(model.Identifier))
                {
                    if (!new EmailAddressAttribute().IsValid(model.Identifier))
                    {
                        ModelState.AddModelError(nameof(model.Identifier), "Email không đúng định dạng.");
                    }
                    else
                    {
                        model.Email = ContactUtility.NormalizeEmail(model.Identifier);
                        model.Identifier = model.Email;
                    }
                }
                else
                {
                    if (!new PhoneAttribute().IsValid(model.Identifier))
                    {
                        ModelState.AddModelError(nameof(model.Identifier), "Số điện thoại không đúng định dạng.");
                    }
                    else
                    {
                        model.PhoneNumber = ContactUtility.NormalizePhone(model.Identifier);
                        model.Identifier = model.PhoneNumber;
                    }
                }
            }

            await ValidateRegisterRequestAsync(model);
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!_authVerificationOptions.RequireRegistrationVerification)
            {
                var user = new AppUser
                {
                    UserName = ResolveUserName(model.Email, model.PhoneNumber),
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    EmailConfirmed = !string.IsNullOrWhiteSpace(model.Email),
                    PhoneNumberConfirmed = !string.IsNullOrWhiteSpace(model.PhoneNumber),
                    IsActive = true
                };

                var createResult = await _userManager.CreateAsync(user, model.Password);
                if (!createResult.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, BuildIdentityErrorMessage(createResult, "Không thể tạo tài khoản."));
                    return View(model);
                }

                var roleResult = await _userManager.AddToRoleAsync(user, "Khách hàng");
                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                    ModelState.AddModelError(string.Empty, BuildIdentityErrorMessage(roleResult, "Không thể gán quyền mặc định cho tài khoản."));
                    return View(model);
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            var registrationKey = ResolveRegistrationKey(model.Email, model.PhoneNumber);
            if (_pendingVerificationService.TryGetRegistrationLockout(registrationKey, out var registerLockedUntil))
            {
                ModelState.AddModelError(string.Empty, BuildLockoutMessage("xác nhận đăng ký", registerLockedUntil));
                return View(model);
            }

            model.VerificationChannel = ResolvePreferredVerificationChannel(model.Email, model.PhoneNumber);
            var destination = ResolveDestination(model.Email, model.PhoneNumber, model.VerificationChannel);

            var verification = _pendingVerificationService.CreateRegistrationVerification(
                registrationKey,
                model.FullName,
                model.Email,
                model.PhoneNumber,
                model.Password,
                model.VerificationChannel);

            await _notificationService.SendRegisterVerificationCodeAsync(destination, model.FullName, verification.VerificationCode);

            return View(BuildRegisterVerificationViewModel(model, destination, verification));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendLoginCode(string verificationFlowId)
        {
            var verification = _pendingVerificationService.GetLoginVerification(verificationFlowId);
            if (verification == null)
            {
                ModelState.AddModelError(string.Empty, "Phiên xác nhận đăng nhập đã hết hạn hoặc không tồn tại. Vui lòng đăng nhập lại.");
                return View("Login", new LoginViewModel
                {
                    VerificationChannel = VerificationChannelOptions.Phone
                });
            }

            var user = await _userManager.FindByIdAsync(verification.UserId);
            if (user == null || !user.IsActive)
            {
                _pendingVerificationService.RemoveLoginVerification(verificationFlowId);
                ModelState.AddModelError(string.Empty, "Không thể cấp lại mã cho tài khoản này. Vui lòng đăng nhập lại.");
                return View("Login", new LoginViewModel
                {
                    VerificationChannel = ResolvePreferredVerificationChannel(user?.Email, user?.PhoneNumber)
                });
            }

            if (_pendingVerificationService.TryGetLoginLockout(user.Id, out var loginLockedUntil))
            {
                ModelState.AddModelError(string.Empty, BuildLockoutMessage("xác nhận đăng nhập", loginLockedUntil));
                return View("Login", new LoginViewModel
                {
                    VerificationChannel = ResolvePreferredVerificationChannel(user.Email, user.PhoneNumber)
                });
            }

            verification = _pendingVerificationService.RefreshLoginVerification(verification);
            await _notificationService.SendLoginVerificationCodeAsync(verification.Destination, user.FullName, verification.VerificationCode);

            ViewBag.VerificationInfoMessage = $"Đã gửi mã xác nhận mới tới {MaskDestination(verification.Destination, verification.VerificationChannel)}.";

            return View("Login", BuildLoginVerificationViewModel(
                new LoginViewModel
                {
                    RememberMe = verification.RememberMe
                },
                verification.Destination,
                verification));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelLoginVerification(string verificationFlowId)
        {
            var verification = _pendingVerificationService.GetLoginVerification(verificationFlowId);
            _pendingVerificationService.RemoveLoginVerification(verificationFlowId);

            ViewBag.VerificationInfoMessage = "Đã hủy xác nhận đăng nhập. Bạn có thể nhập lại thông tin để bắt đầu phiên mới.";
            return View("Login", new LoginViewModel
            {
                VerificationChannel = verification?.VerificationChannel ?? VerificationChannelOptions.Phone
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendRegisterCode(string verificationFlowId)
        {
            var verification = _pendingVerificationService.GetRegistrationVerification(verificationFlowId);
            if (verification == null)
            {
                ModelState.AddModelError(string.Empty, "Phiên xác nhận đăng ký đã hết hạn hoặc không tồn tại. Vui lòng đăng ký lại.");
                return View("Register", new RegisterViewModel
                {
                    VerificationChannel = VerificationChannelOptions.Phone
                });
            }

            if (_pendingVerificationService.TryGetRegistrationLockout(verification.RegistrationKey, out var registerLockedUntil))
            {
                ModelState.AddModelError(string.Empty, BuildLockoutMessage("xác nhận đăng ký", registerLockedUntil));
                return View("Register", new RegisterViewModel
                {
                    VerificationChannel = ResolvePreferredVerificationChannel(verification.Email, verification.PhoneNumber)
                });
            }

            verification = _pendingVerificationService.RefreshRegistrationVerification(verification);
            var destination = ResolveRegisterDestination(verification);
            await _notificationService.SendRegisterVerificationCodeAsync(destination, verification.FullName, verification.VerificationCode);

            ViewBag.VerificationInfoMessage = $"Đã gửi mã xác nhận mới tới {MaskDestination(destination, verification.VerificationChannel)}.";

            return View("Register", BuildRegisterVerificationViewModel(
                new RegisterViewModel(),
                destination,
                verification));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelRegisterVerification(string verificationFlowId)
        {
            var verification = _pendingVerificationService.GetRegistrationVerification(verificationFlowId);
            _pendingVerificationService.RemoveRegistrationVerification(verificationFlowId);

            ViewBag.VerificationInfoMessage = "Đã hủy xác nhận đăng ký. Bạn có thể nhập lại thông tin để bắt đầu phiên mới.";
            return View("Register", new RegisterViewModel
            {
                VerificationChannel = verification?.VerificationChannel ?? VerificationChannelOptions.Phone
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyLogin(LoginViewModel model)
        {
            ModelState.Remove(nameof(model.Identifier));
            ModelState.Remove(nameof(model.Password));
            ModelState.Remove(nameof(model.VerificationChannel));

            if (string.IsNullOrWhiteSpace(model.VerificationCode))
            {
                ModelState.AddModelError(nameof(model.VerificationCode), "Vui lòng nhập mã xác nhận đã được gửi.");
            }

            var verification = _pendingVerificationService.GetLoginVerification(model.VerificationFlowId);
            if (verification == null)
            {
                ModelState.AddModelError(string.Empty, "Phiên xác nhận đăng nhập đã hết hạn hoặc không tồn tại. Vui lòng đăng nhập lại.");
                return View("Login", new LoginViewModel
                {
                    VerificationChannel = VerificationChannelOptions.Phone
                });
            }

            var user = await _userManager.FindByIdAsync(verification.UserId);
            if (user == null || !user.IsActive)
            {
                _pendingVerificationService.RemoveLoginVerification(model.VerificationFlowId);
                ModelState.AddModelError(string.Empty, "Không thể hoàn tất đăng nhập cho tài khoản này.");
            }
            else if (!_pendingVerificationService.IsCodeValid(verification.VerificationCode, model.VerificationCode))
            {
                var failureResult = _pendingVerificationService.RegisterLoginFailure(verification);
                if (failureResult.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, BuildLockoutMessage("xác nhận đăng nhập", failureResult.LockedUntil));
                    return View("Login", new LoginViewModel
                    {
                        VerificationChannel = ResolvePreferredVerificationChannel(user?.Email, user?.PhoneNumber)
                    });
                }

                ModelState.AddModelError(
                    nameof(model.VerificationCode),
                    $"Mã xác nhận không đúng. Bạn còn {failureResult.RemainingAttempts} lần thử trước khi bị khóa 30 phút.");
            }

            if (!ModelState.IsValid)
            {
                return View("Login", BuildLoginVerificationViewModel(
                    model,
                    verification?.Destination,
                    verification));
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var usePersistentCookie = verification.RememberMe || RoleCatalog.IsPrivilegedPersistentRole(userRoles);
            await _signInManager.SignInAsync(user, usePersistentCookie);
            await _notificationService.SendLoginNotificationAsync(verification.Destination, user.FullName);
            _pendingVerificationService.RemoveLoginVerification(model.VerificationFlowId);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }
            if (userRoles.Contains(RoleCatalog.Admin))
            {
                return RedirectToAction("Index", "Management");
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyRegister(RegisterViewModel model)
        {
            ModelState.Remove(nameof(model.FullName));
            ModelState.Remove(nameof(model.Identifier));
            ModelState.Remove(nameof(model.Email));
            ModelState.Remove(nameof(model.PhoneNumber));
            ModelState.Remove(nameof(model.Password));
            ModelState.Remove(nameof(model.VerificationChannel));

            if (string.IsNullOrWhiteSpace(model.VerificationCode))
            {
                ModelState.AddModelError(nameof(model.VerificationCode), "Vui lòng nhập mã xác nhận đã được gửi.");
            }

            var verification = _pendingVerificationService.GetRegistrationVerification(model.VerificationFlowId);
            if (verification == null)
            {
                ModelState.AddModelError(string.Empty, "Phiên xác nhận đăng ký đã hết hạn hoặc không tồn tại. Vui lòng đăng ký lại.");
                return View("Register", new RegisterViewModel
                {
                    VerificationChannel = ResolvePreferredVerificationChannel(verification?.Email, verification?.PhoneNumber)
                });
            }

            if (!_pendingVerificationService.IsCodeValid(verification.VerificationCode, model.VerificationCode))
            {
                var failureResult = _pendingVerificationService.RegisterRegistrationFailure(verification);
                if (failureResult.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, BuildLockoutMessage("xác nhận đăng ký", failureResult.LockedUntil));
                    return View("Register", new RegisterViewModel
                    {
                        VerificationChannel = ResolvePreferredVerificationChannel(verification?.Email, verification?.PhoneNumber)
                    });
                }

                ModelState.AddModelError(
                    nameof(model.VerificationCode),
                    $"Mã xác nhận không đúng. Bạn còn {failureResult.RemainingAttempts} lần thử trước khi bị khóa 30 phút.");
            }

            await ValidatePendingRegistrationAsync(verification);

            if (!ModelState.IsValid)
            {
                return View("Register", BuildRegisterVerificationViewModel(
                    model,
                    ResolveRegisterDestination(verification),
                    verification));
            }

            var user = new AppUser
            {
                UserName = ResolveUserName(verification.Email, verification.PhoneNumber),
                Email = verification.Email,
                FullName = verification.FullName,
                PhoneNumber = verification.PhoneNumber,
                EmailConfirmed = string.Equals(verification.VerificationChannel, VerificationChannelOptions.Email, StringComparison.OrdinalIgnoreCase),
                PhoneNumberConfirmed = string.Equals(verification.VerificationChannel, VerificationChannelOptions.Phone, StringComparison.OrdinalIgnoreCase)
            };

            var createResult = await _userManager.CreateAsync(user, verification.Password);
            if (!createResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, BuildIdentityErrorMessage(createResult, "Không thể tạo tài khoản."));
                return View("Register", BuildRegisterVerificationViewModel(
                    model,
                    ResolveRegisterDestination(verification),
                    verification));
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "Khách hàng");
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                ModelState.AddModelError(string.Empty, BuildIdentityErrorMessage(roleResult, "Không thể gán quyền mặc định cho tài khoản."));
                return View("Register", BuildRegisterVerificationViewModel(
                    model,
                    ResolveRegisterDestination(verification),
                    verification));
            }

            await _notificationService.SendRegisterNotificationAsync(ResolveRegisterDestination(verification), verification.FullName);
            _pendingVerificationService.RemoveRegistrationVerification(model.VerificationFlowId);
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        private async Task ValidateRegisterRequestAsync(RegisterViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.Email)
                && await _userManager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Email này đã được sử dụng bởi tài khoản khác.");
            }

            if (!string.IsNullOrWhiteSpace(model.PhoneNumber)
                && await _userManager.Users.AnyAsync(user => user.PhoneNumber == model.PhoneNumber))
            {
                ModelState.AddModelError(nameof(model.PhoneNumber), "Số điện thoại này đã được sử dụng bởi tài khoản khác.");
            }

            var tempUser = new AppUser
            {
                UserName = ResolveUserName(model.Email, model.PhoneNumber),
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                FullName = model.FullName
            };

            foreach (var validator in _passwordValidators)
            {
                var passwordResult = await validator.ValidateAsync(_userManager, tempUser, model.Password);
                if (passwordResult.Succeeded)
                {
                    continue;
                }

                foreach (var error in passwordResult.Errors)
                {
                    if (!string.IsNullOrWhiteSpace(error.Description))
                    {
                        ModelState.AddModelError(nameof(model.Password), error.Description);
                    }
                }
            }
        }

        private async Task ValidatePendingRegistrationAsync(PendingRegistrationVerification verification)
        {
            if (!string.IsNullOrWhiteSpace(verification.Email)
                && await _userManager.FindByEmailAsync(verification.Email) != null)
            {
                ModelState.AddModelError(string.Empty, "Email này vừa được sử dụng bởi tài khoản khác. Vui lòng đăng ký lại.");
            }

            if (!string.IsNullOrWhiteSpace(verification.PhoneNumber)
                && await _userManager.Users.AnyAsync(user => user.PhoneNumber == verification.PhoneNumber))
            {
                ModelState.AddModelError(string.Empty, "Số điện thoại này vừa được sử dụng bởi tài khoản khác. Vui lòng đăng ký lại.");
            }
        }

        private async Task<AppUser> FindUserByIdentifierAsync(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return null;
            }

            if (ContactUtility.LooksLikeEmail(identifier))
            {
                return await _userManager.FindByEmailAsync(identifier);
            }

            return await _userManager.Users.FirstOrDefaultAsync(item => item.PhoneNumber == identifier);
        }

        private static string ResolveVerificationDestination(AppUser user, string verificationChannel)
        {
            return ResolveDestination(user?.Email, user?.PhoneNumber, verificationChannel);
        }

        private static string ResolveRegisterDestination(PendingRegistrationVerification verification)
        {
            return ResolveDestination(verification?.Email, verification?.PhoneNumber, verification?.VerificationChannel);
        }

        private static LoginViewModel BuildLoginVerificationViewModel(
            LoginViewModel source,
            string destination,
            PendingLoginVerification verification)
        {
            return new LoginViewModel
            {
                Identifier = source?.Identifier,
                RememberMe = source?.RememberMe ?? false,
                RequiresVerification = true,
                VerificationFlowId = verification?.FlowId ?? source?.VerificationFlowId,
                VerificationCode = source?.VerificationCode,
                VerificationChannel = verification?.VerificationChannel ?? source?.VerificationChannel,
                VerificationDestinationDisplay = MaskDestination(destination, verification?.VerificationChannel ?? source?.VerificationChannel),
                VerificationExpiresAt = verification?.ExpiresAt,
                ReturnUrl = source?.ReturnUrl
            };
        }

        private static RegisterViewModel BuildRegisterVerificationViewModel(
            RegisterViewModel source,
            string destination,
            PendingRegistrationVerification verification)
        {
            return new RegisterViewModel
            {
                FullName = source?.FullName,
                Identifier = source?.Identifier
                    ?? source?.Email
                    ?? source?.PhoneNumber
                    ?? verification?.Email
                    ?? verification?.PhoneNumber,
                Email = source?.Email,
                PhoneNumber = source?.PhoneNumber,
                VerificationChannel = verification?.VerificationChannel ?? source?.VerificationChannel,
                RequiresVerification = true,
                VerificationFlowId = verification?.FlowId ?? source?.VerificationFlowId,
                VerificationCode = source?.VerificationCode,
                VerificationDestinationDisplay = MaskDestination(destination, verification?.VerificationChannel ?? source?.VerificationChannel),
                VerificationExpiresAt = verification?.ExpiresAt
            };
        }

        private static string ResolvePreferredVerificationChannel(string email, string phoneNumber)
        {
            return !string.IsNullOrWhiteSpace(phoneNumber)
                ? VerificationChannelOptions.Phone
                : VerificationChannelOptions.Email;
        }

        private static string ResolveDestination(string email, string phoneNumber, string verificationChannel)
        {
            return string.Equals(verificationChannel, VerificationChannelOptions.Phone, StringComparison.OrdinalIgnoreCase)
                ? phoneNumber
                : email;
        }

        private static string ResolveRegistrationKey(string email, string phoneNumber)
        {
            return !string.IsNullOrWhiteSpace(email)
                ? $"email:{email}"
                : $"phone:{phoneNumber}";
        }

        private static string ResolveUserName(string email, string phoneNumber)
        {
            return !string.IsNullOrWhiteSpace(email) ? email : phoneNumber;
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

        private static string BuildLockoutMessage(string actionLabel, DateTimeOffset? lockedUntil)
        {
            if (!lockedUntil.HasValue)
            {
                return $"Bạn đã nhập sai mã quá 5 lần. Chức năng {actionLabel} tạm khóa trong 30 phút.";
            }

            return $"Bạn đã nhập sai mã 5 lần. Chức năng {actionLabel} tạm khóa đến {lockedUntil.Value.ToLocalTime():HH:mm dd/MM/yyyy}.";
        }

        private static string MaskDestination(string destination, string verificationChannel)
        {
            return string.Equals(verificationChannel, VerificationChannelOptions.Phone, StringComparison.OrdinalIgnoreCase)
                ? MaskPhone(destination)
                : MaskEmail(destination);
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
    }
}
