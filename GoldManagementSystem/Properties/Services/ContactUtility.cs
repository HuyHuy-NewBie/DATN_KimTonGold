using System;
using System.Linq;

namespace GoldManagementSystem.Services
{
    public static class ContactUtility
    {
        public static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        public static string NormalizeEmail(string email)
        {
            var normalized = Normalize(email);
            return normalized?.ToLowerInvariant();
        }

        public static string NormalizePhone(string phoneNumber)
        {
            var normalized = Normalize(phoneNumber);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return null;
            }

            var hasLeadingPlus = normalized.StartsWith("+", StringComparison.Ordinal);
            var digits = new string(normalized.Where(char.IsDigit).ToArray());
            if (string.IsNullOrWhiteSpace(digits))
            {
                return null;
            }

            return hasLeadingPlus ? $"+{digits}" : digits;
        }

        public static string NormalizeIdentifier(string identifier)
        {
            var normalized = Normalize(identifier);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return null;
            }

            return LooksLikeEmail(normalized)
                ? NormalizeEmail(normalized)
                : NormalizePhone(normalized);
        }

        public static bool LooksLikeEmail(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.Contains('@');
        }
    }
}
