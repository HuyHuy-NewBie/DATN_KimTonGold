using System;
using System.Collections.Generic;
using System.Linq;

namespace GoldManagementSystem.Services
{
    public static class RoleCatalog
    {
        public const string ManagementRoles = "Admin,Branch Owner,Manager,Warehouse Manager,Sales,Customer Care,Accountant,Staff";
        public const string Admin = "Admin";
        public const string BranchOwner = "Branch Owner";
        public const string Manager = "Manager";
        public const string WarehouseManager = "Warehouse Manager";
        public const string Sales = "Sales";
        public const string CustomerCare = "Customer Care";
        public const string Staff = "Staff";
        public const string Accountant = "Accountant";
        public const string Customer = "Khách hàng";

        private static readonly IReadOnlyDictionary<string, RoleMeta> Metadata =
            new Dictionary<string, RoleMeta>(StringComparer.OrdinalIgnoreCase)
            {
                [Admin] = new("Quản trị viên", "Admin", 600),
                [BranchOwner] = new("Chủ chi nhánh", "Branch owner", 500),
                [Manager] = new("Quản lí", "Manager", 400),
                [WarehouseManager] = new("Quản lí kho", "Warehouse manager", 350),
                [Accountant] = new("Kế toán", "Accountant", 300),
                [Sales] = new("Nhân viên sale", "Sales", 240),
                [CustomerCare] = new("Chăm sóc khách hàng", "Customer care", 220),
                [Staff] = new("Nhân viên", "Staff", 200),
                [Customer] = new("Khách hàng", "Customer", 100)
            };

        public static IReadOnlyList<string> AllOrderedRoles { get; } =
            Metadata
                .OrderByDescending(pair => pair.Value.Priority)
                .Select(pair => pair.Key)
                .ToList();

        public static string GetVietnameseLabel(string role)
        {
            return TryGetMeta(role, out var meta) ? meta.Vietnamese : role;
        }

        public static string GetEnglishLabel(string role)
        {
            return TryGetMeta(role, out var meta) ? meta.English : role;
        }

        public static int GetPriority(string role)
        {
            return TryGetMeta(role, out var meta) ? meta.Priority : 0;
        }

        public static string GetHighestRole(IEnumerable<string> roles)
        {
            return roles?
                .OrderByDescending(GetPriority)
                .FirstOrDefault() ?? Customer;
        }

        public static bool IsPrivilegedPersistentRole(IEnumerable<string> roles)
        {
            return roles?.Any(role =>
                string.Equals(role, Admin, StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, BranchOwner, StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, Manager, StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, WarehouseManager, StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, Sales, StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, CustomerCare, StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, Accountant, StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, Staff, StringComparison.OrdinalIgnoreCase)) == true;
        }

        private static bool TryGetMeta(string role, out RoleMeta meta)
        {
            return Metadata.TryGetValue(role ?? string.Empty, out meta);
        }

        private sealed class RoleMeta
        {
            public RoleMeta(string vietnamese, string english, int priority)
            {
                Vietnamese = vietnamese;
                English = english;
                Priority = priority;
            }

            public string Vietnamese { get; }
            public string English { get; }
            public int Priority { get; }
        }
    }
}
