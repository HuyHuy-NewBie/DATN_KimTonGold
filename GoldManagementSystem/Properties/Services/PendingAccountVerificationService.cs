using Microsoft.Extensions.Caching.Memory;
using System;
using System.Security.Cryptography;

namespace GoldManagementSystem.Services
{
    public class PendingAccountVerificationService
    {
        private static readonly TimeSpan VerificationLifetime = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(30);
        private const int MaxFailedAttempts = 5;
        private readonly IMemoryCache _memoryCache;

        public PendingAccountVerificationService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public PendingRegistrationVerification CreateRegistrationVerification(
            string registrationKey,
            string fullName,
            string email,
            string phoneNumber,
            string password,
            string verificationChannel)
        {
            var verification = new PendingRegistrationVerification
            {
                FlowId = Guid.NewGuid().ToString("N"),
                RegistrationKey = registrationKey,
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                Password = password,
                VerificationChannel = verificationChannel,
                VerificationCode = GenerateVerificationCode(),
                ExpiresAt = DateTimeOffset.UtcNow.Add(VerificationLifetime)
            };

            _memoryCache.Set(
                BuildRegisterCacheKey(verification.FlowId),
                verification,
                verification.ExpiresAt);

            return verification;
        }

        public PendingLoginVerification CreateLoginVerification(
            string userId,
            bool rememberMe,
            string verificationChannel,
            string destination)
        {
            var verification = new PendingLoginVerification
            {
                FlowId = Guid.NewGuid().ToString("N"),
                UserId = userId,
                RememberMe = rememberMe,
                VerificationChannel = verificationChannel,
                Destination = destination,
                VerificationCode = GenerateVerificationCode(),
                ExpiresAt = DateTimeOffset.UtcNow.Add(VerificationLifetime)
            };

            _memoryCache.Set(
                BuildLoginCacheKey(verification.FlowId),
                verification,
                verification.ExpiresAt);

            return verification;
        }

        public PendingRegistrationVerification RefreshRegistrationVerification(PendingRegistrationVerification verification)
        {
            if (verification == null)
            {
                return null;
            }

            verification.VerificationCode = GenerateVerificationCode();
            verification.ExpiresAt = DateTimeOffset.UtcNow.Add(VerificationLifetime);

            _memoryCache.Set(
                BuildRegisterCacheKey(verification.FlowId),
                verification,
                verification.ExpiresAt);

            return verification;
        }

        public PendingLoginVerification RefreshLoginVerification(PendingLoginVerification verification)
        {
            if (verification == null)
            {
                return null;
            }

            verification.VerificationCode = GenerateVerificationCode();
            verification.ExpiresAt = DateTimeOffset.UtcNow.Add(VerificationLifetime);

            _memoryCache.Set(
                BuildLoginCacheKey(verification.FlowId),
                verification,
                verification.ExpiresAt);

            return verification;
        }

        public PendingRegistrationVerification GetRegistrationVerification(string flowId)
        {
            if (string.IsNullOrWhiteSpace(flowId))
            {
                return null;
            }

            _memoryCache.TryGetValue(BuildRegisterCacheKey(flowId), out PendingRegistrationVerification verification);
            return verification;
        }

        public PendingLoginVerification GetLoginVerification(string flowId)
        {
            if (string.IsNullOrWhiteSpace(flowId))
            {
                return null;
            }

            _memoryCache.TryGetValue(BuildLoginCacheKey(flowId), out PendingLoginVerification verification);
            return verification;
        }

        public bool TryGetRegistrationLockout(string registrationKey, out DateTimeOffset lockedUntil)
        {
            return _memoryCache.TryGetValue(BuildRegisterLockKey(registrationKey), out lockedUntil)
                && lockedUntil > DateTimeOffset.UtcNow;
        }

        public bool TryGetLoginLockout(string userId, out DateTimeOffset lockedUntil)
        {
            return _memoryCache.TryGetValue(BuildLoginLockKey(userId), out lockedUntil)
                && lockedUntil > DateTimeOffset.UtcNow;
        }

        public void RemoveRegistrationVerification(string flowId)
        {
            if (!string.IsNullOrWhiteSpace(flowId))
            {
                _memoryCache.Remove(BuildRegisterCacheKey(flowId));
            }
        }

        public void RemoveLoginVerification(string flowId)
        {
            if (!string.IsNullOrWhiteSpace(flowId))
            {
                _memoryCache.Remove(BuildLoginCacheKey(flowId));
            }
        }

        public bool IsCodeValid(string expectedCode, string actualCode)
        {
            return !string.IsNullOrWhiteSpace(expectedCode)
                && !string.IsNullOrWhiteSpace(actualCode)
                && string.Equals(expectedCode, actualCode.Trim(), StringComparison.Ordinal);
        }

        public VerificationFailureResult RegisterLoginFailure(PendingLoginVerification verification)
        {
            verification.FailedAttempts++;

            if (verification.FailedAttempts >= MaxFailedAttempts)
            {
                var lockedUntil = DateTimeOffset.UtcNow.Add(LockoutDuration);
                _memoryCache.Set(BuildLoginLockKey(verification.UserId), lockedUntil, lockedUntil);
                RemoveLoginVerification(verification.FlowId);
                return new VerificationFailureResult
                {
                    IsLockedOut = true,
                    FailedAttempts = verification.FailedAttempts,
                    LockedUntil = lockedUntil,
                    RemainingAttempts = 0
                };
            }

            _memoryCache.Set(BuildLoginCacheKey(verification.FlowId), verification, verification.ExpiresAt);
            return new VerificationFailureResult
            {
                FailedAttempts = verification.FailedAttempts,
                RemainingAttempts = MaxFailedAttempts - verification.FailedAttempts
            };
        }

        public VerificationFailureResult RegisterRegistrationFailure(PendingRegistrationVerification verification)
        {
            verification.FailedAttempts++;

            if (verification.FailedAttempts >= MaxFailedAttempts)
            {
                var lockedUntil = DateTimeOffset.UtcNow.Add(LockoutDuration);
                _memoryCache.Set(BuildRegisterLockKey(verification.RegistrationKey), lockedUntil, lockedUntil);
                RemoveRegistrationVerification(verification.FlowId);
                return new VerificationFailureResult
                {
                    IsLockedOut = true,
                    FailedAttempts = verification.FailedAttempts,
                    LockedUntil = lockedUntil,
                    RemainingAttempts = 0
                };
            }

            _memoryCache.Set(BuildRegisterCacheKey(verification.FlowId), verification, verification.ExpiresAt);
            return new VerificationFailureResult
            {
                FailedAttempts = verification.FailedAttempts,
                RemainingAttempts = MaxFailedAttempts - verification.FailedAttempts
            };
        }

        private static string GenerateVerificationCode()
        {
            return RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        }

        private static string BuildRegisterCacheKey(string flowId) => $"account-register:{flowId}";
        private static string BuildLoginCacheKey(string flowId) => $"account-login:{flowId}";
        private static string BuildRegisterLockKey(string registrationKey) => $"account-register-lock:{registrationKey?.Trim().ToLowerInvariant()}";
        private static string BuildLoginLockKey(string userId) => $"account-login-lock:{userId}";
    }

    public class PendingRegistrationVerification
    {
        public string FlowId { get; set; }
        public string RegistrationKey { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string VerificationChannel { get; set; }
        public string VerificationCode { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public int FailedAttempts { get; set; }
    }

    public class PendingLoginVerification
    {
        public string FlowId { get; set; }
        public string UserId { get; set; }
        public bool RememberMe { get; set; }
        public string VerificationChannel { get; set; }
        public string Destination { get; set; }
        public string VerificationCode { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public int FailedAttempts { get; set; }
    }

    public class VerificationFailureResult
    {
        public bool IsLockedOut { get; set; }
        public int FailedAttempts { get; set; }
        public int RemainingAttempts { get; set; }
        public DateTimeOffset? LockedUntil { get; set; }
    }
}
