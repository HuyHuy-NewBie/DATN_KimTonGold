using GoldManagementSystem.Data;
using GoldManagementSystem.Models;
using GoldManagementSystem.Properties.Services;
using GoldManagementSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Background service lỗi sẽ không crash toàn bộ app
builder.Services.Configure<HostOptions>(options =>
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore);


// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString)
{
    Encrypt = false,
    TrustServerCertificate = true
};

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        sqlConnectionStringBuilder.ConnectionString,
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

// Cấu hình custom Identity cho 6 roles
builder.Services.AddIdentity<AppUser, IdentityRole>(options => {
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

builder.Services.Configure<NotificationOptions>(builder.Configuration.GetSection("Notifications"));
builder.Services.Configure<AuthVerificationOptions>(builder.Configuration.GetSection("AuthVerification"));
builder.Services.AddScoped<AuthNotificationService>();
builder.Services.AddScoped<InventoryStockService>();
builder.Services.AddScoped<IManagementPermissionService, ManagementPermissionService>();
builder.Services.AddScoped<SystemNotificationService>();
builder.Services.AddSingleton<PendingAccountVerificationService>();
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();
builder.Services.AddHostedService<OrderCleanupWorker>();
builder.Services.AddHttpClient("PreciousMetals", client =>
{
    client.BaseAddress = new Uri("https://data.silv.app/");
    client.Timeout = TimeSpan.FromSeconds(12);
});
builder.Services.AddHttpClient("ExchangeRates", client =>
{
    client.BaseAddress = new Uri("https://api.frankfurter.dev/");
    client.Timeout = TimeSpan.FromSeconds(12);
});
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<ChatService>();
builder.Services.AddScoped<IMarketPriceService, MarketPriceService>();
builder.Services.AddHostedService<MarketUpdateWorker>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Bảo đảm đầy đủ cấp bậc nhân sự và tài khoản Admin mặc định.
using (var roleScope = app.Services.CreateScope())
{
    try
    {
        var roleManager = roleScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var roleName in RoleCatalog.AllOrderedRoles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole(roleName));
        }

        var dbContext = roleScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Branch defaultBranch = null;
        if (!await dbContext.Branches.AnyAsync())
        {
            defaultBranch = new Branch
            {
                BranchName = "Chi nhánh Trụ sở Chính",
                Address = "Hà Nội",
                PhoneNumber = "0961137407",
                IsActive = true
            };
            dbContext.Branches.Add(defaultBranch);
            await dbContext.SaveChangesAsync();
        }

        var userManager = roleScope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var adminEmail = "admin@goldsys.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Admin",
                IsActive = true,
                EmailConfirmed = true,
                PhoneNumber = "0961137407",
                PhoneNumberConfirmed = true,
                BranchId = defaultBranch?.Id,
                CreatedAt = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, RoleCatalog.Admin);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                app.Logger.LogWarning($"Không thể tạo tài khoản Admin mặc định: {errors}");
            }
        }
        else if (adminUser.BranchId == null && defaultBranch != null)
        {
            adminUser.BranchId = defaultBranch.Id;
            await userManager.UpdateAsync(adminUser);
        }
    }
    catch (Exception exception)
    {
        // Không chặn web khởi động khi SQL Server tạm thời mất kết nối.
        app.Logger.LogWarning(exception, "Không thể đồng bộ danh sách vai trò lúc khởi động.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var userManager = context.RequestServices.GetRequiredService<UserManager<AppUser>>();
        var signInManager = context.RequestServices.GetRequiredService<SignInManager<AppUser>>();
        var currentUser = await userManager.GetUserAsync(context.User);

        if (currentUser == null || !currentUser.IsActive)
        {
            await signInManager.SignOutAsync();
            context.Response.Redirect("/Account/Login?accountLocked=1");
            return;
        }
    }

    await next();
});
app.UseAuthorization();
app.UseMiddleware<ManagementAccessMiddleware>();
app.UseMiddleware<AuditTrailMiddleware>();

app.MapHub<GoldManagementSystem.Hubs.NotificationHub>("/notificationHub");
app.MapHub<GoldManagementSystem.Hubs.SupportChatHub>("/supportChatHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
