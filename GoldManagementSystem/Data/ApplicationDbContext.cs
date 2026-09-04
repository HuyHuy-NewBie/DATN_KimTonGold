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
        public DbSet<PurityDefinition> PurityDefinitions { get; set; }
        public DbSet<ProductSpecVersion> ProductSpecVersions { get; set; }
        public DbSet<PriceBook> PriceBooks { get; set; }
        public DbSet<PriceVersion> PriceVersions { get; set; }
        public DbSet<PriceLine> PriceLines { get; set; }
        public DbSet<PriceSnapshot> PriceSnapshots { get; set; }
        public DbSet<BusinessLocation> BusinessLocations { get; set; }
        public DbSet<BusinessLicense> BusinessLicenses { get; set; }
        public DbSet<CustomerKycProfile> CustomerKycProfiles { get; set; }
        public DbSet<GoldBarSerial> GoldBarSerials { get; set; }
        public DbSet<GoldBarSaleRecord> GoldBarSaleRecords { get; set; }
        public DbSet<PosQuote> PosQuotes { get; set; }
        public DbSet<PosQuoteLine> PosQuoteLines { get; set; }
        public DbSet<DiscountApproval> DiscountApprovals { get; set; }
        public DbSet<PosInventoryReservation> PosInventoryReservations { get; set; }
        public DbSet<OrderDelivery> OrderDeliveries { get; set; }
        public DbSet<DeliveryEvidence> DeliveryEvidences { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentAllocation> PaymentAllocations { get; set; }
        public DbSet<BankReconciliation> BankReconciliations { get; set; }
        public DbSet<CashFundEntry> CashFundEntries { get; set; }
        public DbSet<EInvoice> EInvoices { get; set; }
        public DbSet<BuybackCase> BuybackCases { get; set; }
        public DbSet<BuybackAssay> BuybackAssays { get; set; }
        public DbSet<ReturnCase> ReturnCases { get; set; }
        public DbSet<Refund> Refunds { get; set; }
        public DbSet<WarrantyCase> WarrantyCases { get; set; }
        public DbSet<RepairCase> RepairCases { get; set; }
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
        public DbSet<InventoryStocktake> InventoryStocktakes { get; set; }
        public DbSet<InventoryStocktakeDetail> InventoryStocktakeDetails { get; set; }
        public DbSet<ProductionWorkshop> ProductionWorkshops { get; set; }
        public DbSet<ProductionLossPolicy> ProductionLossPolicies { get; set; }
        public DbSet<RawMaterialLot> RawMaterialLots { get; set; }
        public DbSet<ProductionBom> ProductionBoms { get; set; }
        public DbSet<ProductionBomItem> ProductionBomItems { get; set; }
        public DbSet<ProductionBomOperation> ProductionBomOperations { get; set; }
        public DbSet<ProductionWorkOrder> ProductionWorkOrders { get; set; }
        public DbSet<ProductionMaterialReservation> ProductionMaterialReservations { get; set; }
        public DbSet<ProductionOperationLog> ProductionOperationLogs { get; set; }
        public DbSet<ProductionLossRecord> ProductionLossRecords { get; set; }
        public DbSet<ProductionQualityInspection> ProductionQualityInspections { get; set; }
        public DbSet<ProductionReceipt> ProductionReceipts { get; set; }
        public DbSet<ProductionRecycleBatch> ProductionRecycleBatches { get; set; }
        public DbSet<CustomerJobOrder> CustomerJobOrders { get; set; }
        public DbSet<CustomerMaterialCustodyRecord> CustomerMaterialCustodyRecords { get; set; }
        public DbSet<ProductionStatusHistory> ProductionStatusHistories { get; set; }
        public DbSet<ProductionAuditLog> ProductionAuditLogs { get; set; }
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

            // A quote may create at most one order. The SQL Server filter permits
            // ordinary orders that do not originate from POS to remain nullable.
            builder.Entity<Order>()
                .HasIndex(order => order.PosQuoteId)
                .IsUnique()
                .HasFilter("[PosQuoteId] IS NOT NULL");

            builder.Entity<Product>()
                .HasOne(p => p.Branch)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Product>()
                .HasOne(product => product.PurityDefinition)
                .WithMany(purity => purity.Products)
                .HasForeignKey(product => product.PurityDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductSpecVersion>()
                .HasOne(version => version.Product)
                .WithMany(product => product.SpecificationVersions)
                .HasForeignKey(version => version.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductSpecVersion>()
                .HasOne(version => version.PurityDefinition)
                .WithMany()
                .HasForeignKey(version => version.PurityDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductSpecVersion>()
                .HasOne(version => version.CreatedByUser)
                .WithMany()
                .HasForeignKey(version => version.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PriceBook>().HasOne(book => book.Branch).WithMany().HasForeignKey(book => book.BranchId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<PriceBook>().HasOne(book => book.CreatedByUser).WithMany().HasForeignKey(book => book.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<PriceBook>().HasOne(book => book.SubmittedByUser).WithMany().HasForeignKey(book => book.SubmittedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<PriceBook>().HasOne(book => book.ApprovedByUser).WithMany().HasForeignKey(book => book.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<PriceVersion>().HasOne(version => version.PriceBook).WithMany(book => book.Versions).HasForeignKey(version => version.PriceBookId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<PriceVersion>().HasOne(version => version.CreatedByUser).WithMany().HasForeignKey(version => version.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<PriceLine>().HasOne(line => line.PriceVersion).WithMany(version => version.Lines).HasForeignKey(line => line.PriceVersionId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<PriceLine>().HasOne(line => line.Product).WithMany().HasForeignKey(line => line.ProductId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<PriceSnapshot>().HasOne(snapshot => snapshot.Order).WithMany().HasForeignKey(snapshot => snapshot.OrderId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<PriceSnapshot>().HasOne(snapshot => snapshot.Product).WithMany().HasForeignKey(snapshot => snapshot.ProductId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<PriceSnapshot>().HasOne(snapshot => snapshot.CapturedByUser).WithMany().HasForeignKey(snapshot => snapshot.CapturedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<OrderDetail>().HasOne(detail => detail.PriceSnapshot).WithMany().HasForeignKey(detail => detail.PriceSnapshotId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<BusinessLocation>().HasOne(location => location.Branch).WithMany().HasForeignKey(location => location.BranchId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<BusinessLicense>().HasOne(license => license.BusinessLocation).WithMany(location => location.Licenses).HasForeignKey(license => license.BusinessLocationId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<BusinessLicense>().HasOne(license => license.VerifiedByUser).WithMany().HasForeignKey(license => license.VerifiedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<CustomerKycProfile>().HasOne(profile => profile.Branch).WithMany().HasForeignKey(profile => profile.BranchId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<CustomerKycProfile>().HasOne(profile => profile.VerifiedByUser).WithMany().HasForeignKey(profile => profile.VerifiedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<CustomerKycProfile>().HasOne(profile => profile.CreatedByUser).WithMany().HasForeignKey(profile => profile.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<CustomerKycProfile>().Property(profile => profile.RowVersion).IsRowVersion();
            builder.Entity<GoldBarSerial>().HasOne(serial => serial.Product).WithMany().HasForeignKey(serial => serial.ProductId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<GoldBarSerial>().HasOne(serial => serial.BusinessLocation).WithMany(location => location.GoldBarSerials).HasForeignKey(serial => serial.BusinessLocationId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<GoldBarSerial>().Property(serial => serial.RowVersion).IsRowVersion();
            builder.Entity<GoldBarSaleRecord>().HasOne(record => record.Order).WithMany().HasForeignKey(record => record.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<GoldBarSaleRecord>().HasOne(record => record.OrderDetail).WithMany().HasForeignKey(record => record.OrderDetailId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<GoldBarSaleRecord>().HasOne(record => record.GoldBarSerial).WithMany().HasForeignKey(record => record.GoldBarSerialId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<GoldBarSaleRecord>().HasOne(record => record.CustomerKycProfile).WithMany().HasForeignKey(record => record.CustomerKycProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<GoldBarSaleRecord>().HasOne(record => record.BusinessLocation).WithMany().HasForeignKey(record => record.BusinessLocationId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<GoldBarSaleRecord>().HasOne(record => record.PriceSnapshot).WithMany().HasForeignKey(record => record.PriceSnapshotId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<GoldBarSaleRecord>().HasOne(record => record.CreatedByUser).WithMany().HasForeignKey(record => record.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<PosQuote>().HasOne(quote => quote.Branch).WithMany().HasForeignKey(quote => quote.BranchId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<PosQuote>().HasOne(quote => quote.CreatedByUser).WithMany().HasForeignKey(quote => quote.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<PosQuoteLine>().HasOne(line => line.PosQuote).WithMany(quote => quote.Lines).HasForeignKey(line => line.PosQuoteId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<PosQuoteLine>().HasOne(line => line.Product).WithMany().HasForeignKey(line => line.ProductId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<PosQuoteLine>().HasOne(line => line.PriceSnapshot).WithMany().HasForeignKey(line => line.PriceSnapshotId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<DiscountApproval>().HasOne(item => item.PosQuote).WithMany(quote => quote.DiscountApprovals).HasForeignKey(item => item.PosQuoteId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<DiscountApproval>().HasOne(item => item.Order).WithMany().HasForeignKey(item => item.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<DiscountApproval>().HasOne(item => item.RequestedByUser).WithMany().HasForeignKey(item => item.RequestedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<DiscountApproval>().HasOne(item => item.ApprovedByUser).WithMany().HasForeignKey(item => item.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<PosInventoryReservation>().HasOne(item => item.Order).WithMany(order => order.PosInventoryReservations).HasForeignKey(item => item.OrderId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<PosInventoryReservation>().HasOne(item => item.InventoryItem).WithMany().HasForeignKey(item => item.InventoryItemId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<PosInventoryReservation>().HasOne(item => item.CreatedByUser).WithMany().HasForeignKey(item => item.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<OrderDelivery>().HasOne(item => item.Order).WithOne(order => order.OrderDelivery).HasForeignKey<OrderDelivery>(item => item.OrderId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<DeliveryEvidence>().HasOne(item => item.OrderDelivery).WithMany(delivery => delivery.Evidences).HasForeignKey(item => item.OrderDeliveryId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<DeliveryEvidence>().HasOne(item => item.UploadedByUser).WithMany().HasForeignKey(item => item.UploadedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<InventoryIssue>().HasOne(issue => issue.Order).WithMany().HasForeignKey(issue => issue.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Payment>().HasOne(item => item.Branch).WithMany().HasForeignKey(item => item.BranchId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Payment>().HasOne(item => item.CreatedByUser).WithMany().HasForeignKey(item => item.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Payment>().HasOne(item => item.ConfirmedByUser).WithMany().HasForeignKey(item => item.ConfirmedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<PaymentAllocation>().HasOne(item => item.Payment).WithMany(payment => payment.Allocations).HasForeignKey(item => item.PaymentId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<PaymentAllocation>().HasOne(item => item.Order).WithMany().HasForeignKey(item => item.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<BankReconciliation>().HasOne(item => item.Payment).WithMany(payment => payment.Reconciliations).HasForeignKey(item => item.PaymentId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<BankReconciliation>().HasOne(item => item.ReconciledByUser).WithMany().HasForeignKey(item => item.ReconciledByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<CashFundEntry>().HasOne(item => item.Payment).WithMany(payment => payment.CashEntries).HasForeignKey(item => item.PaymentId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<CashFundEntry>().HasOne(item => item.Branch).WithMany().HasForeignKey(item => item.BranchId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<CashFundEntry>().HasOne(item => item.CreatedByUser).WithMany().HasForeignKey(item => item.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<EInvoice>().HasOne(item => item.Order).WithOne().HasForeignKey<EInvoice>(item => item.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<EInvoice>().HasOne(item => item.CreatedByUser).WithMany().HasForeignKey(item => item.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<BuybackCase>().HasOne(item => item.Branch).WithMany().HasForeignKey(item => item.BranchId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<BuybackCase>().HasOne(item => item.Product).WithMany().HasForeignKey(item => item.ProductId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<BuybackCase>().HasOne(item => item.OrderDetail).WithMany().HasForeignKey(item => item.OrderDetailId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<BuybackCase>().HasOne(item => item.CreatedByUser).WithMany().HasForeignKey(item => item.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<BuybackCase>().HasOne(item => item.ApprovedByUser).WithMany().HasForeignKey(item => item.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<BuybackAssay>().HasOne(item => item.BuybackCase).WithMany(item => item.Assays).HasForeignKey(item => item.BuybackCaseId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<BuybackAssay>().HasOne(item => item.AssayedByUser).WithMany().HasForeignKey(item => item.AssayedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ReturnCase>().HasOne(item => item.Branch).WithMany().HasForeignKey(item => item.BranchId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ReturnCase>().HasOne(item => item.Order).WithMany().HasForeignKey(item => item.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ReturnCase>().HasOne(item => item.OrderDetail).WithMany().HasForeignKey(item => item.OrderDetailId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ReturnCase>().HasOne(item => item.Refund).WithOne(item => item.ReturnCase).HasForeignKey<Refund>(item => item.ReturnCaseId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Refund>().HasOne(item => item.Payment).WithMany().HasForeignKey(item => item.PaymentId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<WarrantyCase>().HasOne(item => item.Branch).WithMany().HasForeignKey(item => item.BranchId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<WarrantyCase>().HasOne(item => item.OrderDetail).WithMany().HasForeignKey(item => item.OrderDetailId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<WarrantyCase>().HasOne(item => item.CreatedByUser).WithMany().HasForeignKey(item => item.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<RepairCase>().HasOne(item => item.Branch).WithMany().HasForeignKey(item => item.BranchId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<RepairCase>().HasOne(item => item.WarrantyCase).WithMany(item => item.Repairs).HasForeignKey(item => item.WarrantyCaseId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<RepairCase>().HasOne(item => item.OrderDetail).WithMany().HasForeignKey(item => item.OrderDetailId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<RepairCase>().HasOne(item => item.CreatedByUser).WithMany().HasForeignKey(item => item.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<RepairCase>().HasOne(item => item.ApprovedByUser).WithMany().HasForeignKey(item => item.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PurityDefinition>().HasData(
                new PurityDefinition { Id = 1, Code = "GOLD-9999", Material = ProductMaterialOptions.Gold, DisplayName = "Vàng 9999 (24K)", Rate = 0.9999m, Karat = 24m, IsActive = true, CreatedAt = new System.DateTime(2025, 1, 1, 0, 0, 0, System.DateTimeKind.Utc) },
                new PurityDefinition { Id = 2, Code = "GOLD-750", Material = ProductMaterialOptions.Gold, DisplayName = "Vàng 750 (18K)", Rate = 0.7500m, Karat = 18m, IsActive = true, CreatedAt = new System.DateTime(2025, 1, 1, 0, 0, 0, System.DateTimeKind.Utc) },
                new PurityDefinition { Id = 3, Code = "GOLD-585", Material = ProductMaterialOptions.Gold, DisplayName = "Vàng 585 (14K)", Rate = 0.5850m, Karat = 14m, IsActive = true, CreatedAt = new System.DateTime(2025, 1, 1, 0, 0, 0, System.DateTimeKind.Utc) },
                new PurityDefinition { Id = 4, Code = "SILVER-999", Material = ProductMaterialOptions.Silver, DisplayName = "Bạc 999", Rate = 0.9990m, IsActive = true, CreatedAt = new System.DateTime(2025, 1, 1, 0, 0, 0, System.DateTimeKind.Utc) },
                new PurityDefinition { Id = 5, Code = "SILVER-925", Material = ProductMaterialOptions.Silver, DisplayName = "Bạc 925", Rate = 0.9250m, IsActive = true, CreatedAt = new System.DateTime(2025, 1, 1, 0, 0, 0, System.DateTimeKind.Utc) },
                new PurityDefinition { Id = 6, Code = "DIAMOND-1000", Material = ProductMaterialOptions.Diamond, DisplayName = "Kim cương (không áp dụng hàm lượng kim loại)", Rate = 1.0000m, IsActive = true, CreatedAt = new System.DateTime(2025, 1, 1, 0, 0, 0, System.DateTimeKind.Utc) });

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
                .HasOne(item => item.Product)
                .WithMany()
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InventoryItem>()
                .HasIndex(item => new { item.ProductId, item.WarehouseId, item.Status });

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

            builder.Entity<InventoryStocktake>()
                .HasIndex(x => x.StocktakeCode)
                .IsUnique();

            builder.Entity<InventoryStocktake>()
                .HasOne(x => x.Warehouse)
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InventoryStocktake>()
                .HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InventoryStocktakeDetail>()
                .HasIndex(x => new { x.InventoryStocktakeId, x.InventoryItemId })
                .IsUnique();

            builder.Entity<InventoryStocktakeDetail>()
                .HasOne(x => x.InventoryStocktake)
                .WithMany(x => x.Details)
                .HasForeignKey(x => x.InventoryStocktakeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<InventoryStocktakeDetail>()
                .HasOne(x => x.InventoryItem)
                .WithMany()
                .HasForeignKey(x => x.InventoryItemId)
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

            /*
             * SẢN XUẤT / CHẾ TÁC / GIA CÔNG
             * Chứng từ sản xuất là lịch sử bất biến nên mọi quan hệ đều Restrict.
             * Việc hủy được thực hiện bằng chuyển trạng thái, không xóa dây chuyền dữ liệu.
             */
            builder.Entity<ProductionWorkshop>()
                .HasIndex(item => new { item.BranchId, item.IsActive });

            builder.Entity<ProductionLossPolicy>()
                .HasIndex(item => new { item.BranchId, item.Status, item.EffectiveFrom });

            builder.Entity<RawMaterialLot>()
                .HasIndex(item => new { item.BranchId, item.Status, item.MaterialType });

            builder.Entity<ProductionBom>()
                .HasIndex(item => new { item.BranchId, item.ProductId, item.Status });

            builder.Entity<ProductionWorkOrder>()
                .HasIndex(item => new { item.BranchId, item.Status, item.PlannedStartAt });

            builder.Entity<ProductionWorkOrder>()
                .HasOne(item => item.WipInventoryItem)
                .WithMany()
                .HasForeignKey(item => item.WipInventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionLossRecord>()
                .HasIndex(item => new { item.ProductionWorkOrderId, item.Status, item.IsOverTolerance });

            builder.Entity<CustomerJobOrder>()
                .HasIndex(item => new { item.BranchId, item.Status, item.PromisedAt });

            builder.Entity<CustomerMaterialCustodyRecord>()
                .HasIndex(item => item.CustomerJobOrderId)
                .IsUnique();
            builder.Entity<CustomerMaterialCustodyRecord>()
                .HasOne(item => item.CustomerJobOrder)
                .WithMany()
                .HasForeignKey(item => item.CustomerJobOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionRecycleBatch>()
                .HasIndex(item => new { item.BranchId, item.Status });

            builder.Entity<ProductionStatusHistory>()
                .HasIndex(item => new { item.EntityType, item.EntityId, item.ChangedAt });

            builder.Entity<ProductionReceipt>()
                .HasIndex(item => item.ProductionWorkOrderId)
                .IsUnique();

            builder.Entity<ProductionReceipt>()
                .HasOne(item => item.ProductionQualityInspection)
                .WithOne(item => item.Receipt)
                .HasForeignKey<ProductionReceipt>(item => item.ProductionQualityInspectionId)
                .OnDelete(DeleteBehavior.Restrict);

            var immutableProductionTypes = new[]
            {
                typeof(ProductionWorkshop),
                typeof(ProductionLossPolicy),
                typeof(RawMaterialLot),
                typeof(ProductionBom),
                typeof(ProductionBomItem),
                typeof(ProductionBomOperation),
                typeof(ProductionWorkOrder),
                typeof(ProductionMaterialReservation),
                typeof(ProductionOperationLog),
                typeof(ProductionLossRecord),
                typeof(ProductionQualityInspection),
                typeof(ProductionReceipt),
                typeof(ProductionRecycleBatch),
                typeof(CustomerJobOrder),
                typeof(CustomerMaterialCustodyRecord),
                typeof(ProductionStatusHistory)
            };

            foreach (var productionType in immutableProductionTypes)
            {
                var entityType = builder.Model.FindEntityType(productionType);
                if (entityType == null)
                {
                    continue;
                }

                foreach (var foreignKey in entityType.GetForeignKeys())
                {
                    foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
                }
            }
        }
    }
}
