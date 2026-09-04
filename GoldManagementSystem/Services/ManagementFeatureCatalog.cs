using System.Collections.Generic;
using System.Linq;

namespace GoldManagementSystem.Services
{
    public static class ManagementFeatureCatalog
    {
        public const string WarehouseSuppliers = "warehouse.suppliers";
        public const string WarehouseReceipts = "warehouse.receipts";
        public const string WarehouseApproval = "warehouse.approval";
        public const string ProductionView = "production.view";
        public const string ProductionOperate = "production.operate";
        public const string ProductionApprove = "production.approve";
        public const string ProductionCustomerJobs = "production.customerjobs";
        public const string PeopleView = "people.view";
        public const string PeopleShifts = "people.shifts";
        public const string PeoplePayroll = "people.payroll";
        public const string ProductsView = "products.view";
        public const string ProductsEdit = "products.edit";
        public const string PriceManage = "pricing.manage";
        public const string PriceApprove = "pricing.approve";
        public const string GoldBarCompliance = "goldbar.compliance";
        public const string AfterSalesManage = "aftersales.manage";
        public const string AfterSalesApprove = "aftersales.approve";
        public const string RevenueView = "revenue.view";
        public const string CustomerCareChat = "customercare.chat";
        public const string CustomerCareFeedback = "customercare.feedback";
        public const string SystemUsers = "system.users";
        public const string SystemPermissions = "system.permissions";
        public const string SystemBranches = "system.branches";
        public const string SystemAudit = "system.audit";

        public static IReadOnlyList<ManagementFeature> BranchFeatures { get; } = new[]
        {
            new ManagementFeature(WarehouseSuppliers, "Quản lí kho", "Nhà cung cấp", "Xem, thêm, sửa, tìm kiếm, kích hoạt và tạm ngưng nhà cung cấp."),
            new ManagementFeature(WarehouseReceipts, "Quản lí kho", "Nhập hàng", "Đơn nhập, phiếu nhận và tồn kho."),
            new ManagementFeature(WarehouseApproval, "Quản lí kho", "Kiểm duyệt", "Quyền kiểm duyệt hàng nhập khi chức năng được phát hành."),
            new ManagementFeature(ProductionView, "Chế tác & gia công", "Theo dõi sản xuất", "Xem lô nguyên liệu, BOM, WIP, lệnh sản xuất, QC và báo cáo truy vết."),
            new ManagementFeature(ProductionOperate, "Chế tác & gia công", "Vận hành xưởng", "Tạo lệnh, giữ/cấp nguyên liệu, ghi công đoạn, hao hụt và thành phẩm."),
            new ManagementFeature(ProductionApprove, "Chế tác & gia công", "Kiểm duyệt sản xuất", "Duyệt BOM, lô nguyên liệu, ngoại lệ hao hụt, QC và giải phóng thành phẩm."),
            new ManagementFeature(ProductionCustomerJobs, "Chế tác & gia công", "Đơn gia công khách", "Tiếp nhận, báo giá, chế tác, QC và bàn giao vật liệu thuộc sở hữu khách."),
            new ManagementFeature(PeopleView, "Nhân sự", "Danh sách nhân sự", "Xem nhân viên trực thuộc chi nhánh."),
            new ManagementFeature(PeopleShifts, "Nhân sự", "Ca làm & điểm danh", "Xếp ca, sửa ca, theo dõi in/out."),
            new ManagementFeature(PeoplePayroll, "Nhân sự", "Lương & ghi chú", "Xem lương, KPI và cập nhật ghi chú."),
            new ManagementFeature(ProductsView, "Sản phẩm", "Xem sản phẩm", "Xem dashboard vàng, bạc, kim cương."),
            new ManagementFeature(ProductsEdit, "Sản phẩm", "Chỉnh sửa sản phẩm", "Thêm, xóa, sửa và đặt ưu tiên."),
            new ManagementFeature(PriceManage, "Giá", "Soạn bảng giá", "Tạo, sửa và gửi bảng giá chờ duyệt."),
            new ManagementFeature(PriceApprove, "Giá", "Duyệt và công bố giá", "Duyệt, công bố và hết hạn bảng giá."),
            new ManagementFeature(GoldBarCompliance, "Vàng miếng", "Tuân thủ vàng miếng", "KYC, serial, giấy phép địa điểm và báo cáo giao dịch vàng miếng."),
            new ManagementFeature(AfterSalesManage, "Hậu mãi", "Xử lý hậu mãi", "Thu mua lại, đổi trả, hoàn tiền, bảo hành và sửa chữa."),
            new ManagementFeature(AfterSalesApprove, "Hậu mãi", "Duyệt hậu mãi", "Duyệt thu mua, đổi trả, hoàn tiền và báo giá sửa chữa."),
            new ManagementFeature(RevenueView, "Doanh thu", "Báo cáo doanh thu", "Xem tổng hợp ngày, tháng và năm."),
            new ManagementFeature(CustomerCareChat, "Chăm sóc khách hàng", "Chat 1-1 hỗ trợ", "Tiếp nhận và hỗ trợ chat trực tiếp 1-1 với khách hàng."),
            new ManagementFeature(CustomerCareFeedback, "Chăm sóc khách hàng", "Đánh giá & Feedback", "Xem và phản hồi đánh giá, góp ý của khách hàng.")
        };

        public static IReadOnlyList<ManagementFeature> SystemFeatures { get; } = new[]
        {
            new ManagementFeature(SystemUsers, "Hệ thống / User", "Người dùng", "Xem, thêm, sửa, khóa và xóa tài khoản."),
            new ManagementFeature(SystemPermissions, "Hệ thống / User", "Phân quyền", "Cấp quyền theo card và chi nhánh."),
            new ManagementFeature(SystemBranches, "Hệ thống", "Chi nhánh", "Tạo và quản lí chi nhánh/kho dùng chung."),
            new ManagementFeature(SystemAudit, "Hệ thống", "Lịch sử", "Xem lịch sử bán hàng và quản trị.")
        };

        public static IReadOnlyList<ManagementFeature> All { get; } =
            BranchFeatures.Concat(SystemFeatures).ToList();

        public static bool IsSystemFeature(string key) => key?.StartsWith("system.") == true;
    }

    public sealed class ManagementFeature
    {
        public ManagementFeature(string key, string group, string name, string description)
        {
            Key = key;
            Group = group;
            Name = name;
            Description = description;
        }

        public string Key { get; }
        public string Group { get; }
        public string Name { get; }
        public string Description { get; }
    }
}
