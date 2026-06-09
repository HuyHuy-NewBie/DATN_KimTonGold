using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MobileApi.Data;
using MobileApi.Models;
using MobileApi.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString)
{
    Encrypt = false,
    TrustServerCertificate = true
};

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        sqlConnectionStringBuilder.ConnectionString,
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.BackOffice, policy =>
        policy.RequireRole(RoleCatalog.BackOfficeRoles));
    options.AddPolicy(Policies.ProductWrite, policy =>
        policy.RequireRole(RoleCatalog.Admin, RoleCatalog.BranchOwner, RoleCatalog.Manager, RoleCatalog.Staff));
    options.AddPolicy(Policies.OrderRead, policy =>
        policy.RequireRole(RoleCatalog.Admin, RoleCatalog.BranchOwner, RoleCatalog.Manager, RoleCatalog.Staff, RoleCatalog.Accountant));
    options.AddPolicy(Policies.OrderManage, policy =>
        policy.RequireRole(RoleCatalog.Admin, RoleCatalog.BranchOwner, RoleCatalog.Manager));
    options.AddPolicy(Policies.ReportsRead, policy =>
        policy.RequireRole(RoleCatalog.Admin, RoleCatalog.BranchOwner, RoleCatalog.Manager, RoleCatalog.Accountant));
    options.AddPolicy(Policies.UsersManage, policy =>
        policy.RequireRole(RoleCatalog.Admin, RoleCatalog.BranchOwner, RoleCatalog.Manager));
    options.AddPolicy(Policies.BranchesManage, policy =>
        policy.RequireRole(RoleCatalog.Admin));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("MobileDev", policy =>
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin());
});

builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<MobileDatabaseInitializer>();
builder.Services.AddHttpClient<ExpoPushNotificationService>();
builder.Services.AddHostedService<PendingOrderNotificationWorker>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<MobileDatabaseInitializer>().InitializeAsync();
}

app.UseCors("MobileDev");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
