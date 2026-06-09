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
        }
    }
}
