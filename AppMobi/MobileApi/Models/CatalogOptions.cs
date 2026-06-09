namespace MobileApi.Models;

public static class ProductLineOptions
{
    public const string Gold = "Gold";
    public const string Silver = "Silver";
    public const string Diamond = "Diamond";

    public static readonly string[] All = { Gold, Silver, Diamond };
}

public static class ProductCatalogModeOptions
{
    public const string Single = "Single";
    public const string Multi = "Multi";
}

public static class OrderStatusOptions
{
    public const string PendingApproval = "Chờ phê duyệt";
    public const string Processing = "Đang xử lý";
    public const string Shipping = "Vận chuyển";
    public const string Completed = "Hoàn thành";
    public const string Cancelled = "Đã hủy";

    public static readonly string[] All =
    {
        PendingApproval,
        Processing,
        Shipping,
        Completed,
        Cancelled
    };
}

public static class RoleCatalog
{
    public const string Admin = "Admin";
    public const string BranchOwner = "Branch Owner";
    public const string Manager = "Manager";
    public const string Staff = "Staff";
    public const string Accountant = "Accountant";
    public const string Customer = "Khách hàng";

    private static readonly Dictionary<string, int> Priority = new(StringComparer.OrdinalIgnoreCase)
    {
        [Admin] = 600,
        [BranchOwner] = 500,
        [Manager] = 400,
        [Accountant] = 300,
        [Staff] = 200,
        [Customer] = 100
    };

    public static readonly string[] BackOfficeRoles =
    {
        Admin,
        BranchOwner,
        Manager,
        Accountant,
        Staff
    };

    public static int GetPriority(string role)
    {
        return Priority.TryGetValue(role ?? string.Empty, out var priority) ? priority : 0;
    }

    public static string GetHighestRole(IEnumerable<string> roles)
    {
        return roles.OrderByDescending(GetPriority).FirstOrDefault() ?? Customer;
    }

    public static string GetVietnameseLabel(string role)
    {
        return role switch
        {
            Admin => "Quản trị viên",
            BranchOwner => "Chủ chi nhánh",
            Manager => "Quản lí",
            Accountant => "Kế toán",
            Staff => "Nhân viên",
            Customer => "Khách hàng",
            _ => role
        };
    }
}

public static class Policies
{
    public const string BackOffice = nameof(BackOffice);
    public const string ProductWrite = nameof(ProductWrite);
    public const string OrderRead = nameof(OrderRead);
    public const string OrderManage = nameof(OrderManage);
    public const string ReportsRead = nameof(ReportsRead);
    public const string UsersManage = nameof(UsersManage);
    public const string BranchesManage = nameof(BranchesManage);
}
