using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MobileApi.Models;

namespace MobileApi.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<GoldProductCatalogEntry> GoldProductCatalogEntries => Set<GoldProductCatalogEntry>();
    public DbSet<SilverProductCatalogEntry> SilverProductCatalogEntries => Set<SilverProductCatalogEntry>();
    public DbSet<DiamondProductCatalogEntry> DiamondProductCatalogEntries => Set<DiamondProductCatalogEntry>();
    public DbSet<GoldSilverProductCatalogEntry> GoldSilverProductCatalogEntries => Set<GoldSilverProductCatalogEntry>();
    public DbSet<GoldDiamondProductCatalogEntry> GoldDiamondProductCatalogEntries => Set<GoldDiamondProductCatalogEntry>();
    public DbSet<SilverDiamondProductCatalogEntry> SilverDiamondProductCatalogEntries => Set<SilverDiamondProductCatalogEntry>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<MobileRefreshToken> MobileRefreshTokens => Set<MobileRefreshToken>();
    public DbSet<MobileDeviceToken> MobileDeviceTokens => Set<MobileDeviceToken>();
    public DbSet<MobileOrderNotificationLog> MobileOrderNotificationLogs => Set<MobileOrderNotificationLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Branch>()
            .HasMany(branch => branch.Products)
            .WithOne(product => product.Branch)
            .HasForeignKey(product => product.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Branch>()
            .HasMany(branch => branch.Orders)
            .WithOne(order => order.Branch)
            .HasForeignKey(order => order.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AppUser>()
            .HasOne(user => user.Branch)
            .WithMany(branch => branch.AppUsers)
            .HasForeignKey(user => user.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Order>()
            .HasMany(order => order.OrderDetails)
            .WithOne(detail => detail.Order)
            .HasForeignKey(detail => detail.OrderId);

        builder.Entity<OrderDetail>()
            .HasOne(detail => detail.Product)
            .WithMany()
            .HasForeignKey(detail => detail.ProductId)
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

        builder.Entity<MobileRefreshToken>()
            .HasIndex(token => token.TokenHash)
            .IsUnique();

        builder.Entity<MobileRefreshToken>()
            .HasIndex(token => new { token.UserId, token.DeviceId });

        builder.Entity<MobileDeviceToken>()
            .HasIndex(token => new { token.UserId, token.DeviceId })
            .IsUnique();

        builder.Entity<MobileOrderNotificationLog>()
            .HasIndex(log => log.OrderId)
            .IsUnique();
    }
}
