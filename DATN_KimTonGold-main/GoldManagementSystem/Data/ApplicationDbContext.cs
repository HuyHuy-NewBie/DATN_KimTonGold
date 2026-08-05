using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GoldManagementSystem.Models;

namespace GoldManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Branch> Branches { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<GoldProductCatalogEntry> GoldProductCatalogEntries { get; set; }
        public DbSet<SilverProductCatalogEntry> SilverProductCatalogEntries { get; set; }
        public DbSet<DiamondProductCatalogEntry> DiamondProductCatalogEntries { get; set; }
        public DbSet<GoldSilverProductCatalogEntry> GoldSilverProductCatalogEntries { get; set; }
        public DbSet<GoldDiamondProductCatalogEntry> GoldDiamondProductCatalogEntries { get; set; }
        public DbSet<SilverDiamondProductCatalogEntry> SilverDiamondProductCatalogEntries { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<FavoriteProduct> FavoriteProducts { get; set; }
        public DbSet<MarketHistory> MarketHistories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SupplierPurchaseOrder> SupplierPurchaseOrders { get; set; }
        public DbSet<SupplierPurchaseOrderDetail> SupplierPurchaseOrderDetails { get; set; }
        public DbSet<SupplierGoodsReceipt> SupplierGoodsReceipts { get; set; }
        public DbSet<SupplierGoodsReceiptDetail> SupplierGoodsReceiptDetails { get; set; }
        public DbSet<SupplierPayment> SupplierPayments { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<InventoryIssue> InventoryIssues { get; set; }
        public DbSet<InventoryIssueDetail> InventoryIssueDetails { get; set; }
        public DbSet<ChatSettings> ChatSettings { get; set; }
        public DbSet<WorkShift> WorkShifts { get; set; }
        public DbSet<ShiftAssignment> ShiftAssignments { get; set; }
        public DbSet<ShiftChangeLog> ShiftChangeLogs { get; set; }
        public DbSet<UserFeaturePermission> UserFeaturePermissions { get; set; }
        public DbSet<EmployeeManagementNote> EmployeeManagementNotes { get; set; }
        public DbSet<SystemNotification> SystemNotifications { get; set; }
        public DbSet<ManagementAuditLog> ManagementAuditLogs { get; set; }
        public DbSet<BranchWarehouseAccess> BranchWarehouseAccesses { get; set; }
        public DbSet<SupportChatSession> SupportChatSessions { get; set; }
        public DbSet<SupportChatMessage> SupportChatMessages { get; set; }
        public DbSet<CustomerFeedback> CustomerFeedbacks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seed 1 row mặc định cho ChatSettings (singleton pattern)
            builder.Entity<ChatSettings>().HasData(new ChatSettings
            {
                Id = 1,
                ShopName = "KimTon Gold",
                Hotline = "1800 9999",
                ShopAddress = "123 Đường Vàng Kim, Quận 1, TP. HCM",
                ProductPriceInfo = string.Empty,
                SizeGuideInfo = string.Empty,
                WarrantyInfo = string.Empty,
                ExchangePolicy = string.Empty,
                OrderProcess = string.Empty,
                UpdatedAt = new System.DateTime(2025, 1, 1, 0, 0, 0, System.DateTimeKind.Utc),
                UpdatedBy = "system"
            }); 
            //2//

 
            // Cấu hình Fluent API cho Code First nếu cần thiết
            builder.Entity<Order>()
                .HasOne(o => o.Branch)
                .WithMany(b => b.Orders)
                .HasForeignKey(o => o.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Product>()
                .HasOne(p => p.Branch)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<GoldProductCatalogEntry>()
                .HasOne(item => item.Product)
                .WithOne(product => product.GoldCatalogEntry)
                .HasForeignKey<GoldProductCatalogEntry>(item => item.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SilverProductCatalogEntry>()
                .HasOne(item => item.Product)
                .WithOne(product => product.SilverCatalogEntry)
                .HasForeignKey<SilverProductCatalogEntry>(item => item.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DiamondProductCatalogEntry>()
                .HasOne(item => item.Product)
                .WithOne(product => product.DiamondCatalogEntry)
                .HasForeignKey<DiamondProductCatalogEntry>(item => item.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<GoldSilverProductCatalogEntry>()
                .HasOne(item => item.Product)
                .WithOne(product => product.GoldSilverCatalogEntry)
                .HasForeignKey<GoldSilverProductCatalogEntry>(item => item.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<GoldDiamondProductCatalogEntry>()
                .HasOne(item => item.Product)
                .WithOne(product => product.GoldDiamondCatalogEntry)
                .HasForeignKey<GoldDiamondProductCatalogEntry>(item => item.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SilverDiamondProductCatalogEntry>()
                .HasOne(item => item.Product)
                .WithOne(product => product.SilverDiamondCatalogEntry)
                .HasForeignKey<SilverDiamondProductCatalogEntry>(item => item.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FavoriteProduct>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FavoriteProduct>()
                .HasOne(f => f.Product)
                .WithMany()
                .HasForeignKey(f => f.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<SupplierPurchaseOrder>()
            .HasOne(order => order.Supplier)
            .WithMany(supplier => supplier.PurchaseOrders)
            .HasForeignKey(order => order.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SupplierPurchaseOrder>()
                .HasOne(order => order.Branch)
                .WithMany()
                .HasForeignKey(order => order.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SupplierPurchaseOrder>()
                .HasOne(order => order.CreatedByUser)
                .WithMany()
                .HasForeignKey(order => order.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SupplierPurchaseOrderDetail>()
                .HasOne(detail => detail.SupplierPurchaseOrder)
                .WithMany(order => order.Details)
                .HasForeignKey(detail => detail.SupplierPurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SupplierGoodsReceipt>()
                .HasOne(receipt => receipt.SupplierPurchaseOrder)
                .WithMany(order => order.Receipts)
                .HasForeignKey(receipt => receipt.SupplierPurchaseOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SupplierGoodsReceipt>()
                .HasOne(receipt => receipt.CreatedByUser)
                .WithMany()
                .HasForeignKey(receipt => receipt.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SupplierGoodsReceipt>()
                .HasOne(receipt => receipt.Warehouse)
                .WithMany()
                .HasForeignKey(receipt => receipt.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SupplierGoodsReceipt>()
                .HasIndex(receipt => receipt.ReceiptCode)
                .IsUnique();
                
            builder.Entity<SupplierGoodsReceiptDetail>()
                .HasOne(detail => detail.SupplierGoodsReceipt)
                .WithMany(receipt => receipt.Details)
                .HasForeignKey(detail => detail.SupplierGoodsReceiptId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SupplierGoodsReceiptDetail>()
                .HasOne(detail => detail.SupplierPurchaseOrderDetail)
                .WithMany()
                .HasForeignKey(detail => detail.SupplierPurchaseOrderDetailId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SupplierPayment>()
                .HasOne(payment => payment.Supplier)
                .WithMany(supplier => supplier.Payments)
                .HasForeignKey(payment => payment.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SupplierPayment>()
                .HasOne(payment => payment.SupplierPurchaseOrder)
                .WithMany(order => order.Payments)
                .HasForeignKey(payment => payment.SupplierPurchaseOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SupplierPayment>()
                .HasOne(payment => payment.CreatedByUser)
                .WithMany()
                .HasForeignKey(payment => payment.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<Warehouse>()
                .HasIndex(warehouse => warehouse.Code)
                .IsUnique();

            builder.Entity<Warehouse>()
                .HasOne(warehouse => warehouse.Branch)
                .WithMany(branch => branch.Warehouses)
                .HasForeignKey(warehouse => warehouse.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Warehouse>()
                .Property(warehouse => warehouse.LocationType)
                .HasDefaultValue(
                    Warehouse.LocationTypeStorage);
            builder.Entity<InventoryItem>()
                .HasIndex(item => item.StockCode)
                .IsUnique();

            builder.Entity<InventoryItem>()
                .HasOne(item => item.Warehouse)
                .WithMany(warehouse => warehouse.InventoryItems)
                .HasForeignKey(item => item.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InventoryItem>()
                .HasOne(item => item.Supplier)
                .WithMany()
                .HasForeignKey(item => item.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InventoryItem>()
                .HasOne(item => item.SupplierPurchaseOrder)
                .WithMany()
                .HasForeignKey(item => item.SupplierPurchaseOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InventoryItem>()
                .HasOne(item => item.SupplierGoodsReceiptDetail)
                .WithMany()
                .HasForeignKey(item => item.SupplierGoodsReceiptDetailId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InventoryTransaction>()
                .HasIndex(transaction => transaction.TransactionCode)
                .IsUnique();

            builder.Entity<InventoryTransaction>()
                .HasOne(transaction => transaction.Warehouse)
                .WithMany(warehouse => warehouse.Transactions)
                .HasForeignKey(transaction => transaction.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InventoryTransaction>()
                .HasOne(transaction => transaction.InventoryItem)
                .WithMany()
                .HasForeignKey(transaction => transaction.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InventoryTransaction>()
                .HasOne(transaction => transaction.CreatedByUser)
                .WithMany()
                .HasForeignKey(transaction => transaction.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            /*
            * =========================================
            * PHIẾU XUẤT KHO
            * =========================================
            */

            /*
            * Mã phiếu xuất không được trùng.
            */
            builder.Entity<InventoryIssue>()
                .HasIndex(issue => issue.IssueCode)
                .IsUnique();

            /*
            * Chi nhánh thực hiện xuất kho.
            */
            builder.Entity<InventoryIssue>()
                .HasOne(issue => issue.Branch)
                .WithMany()
                .HasForeignKey(issue => issue.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
            /*
            * Kho bị trừ hàng.
            */
            builder.Entity<InventoryIssue>()
                .HasOne(issue => issue.Warehouse)
                .WithMany()
                .HasForeignKey(issue => issue.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InventoryIssue>()
                .HasOne(issue => issue.ReceiverUser)
                .WithMany()
                .HasForeignKey(issue => issue.ReceiverUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InventoryIssue>()
                .HasOne(issue => issue.DestinationWarehouse)
                .WithMany()
                .HasForeignKey(issue =>
                    issue.DestinationWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
            /*
            * Nhà cung cấp nhận lại hàng.
            * SupplierId chỉ có giá trị đối với phiếu
            * Trả nhà cung cấp.
            */
            builder.Entity<InventoryIssue>()
                .HasOne(issue => issue.Supplier)
                .WithMany()
                .HasForeignKey(issue => issue.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            /*
            * Người lập phiếu.
            */
            builder.Entity<InventoryIssue>()
                .HasOne(issue => issue.CreatedByUser)
                .WithMany()
                .HasForeignKey(issue => issue.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            /*
            * Người xác nhận xuất kho.
            * Khi phiếu còn Chờ xuất kho thì
            * ConfirmedByUserId bằng null.
            */
            builder.Entity<InventoryIssue>()
                .HasOne(issue => issue.ConfirmedByUser)
                .WithMany()
                .HasForeignKey(issue => issue.ConfirmedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            /*
            * Một mã tồn không được xuất lặp lại
            * nhiều lần trong cùng một phiếu.
            */
            builder.Entity<InventoryIssueDetail>()
                .HasIndex(detail => new
                {
                    detail.InventoryIssueId,
                    detail.InventoryItemId
                })
                .IsUnique();

            /*
            * Khi xóa phiếu chưa phát sinh nghiệp vụ,
            * các chi tiết thuộc phiếu cũng được xóa.
            *
            * Sau này controller sẽ không cho phép
            * xóa trực tiếp phiếu đã xuất kho.
            */
            builder.Entity<InventoryIssueDetail>()
                .HasOne(detail => detail.InventoryIssue)
                .WithMany(issue => issue.Details)
                .HasForeignKey(detail => detail.InventoryIssueId)
                .OnDelete(DeleteBehavior.Cascade);

            /*
            * Không cho xóa mã tồn nếu mã tồn đã từng
            * xuất hiện trong phiếu xuất kho.
            */
            builder.Entity<InventoryIssueDetail>()
                .HasOne(detail => detail.InventoryItem)
                .WithMany()
                .HasForeignKey(detail => detail.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<UserFeaturePermission>()
                .HasIndex(permission => new { permission.UserId, permission.FeatureKey, permission.BranchId })
                .IsUnique();

            builder.Entity<UserFeaturePermission>()
                .HasOne(permission => permission.Branch)
                .WithMany()
                .HasForeignKey(permission => permission.BranchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<WorkShift>()
                .HasIndex(shift => new { shift.BranchId, shift.ShiftDate, shift.ShiftType })
                .IsUnique();

            builder.Entity<ShiftAssignment>()
                .HasIndex(assignment => new { assignment.WorkShiftId, assignment.UserId })
                .IsUnique();

            builder.Entity<ShiftChangeLog>()
                .HasOne<WorkShift>()
                .WithMany()
                .HasForeignKey(log => log.WorkShiftId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<EmployeeManagementNote>()
                .HasIndex(note => new { note.UserId, note.BranchId })
                .IsUnique();

            builder.Entity<EmployeeManagementNote>()
                .HasOne(note => note.User)
                .WithMany()
                .HasForeignKey(note => note.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<EmployeeManagementNote>()
                .HasOne(note => note.Branch)
                .WithMany()
                .HasForeignKey(note => note.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SystemNotification>()
                .HasOne(notification => notification.User)
                .WithMany()
                .HasForeignKey(notification => notification.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SystemNotification>()
                .HasIndex(notification => new { notification.UserId, notification.IsRead, notification.CreatedAt });

            builder.Entity<ManagementAuditLog>()
                .HasIndex(log => new { log.Area, log.CreatedAt });

            builder.Entity<BranchWarehouseAccess>()
                .HasIndex(access => new { access.BranchId, access.WarehouseId })
                .IsUnique();

            builder.Entity<BranchWarehouseAccess>()
                .HasOne(access => access.Branch)
                .WithMany(branch => branch.WarehouseAccesses)
                .HasForeignKey(access => access.BranchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<BranchWarehouseAccess>()
                .HasOne(access => access.Warehouse)
                .WithMany(warehouse => warehouse.BranchAccesses)
                .HasForeignKey(access => access.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
