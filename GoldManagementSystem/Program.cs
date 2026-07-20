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

// Bảo đảm đầy đủ cấp bậc nhân sự mà khu quản trị sử dụng.
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
