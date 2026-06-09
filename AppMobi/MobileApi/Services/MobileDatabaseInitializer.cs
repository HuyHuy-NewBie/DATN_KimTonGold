using Microsoft.EntityFrameworkCore;
using MobileApi.Data;

namespace MobileApi.Services;

public class MobileDatabaseInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MobileDatabaseInitializer> _logger;

    public MobileDatabaseInitializer(ApplicationDbContext context, ILogger<MobileDatabaseInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Ensuring mobile support tables exist.");

        await _context.Database.ExecuteSqlRawAsync("""
IF OBJECT_ID(N'[dbo].[MobileRefreshTokens]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[MobileRefreshTokens](
        [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_MobileRefreshTokens] PRIMARY KEY,
        [UserId] nvarchar(450) NOT NULL,
        [DeviceId] nvarchar(100) NOT NULL,
        [TokenHash] nvarchar(128) NOT NULL,
        [UserAgent] nvarchar(300) NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_MobileRefreshTokens_CreatedAt] DEFAULT SYSUTCDATETIME(),
        [LastUsedAt] datetime2 NULL,
        [RevokedAt] datetime2 NULL,
        CONSTRAINT [FK_MobileRefreshTokens_AspNetUsers_UserId]
            FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MobileRefreshTokens_TokenHash' AND object_id = OBJECT_ID(N'[dbo].[MobileRefreshTokens]'))
    CREATE UNIQUE INDEX [IX_MobileRefreshTokens_TokenHash] ON [dbo].[MobileRefreshTokens]([TokenHash]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MobileRefreshTokens_UserId_DeviceId' AND object_id = OBJECT_ID(N'[dbo].[MobileRefreshTokens]'))
    CREATE INDEX [IX_MobileRefreshTokens_UserId_DeviceId] ON [dbo].[MobileRefreshTokens]([UserId], [DeviceId]);
""");

        await _context.Database.ExecuteSqlRawAsync("""
IF OBJECT_ID(N'[dbo].[MobileDeviceTokens]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[MobileDeviceTokens](
        [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_MobileDeviceTokens] PRIMARY KEY,
        [UserId] nvarchar(450) NOT NULL,
        [DeviceId] nvarchar(100) NOT NULL,
        [ExpoPushToken] nvarchar(256) NOT NULL,
        [Platform] nvarchar(50) NULL,
        [IsActive] bit NOT NULL CONSTRAINT [DF_MobileDeviceTokens_IsActive] DEFAULT 1,
        [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_MobileDeviceTokens_CreatedAt] DEFAULT SYSUTCDATETIME(),
        [LastSeenAt] datetime2 NOT NULL CONSTRAINT [DF_MobileDeviceTokens_LastSeenAt] DEFAULT SYSUTCDATETIME(),
        CONSTRAINT [FK_MobileDeviceTokens_AspNetUsers_UserId]
            FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MobileDeviceTokens_UserId_DeviceId' AND object_id = OBJECT_ID(N'[dbo].[MobileDeviceTokens]'))
    CREATE UNIQUE INDEX [IX_MobileDeviceTokens_UserId_DeviceId] ON [dbo].[MobileDeviceTokens]([UserId], [DeviceId]);
""");

        await _context.Database.ExecuteSqlRawAsync("""
IF OBJECT_ID(N'[dbo].[MobileOrderNotificationLogs]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[MobileOrderNotificationLogs](
        [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_MobileOrderNotificationLogs] PRIMARY KEY,
        [OrderId] int NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_MobileOrderNotificationLogs_CreatedAt] DEFAULT SYSUTCDATETIME(),
        CONSTRAINT [FK_MobileOrderNotificationLogs_Orders_OrderId]
            FOREIGN KEY([OrderId]) REFERENCES [dbo].[Orders]([Id]) ON DELETE CASCADE
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MobileOrderNotificationLogs_OrderId' AND object_id = OBJECT_ID(N'[dbo].[MobileOrderNotificationLogs]'))
    CREATE UNIQUE INDEX [IX_MobileOrderNotificationLogs_OrderId] ON [dbo].[MobileOrderNotificationLogs]([OrderId]);
""");
    }
}
