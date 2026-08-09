using GoldManagementSystem.Models;
using GoldManagementSystem.Services;

namespace GoldManagementSystem.Models.ViewModels
{
    public sealed class ManagementPortalViewModel
    {
        public string Tab { get; set; } = "overview";
        public string Subtab { get; set; } = string.Empty;
        public string Period { get; set; } = "month";
        public DateTime SelectedDate { get; set; }
        public Branch SelectedBranch { get; set; }
        public IReadOnlyList<Branch> Branches { get; set; } = Array.Empty<Branch>();
        public HashSet<string> GrantedFeatures { get; set; } = new();
        public bool IsAdmin { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public decimal YearRevenue { get; set; }
        public int EmployeeCount { get; set; }
        public IReadOnlyList<ManagementEmployeeItem> Employees { get; set; } = Array.Empty<ManagementEmployeeItem>();
        public IReadOnlyList<ManagementEmployeeItem> AvailableEmployees { get; set; } = Array.Empty<ManagementEmployeeItem>();
        public IReadOnlyList<ManagementShiftItem> Shifts { get; set; } = Array.Empty<ManagementShiftItem>();
        public IReadOnlyList<PayrollItem> Payroll { get; set; } = Array.Empty<PayrollItem>();
        public IReadOnlyList<Product> Products { get; set; } = Array.Empty<Product>();
        public IReadOnlyList<Supplier> Suppliers { get; set; } = Array.Empty<Supplier>();
        public IReadOnlyList<SupplierPurchaseOrder> PurchaseOrders { get; set; } = Array.Empty<SupplierPurchaseOrder>();
        public IReadOnlyList<SupplierGoodsReceipt> GoodsReceipts { get; set; } = Array.Empty<SupplierGoodsReceipt>();
        public IReadOnlyList<Warehouse> Warehouses { get; set; } = Array.Empty<Warehouse>();
        public IReadOnlyList<InventoryItem> InventoryItems { get; set; } = Array.Empty<InventoryItem>();
        public IReadOnlyList<RevenuePoint> RevenuePoints { get; set; } = Array.Empty<RevenuePoint>();
        public IReadOnlyList<ManagementUserItem> Users { get; set; } = Array.Empty<ManagementUserItem>();
        public string PermissionUserId { get; set; }
        public int? PermissionBranchId { get; set; }
        public HashSet<string> PermissionSelection { get; set; } = new();
        public IReadOnlyList<ManagementFeature> BranchFeatureCards { get; set; } = ManagementFeatureCatalog.BranchFeatures;
        public IReadOnlyList<ManagementFeature> SystemFeatureCards { get; set; } = ManagementFeatureCatalog.SystemFeatures;
        public IReadOnlyList<ManagementAuditLog> SalesAuditLogs { get; set; } = Array.Empty<ManagementAuditLog>();
        public IReadOnlyList<ManagementAuditLog> ManagementAuditLogs { get; set; } = Array.Empty<ManagementAuditLog>();
        public IReadOnlyList<ManagementSelectOption> BranchManagerOptions { get; set; } = Array.Empty<ManagementSelectOption>();
        public IReadOnlyList<ManagementSelectOption> WarehouseOptions { get; set; } = Array.Empty<ManagementSelectOption>();
        public bool PermissionUpdated { get; set; }

        public bool Can(string feature) => IsAdmin || GrantedFeatures.Contains(feature);
    }

    public sealed class ManagementSelectOption
    {
        public string Value { get; set; }
        public string Label { get; set; }
    }

    public sealed class ManagementEmployeeItem
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public string SystemNote { get; set; }
        public string ManagerNote { get; set; }
    }

    public sealed class ManagementShiftItem
    {
        public int ShiftId { get; set; }
        public string ShiftType { get; set; }
        public string ShiftLabel { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }
        public bool IsLockedWindow { get; set; }
        public IReadOnlyList<ShiftAssignment> Assignments { get; set; } = Array.Empty<ShiftAssignment>();
        public IReadOnlyList<ShiftChangeLog> SupplementalChanges { get; set; } = Array.Empty<ShiftChangeLog>();
    }

    public sealed class PayrollItem
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal ResponsibilityBonus { get; set; }
        public decimal KpiBonus { get; set; }
        public decimal AttendanceBonus { get; set; }
        public decimal TotalSalary { get; set; }
        public bool HasAttendanceViolation { get; set; }
    }

    public sealed class RevenuePoint
    {
        public string Label { get; set; }
        public decimal Amount { get; set; }
        public int OrderCount { get; set; }
    }

    public sealed class ManagementUserItem
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public int? BranchId { get; set; }
        public string BranchName { get; set; }
    }
}
