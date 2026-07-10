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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

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
        }
    }
}
