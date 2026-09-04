IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170407_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170407_InitialCreate'
)
BEGIN
    CREATE TABLE [Branches] (
        [Id] int NOT NULL IDENTITY,
        [BranchName] nvarchar(150) NOT NULL,
        [Address] nvarchar(300) NOT NULL,
        [PhoneNumber] nvarchar(20) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Branches] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170407_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170407_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FullName] nvarchar(100) NOT NULL,
        [BranchId] int NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUsers_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170407_InitialCreate'
)
BEGIN
    CREATE TABLE [Products] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Category] nvarchar(100) NOT NULL,
        [GoldType] nvarchar(100) NOT NULL,
        [Weight] decimal(18,2) NOT NULL,
        [ProcessingFee] decimal(18,2) NOT NULL,
        [SellPrice] decimal(18,2) NOT NULL,
        [BuyPrice] decimal(18,2) NOT NULL,
        [BranchId] int NOT NULL,
        [ImagesUrl] nvarchar(max) NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Products_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170407_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170407_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170407_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170407_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170407_InitialCreate'
)
BEGIN
    CREATE TABLE [Orders] (
        [Id] int NOT NULL IDENTITY,
        [OrderNumber] nvarchar(max) NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [BranchId] int NOT NULL,
        [CustomerName] nvarchar(100) NOT NULL,
        [CustomerPhone] nvarchar(20) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [OrderDate] datetime2 NOT NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Orders_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Orders_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170407_InitialCreate'
)
BEGIN
    CREATE TABLE [OrderDetails] (
        [Id] int NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [ProductId] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [Quantity] int NOT NULL,
        CONSTRAINT [PK_OrderDetails] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderDetails_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_OrderDetails_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170407_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170407_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170407_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170407_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170407_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170407_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170407_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUsers_BranchId] ON [AspNetUsers] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170407_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170407_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderDetails_OrderId] ON [OrderDetails] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170407_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderDetails_ProductId] ON [OrderDetails] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170407_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_BranchId] ON [Orders] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170407_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_UserId] ON [Orders] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170407_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Products_BranchId] ON [Products] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170407_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260407170407_InitialCreate', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410072242_UpdateMarketHistory'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'Status');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Products] ALTER COLUMN [Status] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410072242_UpdateMarketHistory'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'ImagesUrl');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Products] ALTER COLUMN [ImagesUrl] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410072242_UpdateMarketHistory'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Orders]') AND [c].[name] = N'Status');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Orders] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Orders] ALTER COLUMN [Status] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410072242_UpdateMarketHistory'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Orders]') AND [c].[name] = N'OrderNumber');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Orders] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [Orders] ALTER COLUMN [OrderNumber] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410072242_UpdateMarketHistory'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Orders]') AND [c].[name] = N'CustomerPhone');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Orders] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [Orders] ALTER COLUMN [CustomerPhone] nvarchar(20) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410072242_UpdateMarketHistory'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Orders]') AND [c].[name] = N'CustomerName');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Orders] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [Orders] ALTER COLUMN [CustomerName] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410072242_UpdateMarketHistory'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Branches]') AND [c].[name] = N'PhoneNumber');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Branches] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [Branches] ALTER COLUMN [PhoneNumber] nvarchar(20) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410072242_UpdateMarketHistory'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Branches]') AND [c].[name] = N'Address');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Branches] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [Branches] ALTER COLUMN [Address] nvarchar(300) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410072242_UpdateMarketHistory'
)
BEGIN
    CREATE TABLE [FavoriteProducts] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ProductId] int NOT NULL,
        [AddedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_FavoriteProducts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FavoriteProducts_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_FavoriteProducts_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410072242_UpdateMarketHistory'
)
BEGIN
    CREATE TABLE [MarketHistories] (
        [Id] int NOT NULL IDENTITY,
        [Symbol] nvarchar(100) NOT NULL,
        [DisplayName] nvarchar(max) NULL,
        [MarketType] nvarchar(max) NULL,
        [BuyPrice] decimal(18,2) NOT NULL,
        [SellPrice] decimal(18,2) NOT NULL,
        [Unit] nvarchar(max) NULL,
        [Timestamp] datetime2 NOT NULL,
        CONSTRAINT [PK_MarketHistories] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410072242_UpdateMarketHistory'
)
BEGIN
    CREATE INDEX [IX_FavoriteProducts_ProductId] ON [FavoriteProducts] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410072242_UpdateMarketHistory'
)
BEGIN
    CREATE INDEX [IX_FavoriteProducts_UserId] ON [FavoriteProducts] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410072242_UpdateMarketHistory'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260410072242_UpdateMarketHistory', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416092244_AddDiamondCatalogAndSmsFlow'
)
BEGIN
    ALTER TABLE [Products] ADD [Description] nvarchar(2000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416092244_AddDiamondCatalogAndSmsFlow'
)
BEGIN
    ALTER TABLE [Products] ADD [DiamondCarat] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416092244_AddDiamondCatalogAndSmsFlow'
)
BEGIN
    ALTER TABLE [Products] ADD [DiamondCertificate] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416092244_AddDiamondCatalogAndSmsFlow'
)
BEGIN
    ALTER TABLE [Products] ADD [DiamondClarity] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416092244_AddDiamondCatalogAndSmsFlow'
)
BEGIN
    ALTER TABLE [Products] ADD [DiamondColor] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416092244_AddDiamondCatalogAndSmsFlow'
)
BEGIN
    ALTER TABLE [Products] ADD [DiamondCut] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416092244_AddDiamondCatalogAndSmsFlow'
)
BEGIN
    ALTER TABLE [Products] ADD [DiamondShape] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416092244_AddDiamondCatalogAndSmsFlow'
)
BEGIN
    ALTER TABLE [Products] ADD [DiamondSize] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416092244_AddDiamondCatalogAndSmsFlow'
)
BEGIN
    ALTER TABLE [Products] ADD [ProductLine] nvarchar(30) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416092244_AddDiamondCatalogAndSmsFlow'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260416092244_AddDiamondCatalogAndSmsFlow', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421194209_SplitProductCatalogTablesByLine'
)
BEGIN
    CREATE TABLE [DiamondProductCatalogEntries] (
        [ProductId] int NOT NULL,
        CONSTRAINT [PK_DiamondProductCatalogEntries] PRIMARY KEY ([ProductId]),
        CONSTRAINT [FK_DiamondProductCatalogEntries_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421194209_SplitProductCatalogTablesByLine'
)
BEGIN
    CREATE TABLE [GoldDiamondProductCatalogEntries] (
        [ProductId] int NOT NULL,
        CONSTRAINT [PK_GoldDiamondProductCatalogEntries] PRIMARY KEY ([ProductId]),
        CONSTRAINT [FK_GoldDiamondProductCatalogEntries_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421194209_SplitProductCatalogTablesByLine'
)
BEGIN
    CREATE TABLE [GoldProductCatalogEntries] (
        [ProductId] int NOT NULL,
        CONSTRAINT [PK_GoldProductCatalogEntries] PRIMARY KEY ([ProductId]),
        CONSTRAINT [FK_GoldProductCatalogEntries_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421194209_SplitProductCatalogTablesByLine'
)
BEGIN
    CREATE TABLE [GoldSilverProductCatalogEntries] (
        [ProductId] int NOT NULL,
        CONSTRAINT [PK_GoldSilverProductCatalogEntries] PRIMARY KEY ([ProductId]),
        CONSTRAINT [FK_GoldSilverProductCatalogEntries_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421194209_SplitProductCatalogTablesByLine'
)
BEGIN
    CREATE TABLE [SilverDiamondProductCatalogEntries] (
        [ProductId] int NOT NULL,
        CONSTRAINT [PK_SilverDiamondProductCatalogEntries] PRIMARY KEY ([ProductId]),
        CONSTRAINT [FK_SilverDiamondProductCatalogEntries_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421194209_SplitProductCatalogTablesByLine'
)
BEGIN
    CREATE TABLE [SilverProductCatalogEntries] (
        [ProductId] int NOT NULL,
        CONSTRAINT [PK_SilverProductCatalogEntries] PRIMARY KEY ([ProductId]),
        CONSTRAINT [FK_SilverProductCatalogEntries_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421194209_SplitProductCatalogTablesByLine'
)
BEGIN

                    SELECT
                        p.Id AS ProductId,
                        CASE
                            WHEN p.Category = N'Trang Sức Bạc'
                                OR p.GoldType LIKE N'%Bạc%'
                                OR p.Name LIKE N'%Bạc%' THEN CAST(1 AS bit)
                            ELSE CAST(0 AS bit)
                        END AS IsSilver,
                        CASE
                            WHEN p.Category = N'Kim Cương'
                                OR p.DiamondCarat IS NOT NULL
                                OR p.DiamondSize IS NOT NULL
                                OR p.DiamondShape IS NOT NULL
                                OR p.GoldType LIKE N'%Kim cương%'
                                OR p.GoldType LIKE N'%Kim Cương%'
                                OR p.GoldType LIKE N'%Moissanite%'
                                OR p.GoldType LIKE N'%Cubic%'
                                OR p.Name LIKE N'%Kim cương%'
                                OR p.Name LIKE N'%Kim Cương%'
                                OR p.Name LIKE N'%Moissanite%'
                                OR p.Name LIKE N'%Cubic%' THEN CAST(1 AS bit)
                            ELSE CAST(0 AS bit)
                        END AS IsDiamond,
                        CASE
                            WHEN p.GoldType LIKE N'%Vàng%'
                                OR p.Name LIKE N'%Vàng%'
                                OR (
                                    p.Category <> N'Trang Sức Bạc'
                                    AND p.Category <> N'Kim Cương'
                                    AND (p.GoldType IS NULL OR (
                                        p.GoldType NOT LIKE N'%Bạc%'
                                        AND p.GoldType NOT LIKE N'%Kim cương%'
                                        AND p.GoldType NOT LIKE N'%Kim Cương%'
                                        AND p.GoldType NOT LIKE N'%Moissanite%'
                                        AND p.GoldType NOT LIKE N'%Cubic%'))
                                    AND (p.Name IS NULL OR (
                                        p.Name NOT LIKE N'%Bạc%'
                                        AND p.Name NOT LIKE N'%Kim cương%'
                                        AND p.Name NOT LIKE N'%Kim Cương%'
                                        AND p.Name NOT LIKE N'%Moissanite%'
                                        AND p.Name NOT LIKE N'%Cubic%'))
                                ) THEN CAST(1 AS bit)
                            ELSE CAST(0 AS bit)
                        END AS IsGold
                    INTO #CatalogRouting
                    FROM Products p;

                    UPDATE p
                    SET ProductLine = CASE
                        WHEN route.IsDiamond = 1 AND route.IsGold = 1 THEN N'Gold'
                        WHEN route.IsDiamond = 1 AND route.IsSilver = 1 THEN N'Silver'
                        WHEN route.IsGold = 1 AND route.IsSilver = 1 THEN
                            CASE
                                WHEN p.Category = N'Trang Sức Bạc' OR p.GoldType LIKE N'%Bạc%' THEN N'Silver'
                                ELSE N'Gold'
                            END
                        WHEN route.IsDiamond = 1 THEN N'Diamond'
                        WHEN route.IsSilver = 1 THEN N'Silver'
                        ELSE N'Gold'
                    END
                    FROM Products p
                    INNER JOIN #CatalogRouting route ON route.ProductId = p.Id;

                    INSERT INTO GoldSilverProductCatalogEntries (ProductId)
                    SELECT route.ProductId
                    FROM #CatalogRouting route
                    WHERE route.IsGold = 1 AND route.IsSilver = 1 AND route.IsDiamond = 0;

                    INSERT INTO GoldDiamondProductCatalogEntries (ProductId)
                    SELECT route.ProductId
                    FROM #CatalogRouting route
                    WHERE route.IsGold = 1 AND route.IsDiamond = 1;

                    INSERT INTO SilverDiamondProductCatalogEntries (ProductId)
                    SELECT route.ProductId
                    FROM #CatalogRouting route
                    WHERE route.IsSilver = 1 AND route.IsDiamond = 1 AND route.IsGold = 0;

                    INSERT INTO GoldProductCatalogEntries (ProductId)
                    SELECT route.ProductId
                    FROM #CatalogRouting route
                    WHERE route.IsGold = 1 AND route.IsSilver = 0 AND route.IsDiamond = 0;

                    INSERT INTO SilverProductCatalogEntries (ProductId)
                    SELECT route.ProductId
                    FROM #CatalogRouting route
                    WHERE route.IsSilver = 1 AND route.IsGold = 0 AND route.IsDiamond = 0;

                    INSERT INTO DiamondProductCatalogEntries (ProductId)
                    SELECT route.ProductId
                    FROM #CatalogRouting route
                    WHERE route.IsDiamond = 1 AND route.IsGold = 0 AND route.IsSilver = 0;

                    DROP TABLE #CatalogRouting;
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421194209_SplitProductCatalogTablesByLine'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260421194209_SplitProductCatalogTablesByLine', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612013008_AddOrderDepositWorkflow'
)
BEGIN
    ALTER TABLE [Orders] ADD [CancelReason] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612013008_AddOrderDepositWorkflow'
)
BEGIN
    ALTER TABLE [Orders] ADD [ConfirmedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612013008_AddOrderDepositWorkflow'
)
BEGIN
    ALTER TABLE [Orders] ADD [DepositAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612013008_AddOrderDepositWorkflow'
)
BEGIN
    ALTER TABLE [Orders] ADD [DepositDueAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612013008_AddOrderDepositWorkflow'
)
BEGIN
    ALTER TABLE [Orders] ADD [DepositPaidAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612013008_AddOrderDepositWorkflow'
)
BEGIN
    ALTER TABLE [Orders] ADD [DepositRate] decimal(5,2) NOT NULL DEFAULT 10.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612013008_AddOrderDepositWorkflow'
)
BEGIN
    ALTER TABLE [Orders] ADD [PaymentMethod] nvarchar(30) NOT NULL DEFAULT N'OnlineDeposit';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612013008_AddOrderDepositWorkflow'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260612013008_AddOrderDepositWorkflow', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701194610_AddSupplierManagementModule'
)
BEGIN
    CREATE TABLE [Suppliers] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(180) NOT NULL,
        [TaxCode] nvarchar(30) NULL,
        [ContactPerson] nvarchar(120) NOT NULL,
        [Phone] nvarchar(20) NOT NULL,
        [Email] nvarchar(150) NULL,
        [Address] nvarchar(300) NULL,
        [SupplierType] nvarchar(80) NULL,
        [PaymentTermDays] int NOT NULL,
        [BankName] nvarchar(120) NULL,
        [BankAccountNumber] nvarchar(50) NULL,
        [BankAccountName] nvarchar(120) NULL,
        [Note] nvarchar(1000) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Suppliers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701194610_AddSupplierManagementModule'
)
BEGIN
    CREATE TABLE [SupplierPurchaseOrders] (
        [Id] int NOT NULL IDENTITY,
        [OrderCode] nvarchar(30) NOT NULL,
        [SupplierId] int NOT NULL,
        [BranchId] int NOT NULL,
        [CreatedByUserId] nvarchar(450) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ExpectedDeliveryDate] datetime2 NULL,
        [Status] nvarchar(50) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [Note] nvarchar(1000) NULL,
        CONSTRAINT [PK_SupplierPurchaseOrders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SupplierPurchaseOrders_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SupplierPurchaseOrders_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SupplierPurchaseOrders_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701194610_AddSupplierManagementModule'
)
BEGIN
    CREATE TABLE [SupplierGoodsReceipts] (
        [Id] int NOT NULL IDENTITY,
        [ReceiptCode] nvarchar(30) NOT NULL,
        [SupplierPurchaseOrderId] int NOT NULL,
        [CreatedByUserId] nvarchar(450) NOT NULL,
        [ReceivedAt] datetime2 NOT NULL,
        [TotalAcceptedValue] decimal(18,2) NOT NULL,
        [Note] nvarchar(1000) NULL,
        CONSTRAINT [PK_SupplierGoodsReceipts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SupplierGoodsReceipts_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SupplierGoodsReceipts_SupplierPurchaseOrders_SupplierPurchaseOrderId] FOREIGN KEY ([SupplierPurchaseOrderId]) REFERENCES [SupplierPurchaseOrders] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701194610_AddSupplierManagementModule'
)
BEGIN
    CREATE TABLE [SupplierPayments] (
        [Id] int NOT NULL IDENTITY,
        [PaymentCode] nvarchar(30) NOT NULL,
        [SupplierId] int NOT NULL,
        [SupplierPurchaseOrderId] int NOT NULL,
        [CreatedByUserId] nvarchar(450) NOT NULL,
        [PaidAt] datetime2 NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [PaymentMethod] nvarchar(50) NOT NULL,
        [ReferenceNumber] nvarchar(100) NULL,
        [Note] nvarchar(500) NULL,
        CONSTRAINT [PK_SupplierPayments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SupplierPayments_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SupplierPayments_SupplierPurchaseOrders_SupplierPurchaseOrderId] FOREIGN KEY ([SupplierPurchaseOrderId]) REFERENCES [SupplierPurchaseOrders] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SupplierPayments_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701194610_AddSupplierManagementModule'
)
BEGIN
    CREATE TABLE [SupplierPurchaseOrderDetails] (
        [Id] int NOT NULL IDENTITY,
        [SupplierPurchaseOrderId] int NOT NULL,
        [ProductLine] nvarchar(30) NOT NULL,
        [Category] nvarchar(120) NOT NULL,
        [ProductName] nvarchar(220) NOT NULL,
        [GoldType] nvarchar(120) NOT NULL,
        [Quantity] int NOT NULL,
        [Weight] decimal(18,2) NOT NULL,
        [DiamondCarat] decimal(18,2) NULL,
        [DiamondCertificate] nvarchar(120) NULL,
        [UnitCost] decimal(18,2) NOT NULL,
        [TotalCost] decimal(18,2) NOT NULL,
        [ReceivedQuantity] int NOT NULL,
        [AcceptedQuantity] int NOT NULL,
        [RejectedQuantity] int NOT NULL,
        [Note] nvarchar(500) NULL,
        CONSTRAINT [PK_SupplierPurchaseOrderDetails] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SupplierPurchaseOrderDetails_SupplierPurchaseOrders_SupplierPurchaseOrderId] FOREIGN KEY ([SupplierPurchaseOrderId]) REFERENCES [SupplierPurchaseOrders] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701194610_AddSupplierManagementModule'
)
BEGIN
    CREATE TABLE [SupplierGoodsReceiptDetails] (
        [Id] int NOT NULL IDENTITY,
        [SupplierGoodsReceiptId] int NOT NULL,
        [SupplierPurchaseOrderDetailId] int NOT NULL,
        [ReceivedQuantity] int NOT NULL,
        [AcceptedQuantity] int NOT NULL,
        [RejectedQuantity] int NOT NULL,
        [ActualUnitCost] decimal(18,2) NOT NULL,
        [LineValue] decimal(18,2) NOT NULL,
        [QualityNote] nvarchar(500) NULL,
        CONSTRAINT [PK_SupplierGoodsReceiptDetails] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SupplierGoodsReceiptDetails_SupplierGoodsReceipts_SupplierGoodsReceiptId] FOREIGN KEY ([SupplierGoodsReceiptId]) REFERENCES [SupplierGoodsReceipts] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SupplierGoodsReceiptDetails_SupplierPurchaseOrderDetails_SupplierPurchaseOrderDetailId] FOREIGN KEY ([SupplierPurchaseOrderDetailId]) REFERENCES [SupplierPurchaseOrderDetails] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701194610_AddSupplierManagementModule'
)
BEGIN
    CREATE INDEX [IX_SupplierGoodsReceiptDetails_SupplierGoodsReceiptId] ON [SupplierGoodsReceiptDetails] ([SupplierGoodsReceiptId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701194610_AddSupplierManagementModule'
)
BEGIN
    CREATE INDEX [IX_SupplierGoodsReceiptDetails_SupplierPurchaseOrderDetailId] ON [SupplierGoodsReceiptDetails] ([SupplierPurchaseOrderDetailId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701194610_AddSupplierManagementModule'
)
BEGIN
    CREATE INDEX [IX_SupplierGoodsReceipts_CreatedByUserId] ON [SupplierGoodsReceipts] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701194610_AddSupplierManagementModule'
)
BEGIN
    CREATE INDEX [IX_SupplierGoodsReceipts_SupplierPurchaseOrderId] ON [SupplierGoodsReceipts] ([SupplierPurchaseOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701194610_AddSupplierManagementModule'
)
BEGIN
    CREATE INDEX [IX_SupplierPayments_CreatedByUserId] ON [SupplierPayments] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701194610_AddSupplierManagementModule'
)
BEGIN
    CREATE INDEX [IX_SupplierPayments_SupplierId] ON [SupplierPayments] ([SupplierId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701194610_AddSupplierManagementModule'
)
BEGIN
    CREATE INDEX [IX_SupplierPayments_SupplierPurchaseOrderId] ON [SupplierPayments] ([SupplierPurchaseOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701194610_AddSupplierManagementModule'
)
BEGIN
    CREATE INDEX [IX_SupplierPurchaseOrderDetails_SupplierPurchaseOrderId] ON [SupplierPurchaseOrderDetails] ([SupplierPurchaseOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701194610_AddSupplierManagementModule'
)
BEGIN
    CREATE INDEX [IX_SupplierPurchaseOrders_BranchId] ON [SupplierPurchaseOrders] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701194610_AddSupplierManagementModule'
)
BEGIN
    CREATE INDEX [IX_SupplierPurchaseOrders_CreatedByUserId] ON [SupplierPurchaseOrders] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701194610_AddSupplierManagementModule'
)
BEGIN
    CREATE INDEX [IX_SupplierPurchaseOrders_SupplierId] ON [SupplierPurchaseOrders] ([SupplierId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701194610_AddSupplierManagementModule'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260701194610_AddSupplierManagementModule', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701202004_ImproveSupplierValidationFields'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Suppliers]') AND [c].[name] = N'TaxCode');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Suppliers] DROP CONSTRAINT [' + @var8 + '];');
    EXEC(N'UPDATE [Suppliers] SET [TaxCode] = N'''' WHERE [TaxCode] IS NULL');
    ALTER TABLE [Suppliers] ALTER COLUMN [TaxCode] nvarchar(13) NOT NULL;
    ALTER TABLE [Suppliers] ADD DEFAULT N'' FOR [TaxCode];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701202004_ImproveSupplierValidationFields'
)
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Suppliers]') AND [c].[name] = N'SupplierType');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Suppliers] DROP CONSTRAINT [' + @var9 + '];');
    EXEC(N'UPDATE [Suppliers] SET [SupplierType] = N'''' WHERE [SupplierType] IS NULL');
    ALTER TABLE [Suppliers] ALTER COLUMN [SupplierType] nvarchar(200) NOT NULL;
    ALTER TABLE [Suppliers] ADD DEFAULT N'' FOR [SupplierType];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701202004_ImproveSupplierValidationFields'
)
BEGIN
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Suppliers]') AND [c].[name] = N'Phone');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Suppliers] DROP CONSTRAINT [' + @var10 + '];');
    ALTER TABLE [Suppliers] ALTER COLUMN [Phone] nvarchar(10) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701202004_ImproveSupplierValidationFields'
)
BEGIN
    DECLARE @var11 sysname;
    SELECT @var11 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Suppliers]') AND [c].[name] = N'BankAccountNumber');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Suppliers] DROP CONSTRAINT [' + @var11 + '];');
    ALTER TABLE [Suppliers] ALTER COLUMN [BankAccountNumber] nvarchar(20) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701202004_ImproveSupplierValidationFields'
)
BEGIN
    DECLARE @var12 sysname;
    SELECT @var12 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Suppliers]') AND [c].[name] = N'Address');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Suppliers] DROP CONSTRAINT [' + @var12 + '];');
    EXEC(N'UPDATE [Suppliers] SET [Address] = N'''' WHERE [Address] IS NULL');
    ALTER TABLE [Suppliers] ALTER COLUMN [Address] nvarchar(300) NOT NULL;
    ALTER TABLE [Suppliers] ADD DEFAULT N'' FOR [Address];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701202004_ImproveSupplierValidationFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260701202004_ImproveSupplierValidationFields', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035545_AddWarehouseFoundation'
)
BEGIN
    CREATE TABLE [Warehouses] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(30) NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [BranchId] int NOT NULL,
        [Location] nvarchar(300) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Warehouses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Warehouses_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035545_AddWarehouseFoundation'
)
BEGIN
    CREATE TABLE [InventoryItems] (
        [Id] int NOT NULL IDENTITY,
        [StockCode] nvarchar(40) NOT NULL,
        [WarehouseId] int NOT NULL,
        [SupplierId] int NULL,
        [SupplierPurchaseOrderId] int NULL,
        [SupplierGoodsReceiptDetailId] int NULL,
        [ProductLine] nvarchar(30) NOT NULL,
        [Category] nvarchar(120) NOT NULL,
        [ProductName] nvarchar(220) NOT NULL,
        [MaterialType] nvarchar(120) NOT NULL,
        [QuantityOnHand] int NOT NULL,
        [WeightOnHand] decimal(18,2) NOT NULL,
        [DiamondCarat] decimal(18,2) NULL,
        [CertificateCode] nvarchar(120) NULL,
        [UnitCost] decimal(18,2) NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [Note] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_InventoryItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InventoryItems_SupplierGoodsReceiptDetails_SupplierGoodsReceiptDetailId] FOREIGN KEY ([SupplierGoodsReceiptDetailId]) REFERENCES [SupplierGoodsReceiptDetails] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InventoryItems_SupplierPurchaseOrders_SupplierPurchaseOrderId] FOREIGN KEY ([SupplierPurchaseOrderId]) REFERENCES [SupplierPurchaseOrders] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InventoryItems_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InventoryItems_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035545_AddWarehouseFoundation'
)
BEGIN
    CREATE TABLE [InventoryTransactions] (
        [Id] int NOT NULL IDENTITY,
        [TransactionCode] nvarchar(40) NOT NULL,
        [WarehouseId] int NOT NULL,
        [InventoryItemId] int NOT NULL,
        [TransactionType] nvarchar(50) NOT NULL,
        [QuantityChange] int NOT NULL,
        [WeightChange] decimal(18,2) NOT NULL,
        [QuantityAfter] int NOT NULL,
        [WeightAfter] decimal(18,2) NOT NULL,
        [ReferenceType] nvarchar(50) NULL,
        [ReferenceId] int NULL,
        [Note] nvarchar(500) NULL,
        [CreatedByUserId] nvarchar(450) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_InventoryTransactions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InventoryTransactions_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InventoryTransactions_InventoryItems_InventoryItemId] FOREIGN KEY ([InventoryItemId]) REFERENCES [InventoryItems] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InventoryTransactions_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035545_AddWarehouseFoundation'
)
BEGIN
    CREATE UNIQUE INDEX [IX_InventoryItems_StockCode] ON [InventoryItems] ([StockCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035545_AddWarehouseFoundation'
)
BEGIN
    CREATE INDEX [IX_InventoryItems_SupplierGoodsReceiptDetailId] ON [InventoryItems] ([SupplierGoodsReceiptDetailId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035545_AddWarehouseFoundation'
)
BEGIN
    CREATE INDEX [IX_InventoryItems_SupplierId] ON [InventoryItems] ([SupplierId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035545_AddWarehouseFoundation'
)
BEGIN
    CREATE INDEX [IX_InventoryItems_SupplierPurchaseOrderId] ON [InventoryItems] ([SupplierPurchaseOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035545_AddWarehouseFoundation'
)
BEGIN
    CREATE INDEX [IX_InventoryItems_WarehouseId] ON [InventoryItems] ([WarehouseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035545_AddWarehouseFoundation'
)
BEGIN
    CREATE INDEX [IX_InventoryTransactions_CreatedByUserId] ON [InventoryTransactions] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035545_AddWarehouseFoundation'
)
BEGIN
    CREATE INDEX [IX_InventoryTransactions_InventoryItemId] ON [InventoryTransactions] ([InventoryItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035545_AddWarehouseFoundation'
)
BEGIN
    CREATE UNIQUE INDEX [IX_InventoryTransactions_TransactionCode] ON [InventoryTransactions] ([TransactionCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035545_AddWarehouseFoundation'
)
BEGIN
    CREATE INDEX [IX_InventoryTransactions_WarehouseId] ON [InventoryTransactions] ([WarehouseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035545_AddWarehouseFoundation'
)
BEGIN
    CREATE INDEX [IX_Warehouses_BranchId] ON [Warehouses] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035545_AddWarehouseFoundation'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Warehouses_Code] ON [Warehouses] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035545_AddWarehouseFoundation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260710035545_AddWarehouseFoundation', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710051027_AddSupplierGoodsReceiptWorkflow'
)
BEGIN
    ALTER TABLE [SupplierGoodsReceipts] ADD [DeliveredBy] nvarchar(150) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710051027_AddSupplierGoodsReceiptWorkflow'
)
BEGIN
    ALTER TABLE [SupplierGoodsReceipts] ADD [DeliveryDocumentNumber] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710051027_AddSupplierGoodsReceiptWorkflow'
)
BEGIN
    ALTER TABLE [SupplierGoodsReceipts] ADD [Status] nvarchar(50) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710051027_AddSupplierGoodsReceiptWorkflow'
)
BEGIN
    ALTER TABLE [SupplierGoodsReceipts] ADD [WarehouseId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710051027_AddSupplierGoodsReceiptWorkflow'
)
BEGIN
    ALTER TABLE [SupplierGoodsReceiptDetails] ADD [ActualDiamondCarat] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710051027_AddSupplierGoodsReceiptWorkflow'
)
BEGIN
    ALTER TABLE [SupplierGoodsReceiptDetails] ADD [ActualDiamondCertificate] nvarchar(120) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710051027_AddSupplierGoodsReceiptWorkflow'
)
BEGIN
    ALTER TABLE [SupplierGoodsReceiptDetails] ADD [ActualWeight] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710051027_AddSupplierGoodsReceiptWorkflow'
)
BEGIN
    ALTER TABLE [SupplierGoodsReceiptDetails] ADD [QualityStatus] nvarchar(50) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710051027_AddSupplierGoodsReceiptWorkflow'
)
BEGIN
    ALTER TABLE [SupplierGoodsReceiptDetails] ADD [ReceivingNote] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710051027_AddSupplierGoodsReceiptWorkflow'
)
BEGIN
    ALTER TABLE [SupplierGoodsReceiptDetails] ADD [RejectionReason] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710051027_AddSupplierGoodsReceiptWorkflow'
)
BEGIN
    ALTER TABLE [SupplierGoodsReceiptDetails] ADD [Resolution] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710051027_AddSupplierGoodsReceiptWorkflow'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SupplierGoodsReceipts_ReceiptCode] ON [SupplierGoodsReceipts] ([ReceiptCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710051027_AddSupplierGoodsReceiptWorkflow'
)
BEGIN
    CREATE INDEX [IX_SupplierGoodsReceipts_WarehouseId] ON [SupplierGoodsReceipts] ([WarehouseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710051027_AddSupplierGoodsReceiptWorkflow'
)
BEGIN
    ALTER TABLE [SupplierGoodsReceipts] ADD CONSTRAINT [FK_SupplierGoodsReceipts_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710051027_AddSupplierGoodsReceiptWorkflow'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260710051027_AddSupplierGoodsReceiptWorkflow', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713112217_AddBranchChatMetadata'
)
BEGIN
    ALTER TABLE [Branches] ADD [OrderProcessInfo] nvarchar(1000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713112217_AddBranchChatMetadata'
)
BEGIN
    ALTER TABLE [Branches] ADD [ProductPriceInfo] nvarchar(1000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713112217_AddBranchChatMetadata'
)
BEGIN
    ALTER TABLE [Branches] ADD [SizeSelectionInfo] nvarchar(1000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713112217_AddBranchChatMetadata'
)
BEGIN
    ALTER TABLE [Branches] ADD [TradeInPolicyInfo] nvarchar(1000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713112217_AddBranchChatMetadata'
)
BEGIN
    ALTER TABLE [Branches] ADD [WarrantyInfo] nvarchar(1000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713112217_AddBranchChatMetadata'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260713112217_AddBranchChatMetadata', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714053117_AddChatSettings'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260714053117_AddChatSettings', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260719190059_AddWorkforceManagement'
)
BEGIN
    CREATE TABLE [ChatSettings] (
        [Id] int NOT NULL IDENTITY,
        [ShopName] nvarchar(200) NULL,
        [Hotline] nvarchar(50) NULL,
        [ShopAddress] nvarchar(500) NULL,
        [ProductPriceInfo] nvarchar(max) NULL,
        [SizeGuideInfo] nvarchar(max) NULL,
        [WarrantyInfo] nvarchar(max) NULL,
        [ExchangePolicy] nvarchar(max) NULL,
        [OrderProcess] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(256) NULL,
        CONSTRAINT [PK_ChatSettings] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260719190059_AddWorkforceManagement'
)
BEGIN
    CREATE TABLE [ShiftChangeLogs] (
        [Id] int NOT NULL IDENTITY,
        [WorkShiftId] int NOT NULL,
        [ChangedByUserId] nvarchar(450) NULL,
        [Details] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ShiftChangeLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260719190059_AddWorkforceManagement'
)
BEGIN
    CREATE TABLE [UserFeaturePermissions] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NULL,
        [FeatureKey] nvarchar(100) NULL,
        [IsGranted] bit NOT NULL,
        CONSTRAINT [PK_UserFeaturePermissions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260719190059_AddWorkforceManagement'
)
BEGIN
    CREATE TABLE [WorkShifts] (
        [Id] int NOT NULL IDENTITY,
        [BranchId] int NOT NULL,
        [ShiftDate] datetime2 NOT NULL,
        [ShiftType] nvarchar(20) NULL,
        [StartsAt] datetime2 NOT NULL,
        [EndsAt] datetime2 NOT NULL,
        CONSTRAINT [PK_WorkShifts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WorkShifts_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260719190059_AddWorkforceManagement'
)
BEGIN
    CREATE TABLE [ShiftAssignments] (
        [Id] int NOT NULL IDENTITY,
        [WorkShiftId] int NOT NULL,
        [UserId] nvarchar(450) NULL,
        [CheckedInAt] datetime2 NULL,
        [CheckedOutAt] datetime2 NULL,
        [AttendanceStatus] nvarchar(30) NULL,
        [SystemNote] nvarchar(1000) NULL,
        [ManagerNote] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ShiftAssignments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ShiftAssignments_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_ShiftAssignments_WorkShifts_WorkShiftId] FOREIGN KEY ([WorkShiftId]) REFERENCES [WorkShifts] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260719190059_AddWorkforceManagement'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ExchangePolicy', N'Hotline', N'OrderProcess', N'ProductPriceInfo', N'ShopAddress', N'ShopName', N'SizeGuideInfo', N'UpdatedAt', N'UpdatedBy', N'WarrantyInfo') AND [object_id] = OBJECT_ID(N'[ChatSettings]'))
        SET IDENTITY_INSERT [ChatSettings] ON;
    EXEC(N'INSERT INTO [ChatSettings] ([Id], [ExchangePolicy], [Hotline], [OrderProcess], [ProductPriceInfo], [ShopAddress], [ShopName], [SizeGuideInfo], [UpdatedAt], [UpdatedBy], [WarrantyInfo])
    VALUES (1, N'''', N''1800 9999'', N'''', N'''', N''123 Đường Vàng Kim, Quận 1, TP. HCM'', N''KimTon Gold'', N'''', ''2025-01-01T00:00:00.0000000Z'', N''system'', N'''')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ExchangePolicy', N'Hotline', N'OrderProcess', N'ProductPriceInfo', N'ShopAddress', N'ShopName', N'SizeGuideInfo', N'UpdatedAt', N'UpdatedBy', N'WarrantyInfo') AND [object_id] = OBJECT_ID(N'[ChatSettings]'))
        SET IDENTITY_INSERT [ChatSettings] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260719190059_AddWorkforceManagement'
)
BEGIN
    CREATE INDEX [IX_ShiftAssignments_UserId] ON [ShiftAssignments] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260719190059_AddWorkforceManagement'
)
BEGIN
    CREATE INDEX [IX_ShiftAssignments_WorkShiftId] ON [ShiftAssignments] ([WorkShiftId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260719190059_AddWorkforceManagement'
)
BEGIN
    CREATE INDEX [IX_WorkShifts_BranchId] ON [WorkShifts] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260719190059_AddWorkforceManagement'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260719190059_AddWorkforceManagement', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    DROP INDEX [IX_WorkShifts_BranchId] ON [WorkShifts];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    DROP INDEX [IX_ShiftAssignments_WorkShiftId] ON [ShiftAssignments];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    ALTER TABLE [Products] ADD [IsPriority] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    ALTER TABLE [Products] ADD [PriorityOrder] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    ALTER TABLE [WorkShifts] ADD [ManagerNote] nvarchar(1000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    ALTER TABLE [WorkShifts] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE());
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    ALTER TABLE [ShiftChangeLogs] ADD [ChangeType] nvarchar(30) NULL DEFAULT N'Supplemental';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    ALTER TABLE [UserFeaturePermissions] ADD [BranchId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    ALTER TABLE [UserFeaturePermissions] ADD [GrantedByUserId] nvarchar(450) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    ALTER TABLE [UserFeaturePermissions] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE());
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    CREATE TABLE [BranchWarehouseAccesses] (
        [Id] int NOT NULL IDENTITY,
        [BranchId] int NOT NULL,
        [WarehouseId] int NOT NULL,
        [IsPrimary] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_BranchWarehouseAccesses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BranchWarehouseAccesses_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_BranchWarehouseAccesses_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    CREATE TABLE [EmployeeManagementNotes] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NULL,
        [BranchId] int NOT NULL,
        [SystemNote] nvarchar(2000) NULL,
        [ManagerNote] nvarchar(2000) NULL,
        [UpdatedByUserId] nvarchar(450) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_EmployeeManagementNotes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EmployeeManagementNotes_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_EmployeeManagementNotes_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    CREATE TABLE [ManagementAuditLogs] (
        [Id] bigint NOT NULL IDENTITY,
        [UserId] nvarchar(450) NULL,
        [UserName] nvarchar(120) NULL,
        [Area] nvarchar(30) NOT NULL,
        [HttpMethod] nvarchar(20) NOT NULL,
        [Action] nvarchar(120) NOT NULL,
        [EntityType] nvarchar(120) NULL,
        [EntityId] nvarchar(120) NULL,
        [BranchId] int NULL,
        [Details] nvarchar(1000) NULL,
        [IpAddress] nvarchar(64) NULL,
        [Succeeded] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ManagementAuditLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    CREATE TABLE [SystemNotifications] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [Title] nvarchar(160) NOT NULL,
        [Message] nvarchar(1000) NOT NULL,
        [Link] nvarchar(500) NULL,
        [Type] nvarchar(40) NULL,
        [IsRead] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_SystemNotifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SystemNotifications_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    ALTER TABLE [ShiftChangeLogs] ADD CONSTRAINT [FK_ShiftChangeLogs_WorkShifts_WorkShiftId] FOREIGN KEY ([WorkShiftId]) REFERENCES [WorkShifts] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    ALTER TABLE [UserFeaturePermissions] ADD CONSTRAINT [FK_UserFeaturePermissions_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_BranchWarehouseAccesses_BranchId_WarehouseId] ON [BranchWarehouseAccesses] ([BranchId], [WarehouseId]) WHERE [BranchId] IS NOT NULL AND [WarehouseId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    CREATE INDEX [IX_BranchWarehouseAccesses_WarehouseId] ON [BranchWarehouseAccesses] ([WarehouseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    CREATE INDEX [IX_EmployeeManagementNotes_BranchId] ON [EmployeeManagementNotes] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_EmployeeManagementNotes_UserId_BranchId] ON [EmployeeManagementNotes] ([UserId], [BranchId]) WHERE [UserId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    CREATE INDEX [IX_ManagementAuditLogs_Area_CreatedAt] ON [ManagementAuditLogs] ([Area], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    CREATE INDEX [IX_SystemNotifications_UserId_IsRead_CreatedAt] ON [SystemNotifications] ([UserId], [IsRead], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    CREATE INDEX [IX_UserFeaturePermissions_BranchId] ON [UserFeaturePermissions] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_UserFeaturePermissions_UserId_FeatureKey_BranchId] ON [UserFeaturePermissions] ([UserId], [FeatureKey], [BranchId]) WHERE [UserId] IS NOT NULL AND [FeatureKey] IS NOT NULL AND [BranchId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_WorkShifts_BranchId_ShiftDate_ShiftType] ON [WorkShifts] ([BranchId], [ShiftDate], [ShiftType]) WHERE [ShiftType] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ShiftAssignments_WorkShiftId_UserId] ON [ShiftAssignments] ([WorkShiftId], [UserId]) WHERE [UserId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    INSERT INTO BranchWarehouseAccesses (BranchId, WarehouseId, IsPrimary, CreatedAt)
    SELECT BranchId, Id, 1, GETUTCDATE() FROM Warehouses w
    WHERE NOT EXISTS (SELECT 1 FROM BranchWarehouseAccesses a WHERE a.BranchId = w.BranchId AND a.WarehouseId = w.Id);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720120000_CompleteManagementPortal'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260720120000_CompleteManagementPortal', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727202106_AddInventoryIssueWorkflow'
)
BEGIN
    CREATE TABLE [InventoryIssues] (
        [Id] int NOT NULL IDENTITY,
        [IssueCode] nvarchar(40) NOT NULL,
        [BranchId] int NOT NULL,
        [WarehouseId] int NOT NULL,
        [SupplierId] int NULL,
        [IssueType] nvarchar(50) NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [RecipientName] nvarchar(150) NOT NULL,
        [RecipientPhone] nvarchar(20) NULL,
        [ReferenceCode] nvarchar(100) NULL,
        [Reason] nvarchar(500) NULL,
        [Note] nvarchar(1000) NULL,
        [CreatedByUserId] nvarchar(450) NOT NULL,
        [ConfirmedByUserId] nvarchar(450) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IssuedAt] datetime2 NULL,
        CONSTRAINT [PK_InventoryIssues] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InventoryIssues_AspNetUsers_ConfirmedByUserId] FOREIGN KEY ([ConfirmedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InventoryIssues_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InventoryIssues_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InventoryIssues_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InventoryIssues_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727202106_AddInventoryIssueWorkflow'
)
BEGIN
    CREATE TABLE [InventoryIssueDetails] (
        [Id] int NOT NULL IDENTITY,
        [InventoryIssueId] int NOT NULL,
        [InventoryItemId] int NOT NULL,
        [Quantity] int NOT NULL,
        [IssuedWeight] decimal(18,2) NOT NULL,
        [UnitCost] decimal(18,2) NOT NULL,
        [Note] nvarchar(500) NULL,
        CONSTRAINT [PK_InventoryIssueDetails] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InventoryIssueDetails_InventoryIssues_InventoryIssueId] FOREIGN KEY ([InventoryIssueId]) REFERENCES [InventoryIssues] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_InventoryIssueDetails_InventoryItems_InventoryItemId] FOREIGN KEY ([InventoryItemId]) REFERENCES [InventoryItems] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727202106_AddInventoryIssueWorkflow'
)
BEGIN
    CREATE UNIQUE INDEX [IX_InventoryIssueDetails_InventoryIssueId_InventoryItemId] ON [InventoryIssueDetails] ([InventoryIssueId], [InventoryItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727202106_AddInventoryIssueWorkflow'
)
BEGIN
    CREATE INDEX [IX_InventoryIssueDetails_InventoryItemId] ON [InventoryIssueDetails] ([InventoryItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727202106_AddInventoryIssueWorkflow'
)
BEGIN
    CREATE INDEX [IX_InventoryIssues_BranchId] ON [InventoryIssues] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727202106_AddInventoryIssueWorkflow'
)
BEGIN
    CREATE INDEX [IX_InventoryIssues_ConfirmedByUserId] ON [InventoryIssues] ([ConfirmedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727202106_AddInventoryIssueWorkflow'
)
BEGIN
    CREATE INDEX [IX_InventoryIssues_CreatedByUserId] ON [InventoryIssues] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727202106_AddInventoryIssueWorkflow'
)
BEGIN
    CREATE UNIQUE INDEX [IX_InventoryIssues_IssueCode] ON [InventoryIssues] ([IssueCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727202106_AddInventoryIssueWorkflow'
)
BEGIN
    CREATE INDEX [IX_InventoryIssues_SupplierId] ON [InventoryIssues] ([SupplierId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727202106_AddInventoryIssueWorkflow'
)
BEGIN
    CREATE INDEX [IX_InventoryIssues_WarehouseId] ON [InventoryIssues] ([WarehouseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727202106_AddInventoryIssueWorkflow'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260727202106_AddInventoryIssueWorkflow', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727212939_AddInventoryDisplayLocations'
)
BEGIN
    ALTER TABLE [Warehouses] ADD [LocationType] nvarchar(50) NOT NULL DEFAULT N'Kho lưu trữ';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727212939_AddInventoryDisplayLocations'
)
BEGIN
    ALTER TABLE [InventoryIssues] ADD [DestinationWarehouseId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727212939_AddInventoryDisplayLocations'
)
BEGIN
    CREATE INDEX [IX_InventoryIssues_DestinationWarehouseId] ON [InventoryIssues] ([DestinationWarehouseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727212939_AddInventoryDisplayLocations'
)
BEGIN
    ALTER TABLE [InventoryIssues] ADD CONSTRAINT [FK_InventoryIssues_Warehouses_DestinationWarehouseId] FOREIGN KEY ([DestinationWarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727212939_AddInventoryDisplayLocations'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260727212939_AddInventoryDisplayLocations', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727221937_ReplaceIssueRecipientWithReceiver'
)
BEGIN
    DECLARE @var13 sysname;
    SELECT @var13 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[InventoryIssues]') AND [c].[name] = N'RecipientName');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [InventoryIssues] DROP CONSTRAINT [' + @var13 + '];');
    ALTER TABLE [InventoryIssues] DROP COLUMN [RecipientName];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727221937_ReplaceIssueRecipientWithReceiver'
)
BEGIN
    DECLARE @var14 sysname;
    SELECT @var14 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[InventoryIssues]') AND [c].[name] = N'RecipientPhone');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [InventoryIssues] DROP CONSTRAINT [' + @var14 + '];');
    ALTER TABLE [InventoryIssues] DROP COLUMN [RecipientPhone];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727221937_ReplaceIssueRecipientWithReceiver'
)
BEGIN
    ALTER TABLE [InventoryIssues] ADD [ReceiverUserId] nvarchar(450) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727221937_ReplaceIssueRecipientWithReceiver'
)
BEGIN
    CREATE INDEX [IX_InventoryIssues_ReceiverUserId] ON [InventoryIssues] ([ReceiverUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727221937_ReplaceIssueRecipientWithReceiver'
)
BEGIN
    ALTER TABLE [InventoryIssues] ADD CONSTRAINT [FK_InventoryIssues_AspNetUsers_ReceiverUserId] FOREIGN KEY ([ReceiverUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727221937_ReplaceIssueRecipientWithReceiver'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260727221937_ReplaceIssueRecipientWithReceiver', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730165044_AddCustomerCareAndFeedback'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260730165044_AddCustomerCareAndFeedback', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730165724_AddCustomerCareV2'
)
BEGIN
    CREATE TABLE [CustomerFeedbacks] (
        [Id] int NOT NULL IDENTITY,
        [CustomerId] nvarchar(450) NULL,
        [CustomerName] nvarchar(100) NOT NULL,
        [CustomerPhone] nvarchar(20) NULL,
        [CustomerEmail] nvarchar(256) NULL,
        [Rating] int NOT NULL,
        [Category] nvarchar(100) NULL,
        [ProductId] int NULL,
        [ProductName] nvarchar(200) NULL,
        [Content] nvarchar(2000) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [AdminResponse] nvarchar(2000) NULL,
        [RespondedAt] datetime2 NULL,
        [RespondedByName] nvarchar(100) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_CustomerFeedbacks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CustomerFeedbacks_AspNetUsers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_CustomerFeedbacks_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730165724_AddCustomerCareV2'
)
BEGIN
    CREATE TABLE [SupportChatSessions] (
        [Id] int NOT NULL IDENTITY,
        [SessionCode] nvarchar(50) NOT NULL,
        [CustomerId] nvarchar(450) NULL,
        [CustomerName] nvarchar(100) NOT NULL,
        [CustomerPhone] nvarchar(20) NULL,
        [CustomerEmail] nvarchar(256) NULL,
        [AssignedStaffId] nvarchar(450) NULL,
        [AssignedStaffName] nvarchar(100) NULL,
        [Status] nvarchar(30) NOT NULL,
        [LastMessage] nvarchar(1000) NULL,
        [UnreadByStaffCount] int NOT NULL,
        [UnreadByCustomerCount] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_SupportChatSessions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SupportChatSessions_AspNetUsers_AssignedStaffId] FOREIGN KEY ([AssignedStaffId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_SupportChatSessions_AspNetUsers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [AspNetUsers] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730165724_AddCustomerCareV2'
)
BEGIN
    CREATE TABLE [SupportChatMessages] (
        [Id] int NOT NULL IDENTITY,
        [SupportChatSessionId] int NOT NULL,
        [SenderId] nvarchar(450) NULL,
        [SenderName] nvarchar(100) NOT NULL,
        [SenderRole] nvarchar(20) NOT NULL,
        [Message] nvarchar(max) NOT NULL,
        [IsRead] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_SupportChatMessages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SupportChatMessages_SupportChatSessions_SupportChatSessionId] FOREIGN KEY ([SupportChatSessionId]) REFERENCES [SupportChatSessions] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730165724_AddCustomerCareV2'
)
BEGIN
    CREATE INDEX [IX_CustomerFeedbacks_CustomerId] ON [CustomerFeedbacks] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730165724_AddCustomerCareV2'
)
BEGIN
    CREATE INDEX [IX_CustomerFeedbacks_ProductId] ON [CustomerFeedbacks] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730165724_AddCustomerCareV2'
)
BEGIN
    CREATE INDEX [IX_SupportChatMessages_SupportChatSessionId] ON [SupportChatMessages] ([SupportChatSessionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730165724_AddCustomerCareV2'
)
BEGIN
    CREATE INDEX [IX_SupportChatSessions_AssignedStaffId] ON [SupportChatSessions] ([AssignedStaffId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730165724_AddCustomerCareV2'
)
BEGIN
    CREATE INDEX [IX_SupportChatSessions_CustomerId] ON [SupportChatSessions] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730165724_AddCustomerCareV2'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260730165724_AddCustomerCareV2', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802155003_AddBranchIdToCustomerFeedback'
)
BEGIN
    ALTER TABLE [CustomerFeedbacks] ADD [BranchId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802155003_AddBranchIdToCustomerFeedback'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260802155003_AddBranchIdToCustomerFeedback', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806032814_AddInventoryStocktake'
)
BEGIN
    CREATE TABLE [InventoryStocktakes] (
        [Id] int NOT NULL IDENTITY,
        [StocktakeCode] nvarchar(40) NOT NULL,
        [WarehouseId] int NOT NULL,
        [CreatedByUserId] nvarchar(450) NOT NULL,
        [CountedAt] datetime2 NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [TotalLines] int NOT NULL,
        [DifferenceLines] int NOT NULL,
        [Note] nvarchar(1000) NULL,
        CONSTRAINT [PK_InventoryStocktakes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InventoryStocktakes_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InventoryStocktakes_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806032814_AddInventoryStocktake'
)
BEGIN
    CREATE TABLE [InventoryStocktakeDetails] (
        [Id] int NOT NULL IDENTITY,
        [InventoryStocktakeId] int NOT NULL,
        [InventoryItemId] int NOT NULL,
        [SystemQuantity] int NOT NULL,
        [ActualQuantity] int NOT NULL,
        [QuantityDifference] int NOT NULL,
        [SystemWeight] decimal(18,2) NULL,
        [ActualWeight] decimal(18,2) NULL,
        [WeightDifference] decimal(18,2) NULL,
        [SystemCarat] decimal(18,2) NULL,
        [ActualCarat] decimal(18,2) NULL,
        [CaratDifference] decimal(18,2) NULL,
        [DifferenceNote] nvarchar(500) NULL,
        CONSTRAINT [PK_InventoryStocktakeDetails] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InventoryStocktakeDetails_InventoryItems_InventoryItemId] FOREIGN KEY ([InventoryItemId]) REFERENCES [InventoryItems] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InventoryStocktakeDetails_InventoryStocktakes_InventoryStocktakeId] FOREIGN KEY ([InventoryStocktakeId]) REFERENCES [InventoryStocktakes] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806032814_AddInventoryStocktake'
)
BEGIN
    CREATE INDEX [IX_InventoryStocktakeDetails_InventoryItemId] ON [InventoryStocktakeDetails] ([InventoryItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806032814_AddInventoryStocktake'
)
BEGIN
    CREATE UNIQUE INDEX [IX_InventoryStocktakeDetails_InventoryStocktakeId_InventoryItemId] ON [InventoryStocktakeDetails] ([InventoryStocktakeId], [InventoryItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806032814_AddInventoryStocktake'
)
BEGIN
    CREATE INDEX [IX_InventoryStocktakes_CreatedByUserId] ON [InventoryStocktakes] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806032814_AddInventoryStocktake'
)
BEGIN
    CREATE UNIQUE INDEX [IX_InventoryStocktakes_StocktakeCode] ON [InventoryStocktakes] ([StocktakeCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806032814_AddInventoryStocktake'
)
BEGIN
    CREATE INDEX [IX_InventoryStocktakes_WarehouseId] ON [InventoryStocktakes] ([WarehouseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806032814_AddInventoryStocktake'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260806032814_AddInventoryStocktake', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE TABLE [ProductionBoms] (
        [Id] int NOT NULL IDENTITY,
        [BomCode] nvarchar(40) NOT NULL,
        [BranchId] int NOT NULL,
        [ProductId] int NOT NULL,
        [Version] nvarchar(30) NOT NULL,
        [EffectiveFrom] datetime2 NOT NULL,
        [EffectiveTo] datetime2 NULL,
        [StandardOutputQuantity] int NOT NULL,
        [StandardOutputWeight] decimal(18,4) NOT NULL,
        [ExpectedLossRate] decimal(9,4) NOT NULL,
        [EstimatedMaterialCost] decimal(18,2) NOT NULL,
        [EstimatedLaborCost] decimal(18,2) NOT NULL,
        [EstimatedOverheadCost] decimal(18,2) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [CreatedByUserId] nvarchar(450) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ApprovedByUserId] nvarchar(450) NULL,
        [ApprovedAt] datetime2 NULL,
        [UpdatedByUserId] nvarchar(450) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [Note] nvarchar(1000) NULL,
        CONSTRAINT [PK_ProductionBoms] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductionBoms_AspNetUsers_ApprovedByUserId] FOREIGN KEY ([ApprovedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionBoms_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionBoms_AspNetUsers_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionBoms_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionBoms_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE TABLE [ProductionLossPolicies] (
        [Id] int NOT NULL IDENTITY,
        [PolicyCode] nvarchar(40) NOT NULL,
        [BranchId] int NOT NULL,
        [MaterialType] nvarchar(50) NOT NULL,
        [MinimumPurityRate] decimal(9,6) NOT NULL,
        [MaximumPurityRate] decimal(9,6) NOT NULL,
        [OperationCode] nvarchar(50) NULL,
        [MaximumLossRate] decimal(9,4) NOT NULL,
        [ApprovalWeightLimit] decimal(18,4) NOT NULL,
        [ApprovalAmountLimit] decimal(18,2) NOT NULL,
        [Version] nvarchar(30) NOT NULL,
        [EffectiveFrom] datetime2 NOT NULL,
        [EffectiveTo] datetime2 NULL,
        [Status] nvarchar(30) NOT NULL,
        [CreatedByUserId] nvarchar(450) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ApprovedByUserId] nvarchar(450) NULL,
        [ApprovedAt] datetime2 NULL,
        [UpdatedByUserId] nvarchar(450) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [Note] nvarchar(1000) NULL,
        CONSTRAINT [PK_ProductionLossPolicies] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductionLossPolicies_AspNetUsers_ApprovedByUserId] FOREIGN KEY ([ApprovedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionLossPolicies_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionLossPolicies_AspNetUsers_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionLossPolicies_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE TABLE [ProductionWorkshops] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(30) NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [BranchId] int NOT NULL,
        [Address] nvarchar(300) NULL,
        [IsActive] bit NOT NULL,
        [IsProductionAuthorized] bit NOT NULL,
        [LicenseNumber] nvarchar(100) NULL,
        [LicenseValidFrom] datetime2 NULL,
        [LicenseValidTo] datetime2 NULL,
        [LicenseVerifiedAt] datetime2 NULL,
        [LicenseVerifiedByUserId] nvarchar(450) NULL,
        [CreatedByUserId] nvarchar(450) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedByUserId] nvarchar(450) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [Note] nvarchar(1000) NULL,
        CONSTRAINT [PK_ProductionWorkshops] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductionWorkshops_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionWorkshops_AspNetUsers_LicenseVerifiedByUserId] FOREIGN KEY ([LicenseVerifiedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionWorkshops_AspNetUsers_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionWorkshops_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE TABLE [RawMaterialLots] (
        [Id] int NOT NULL IDENTITY,
        [LotCode] nvarchar(40) NOT NULL,
        [BranchId] int NOT NULL,
        [WarehouseId] int NOT NULL,
        [InventoryItemId] int NOT NULL,
        [SupplierId] int NULL,
        [MaterialType] nvarchar(50) NOT NULL,
        [PurityRate] decimal(9,6) NOT NULL,
        [GrossWeight] decimal(18,4) NOT NULL,
        [FineWeight] decimal(18,4) NOT NULL,
        [AvailableWeight] decimal(18,4) NOT NULL,
        [SourceType] nvarchar(30) NOT NULL,
        [SourceReference] nvarchar(100) NULL,
        [SourceDocumentNumber] nvarchar(100) NULL,
        [UnitCost] decimal(18,2) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [QualityStatus] nvarchar(30) NOT NULL,
        [QualityNote] nvarchar(1000) NULL,
        [InspectedByUserId] nvarchar(450) NULL,
        [InspectedAt] datetime2 NULL,
        [ReleasedByUserId] nvarchar(450) NULL,
        [ReleasedAt] datetime2 NULL,
        [CreatedByUserId] nvarchar(450) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedByUserId] nvarchar(450) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [Note] nvarchar(1000) NULL,
        CONSTRAINT [PK_RawMaterialLots] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RawMaterialLots_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RawMaterialLots_AspNetUsers_InspectedByUserId] FOREIGN KEY ([InspectedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RawMaterialLots_AspNetUsers_ReleasedByUserId] FOREIGN KEY ([ReleasedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RawMaterialLots_AspNetUsers_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RawMaterialLots_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RawMaterialLots_InventoryItems_InventoryItemId] FOREIGN KEY ([InventoryItemId]) REFERENCES [InventoryItems] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RawMaterialLots_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RawMaterialLots_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE TABLE [ProductionBomItems] (
        [Id] int NOT NULL IDENTITY,
        [ProductionBomId] int NOT NULL,
        [SequenceNumber] int NOT NULL,
        [MaterialType] nvarchar(50) NOT NULL,
        [RequiredPurityRate] decimal(9,6) NOT NULL,
        [RequiredWeight] decimal(18,4) NOT NULL,
        [WasteAllowanceRate] decimal(9,4) NOT NULL,
        [EstimatedUnitCost] decimal(18,2) NOT NULL,
        [IsRecoverable] bit NOT NULL,
        [Note] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ProductionBomItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductionBomItems_ProductionBoms_ProductionBomId] FOREIGN KEY ([ProductionBomId]) REFERENCES [ProductionBoms] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE TABLE [ProductionBomOperations] (
        [Id] int NOT NULL IDENTITY,
        [ProductionBomId] int NOT NULL,
        [SequenceNumber] int NOT NULL,
        [OperationCode] nvarchar(50) NOT NULL,
        [OperationName] nvarchar(150) NOT NULL,
        [WorkCenter] nvarchar(150) NULL,
        [StandardMinutes] int NOT NULL,
        [ExpectedLossRate] decimal(9,4) NOT NULL,
        [EstimatedLaborCost] decimal(18,2) NOT NULL,
        [RequiresQualityCheck] bit NOT NULL,
        [Instruction] nvarchar(2000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ProductionBomOperations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductionBomOperations_ProductionBoms_ProductionBomId] FOREIGN KEY ([ProductionBomId]) REFERENCES [ProductionBoms] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE TABLE [CustomerJobOrders] (
        [Id] int NOT NULL IDENTITY,
        [JobOrderCode] nvarchar(40) NOT NULL,
        [BranchId] int NOT NULL,
        [WorkshopId] int NOT NULL,
        [CustomerName] nvarchar(150) NOT NULL,
        [CustomerPhone] nvarchar(20) NOT NULL,
        [CustomerIdentityReference] nvarchar(100) NULL,
        [JobType] nvarchar(30) NOT NULL,
        [MaterialType] nvarchar(50) NOT NULL,
        [InputGrossWeight] decimal(18,4) NOT NULL,
        [InputFineWeight] decimal(18,4) NOT NULL,
        [InputPurityRate] decimal(9,6) NOT NULL,
        [MaterialCondition] nvarchar(1000) NOT NULL,
        [IntakeImageUrl] nvarchar(1000) NOT NULL,
        [CustomerOwnedStorageLocation] nvarchar(200) NOT NULL,
        [AgreedLossRate] decimal(9,4) NOT NULL,
        [DesignDescription] nvarchar(2000) NOT NULL,
        [DesignImageUrl] nvarchar(1000) NULL,
        [DesignApprovalReference] nvarchar(200) NULL,
        [DesignApprovedAt] datetime2 NULL,
        [QuotedLaborCost] decimal(18,2) NOT NULL,
        [QuotedAdditionalMaterialCost] decimal(18,2) NOT NULL,
        [QuotedTotalAmount] decimal(18,2) NOT NULL,
        [DepositAmount] decimal(18,2) NOT NULL,
        [PromisedAt] datetime2 NOT NULL,
        [OutputGrossWeight] decimal(18,4) NOT NULL,
        [OutputFineWeight] decimal(18,4) NOT NULL,
        [OutputPurityRate] decimal(9,6) NOT NULL,
        [FinalAmount] decimal(18,2) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [QualityResult] nvarchar(30) NULL,
        [HandoverReceiverName] nvarchar(150) NULL,
        [HandoverEvidenceUrl] nvarchar(1000) NULL,
        [HandoverAt] datetime2 NULL,
        [HandedOverByUserId] nvarchar(450) NULL,
        [CreatedByUserId] nvarchar(450) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedByUserId] nvarchar(450) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [Note] nvarchar(2000) NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_CustomerJobOrders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CustomerJobOrders_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CustomerJobOrders_AspNetUsers_HandedOverByUserId] FOREIGN KEY ([HandedOverByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CustomerJobOrders_AspNetUsers_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CustomerJobOrders_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CustomerJobOrders_ProductionWorkshops_WorkshopId] FOREIGN KEY ([WorkshopId]) REFERENCES [ProductionWorkshops] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE TABLE [ProductionRecycleBatches] (
        [Id] int NOT NULL IDENTITY,
        [BatchCode] nvarchar(40) NOT NULL,
        [BranchId] int NOT NULL,
        [WorkshopId] int NOT NULL,
        [MaterialType] nvarchar(50) NOT NULL,
        [SourceType] nvarchar(30) NOT NULL,
        [InputGrossWeight] decimal(18,4) NOT NULL,
        [InputFineWeight] decimal(18,4) NOT NULL,
        [OutputGrossWeight] decimal(18,4) NOT NULL,
        [OutputFineWeight] decimal(18,4) NOT NULL,
        [OutputPurityRate] decimal(9,6) NOT NULL,
        [OutputRawMaterialLotId] int NULL,
        [Status] nvarchar(30) NOT NULL,
        [CreatedByUserId] nvarchar(450) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [StartedByUserId] nvarchar(450) NULL,
        [StartedAt] datetime2 NULL,
        [CompletedByUserId] nvarchar(450) NULL,
        [CompletedAt] datetime2 NULL,
        [ReleasedByUserId] nvarchar(450) NULL,
        [ReleasedAt] datetime2 NULL,
        [UpdatedByUserId] nvarchar(450) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [Note] nvarchar(1000) NULL,
        CONSTRAINT [PK_ProductionRecycleBatches] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductionRecycleBatches_AspNetUsers_CompletedByUserId] FOREIGN KEY ([CompletedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionRecycleBatches_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionRecycleBatches_AspNetUsers_ReleasedByUserId] FOREIGN KEY ([ReleasedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionRecycleBatches_AspNetUsers_StartedByUserId] FOREIGN KEY ([StartedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionRecycleBatches_AspNetUsers_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionRecycleBatches_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionRecycleBatches_ProductionWorkshops_WorkshopId] FOREIGN KEY ([WorkshopId]) REFERENCES [ProductionWorkshops] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionRecycleBatches_RawMaterialLots_OutputRawMaterialLotId] FOREIGN KEY ([OutputRawMaterialLotId]) REFERENCES [RawMaterialLots] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE TABLE [ProductionWorkOrders] (
        [Id] int NOT NULL IDENTITY,
        [WorkOrderCode] nvarchar(40) NOT NULL,
        [BranchId] int NOT NULL,
        [WorkshopId] int NOT NULL,
        [ProductionBomId] int NOT NULL,
        [ProductId] int NOT NULL,
        [CustomerJobOrderId] int NULL,
        [MaterialWarehouseId] int NOT NULL,
        [FinishedGoodsWarehouseId] int NOT NULL,
        [PlannedQuantity] int NOT NULL,
        [CompletedQuantity] int NOT NULL,
        [RejectedQuantity] int NOT NULL,
        [PlannedOutputWeight] decimal(18,4) NOT NULL,
        [ActualOutputWeight] decimal(18,4) NOT NULL,
        [ReservedMaterialWeight] decimal(18,4) NOT NULL,
        [IssuedMaterialWeight] decimal(18,4) NOT NULL,
        [ActualLossWeight] decimal(18,4) NOT NULL,
        [MaterialCost] decimal(18,2) NOT NULL,
        [LaborCost] decimal(18,2) NOT NULL,
        [OverheadCost] decimal(18,2) NOT NULL,
        [TotalCost] decimal(18,2) NOT NULL,
        [PlannedStartAt] datetime2 NOT NULL,
        [PlannedEndAt] datetime2 NULL,
        [ActualStartAt] datetime2 NULL,
        [ActualEndAt] datetime2 NULL,
        [Status] nvarchar(30) NOT NULL,
        [CurrentOperationCode] nvarchar(50) NULL,
        [ResponsibleUserId] nvarchar(450) NOT NULL,
        [CreatedByUserId] nvarchar(450) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ApprovedByUserId] nvarchar(450) NULL,
        [ApprovedAt] datetime2 NULL,
        [ClosedByUserId] nvarchar(450) NULL,
        [ClosedAt] datetime2 NULL,
        [UpdatedByUserId] nvarchar(450) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [HoldReason] nvarchar(1000) NULL,
        [Note] nvarchar(2000) NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_ProductionWorkOrders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductionWorkOrders_AspNetUsers_ApprovedByUserId] FOREIGN KEY ([ApprovedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionWorkOrders_AspNetUsers_ClosedByUserId] FOREIGN KEY ([ClosedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionWorkOrders_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionWorkOrders_AspNetUsers_ResponsibleUserId] FOREIGN KEY ([ResponsibleUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionWorkOrders_AspNetUsers_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionWorkOrders_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionWorkOrders_CustomerJobOrders_CustomerJobOrderId] FOREIGN KEY ([CustomerJobOrderId]) REFERENCES [CustomerJobOrders] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionWorkOrders_ProductionBoms_ProductionBomId] FOREIGN KEY ([ProductionBomId]) REFERENCES [ProductionBoms] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionWorkOrders_ProductionWorkshops_WorkshopId] FOREIGN KEY ([WorkshopId]) REFERENCES [ProductionWorkshops] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionWorkOrders_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionWorkOrders_Warehouses_FinishedGoodsWarehouseId] FOREIGN KEY ([FinishedGoodsWarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionWorkOrders_Warehouses_MaterialWarehouseId] FOREIGN KEY ([MaterialWarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE TABLE [ProductionMaterialReservations] (
        [Id] int NOT NULL IDENTITY,
        [ProductionWorkOrderId] int NOT NULL,
        [RawMaterialLotId] int NOT NULL,
        [ReservedWeight] decimal(18,4) NOT NULL,
        [IssuedWeight] decimal(18,4) NOT NULL,
        [ReturnedWeight] decimal(18,4) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [ProductionIssueTransactionId] int NULL,
        [ReturnTransactionId] int NULL,
        [ReservedByUserId] nvarchar(450) NOT NULL,
        [ReservedAt] datetime2 NOT NULL,
        [IssuedByUserId] nvarchar(450) NULL,
        [IssuedAt] datetime2 NULL,
        [ReleasedByUserId] nvarchar(450) NULL,
        [ReleasedAt] datetime2 NULL,
        [Note] nvarchar(500) NULL,
        CONSTRAINT [PK_ProductionMaterialReservations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductionMaterialReservations_AspNetUsers_IssuedByUserId] FOREIGN KEY ([IssuedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionMaterialReservations_AspNetUsers_ReleasedByUserId] FOREIGN KEY ([ReleasedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionMaterialReservations_AspNetUsers_ReservedByUserId] FOREIGN KEY ([ReservedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionMaterialReservations_InventoryTransactions_ProductionIssueTransactionId] FOREIGN KEY ([ProductionIssueTransactionId]) REFERENCES [InventoryTransactions] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionMaterialReservations_InventoryTransactions_ReturnTransactionId] FOREIGN KEY ([ReturnTransactionId]) REFERENCES [InventoryTransactions] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionMaterialReservations_ProductionWorkOrders_ProductionWorkOrderId] FOREIGN KEY ([ProductionWorkOrderId]) REFERENCES [ProductionWorkOrders] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionMaterialReservations_RawMaterialLots_RawMaterialLotId] FOREIGN KEY ([RawMaterialLotId]) REFERENCES [RawMaterialLots] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE TABLE [ProductionOperationLogs] (
        [Id] int NOT NULL IDENTITY,
        [ProductionWorkOrderId] int NOT NULL,
        [ProductionBomOperationId] int NULL,
        [SequenceNumber] int NOT NULL,
        [OperationCode] nvarchar(50) NOT NULL,
        [OperationName] nvarchar(150) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [InputWeight] decimal(18,4) NOT NULL,
        [OutputWeight] decimal(18,4) NOT NULL,
        [ScrapWeight] decimal(18,4) NOT NULL,
        [WorkerUserId] nvarchar(450) NOT NULL,
        [StartedAt] datetime2 NOT NULL,
        [CompletedAt] datetime2 NULL,
        [CreatedByUserId] nvarchar(450) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedByUserId] nvarchar(450) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [EvidenceUrl] nvarchar(500) NULL,
        [Note] nvarchar(1000) NULL,
        CONSTRAINT [PK_ProductionOperationLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductionOperationLogs_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionOperationLogs_AspNetUsers_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionOperationLogs_AspNetUsers_WorkerUserId] FOREIGN KEY ([WorkerUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionOperationLogs_ProductionBomOperations_ProductionBomOperationId] FOREIGN KEY ([ProductionBomOperationId]) REFERENCES [ProductionBomOperations] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionOperationLogs_ProductionWorkOrders_ProductionWorkOrderId] FOREIGN KEY ([ProductionWorkOrderId]) REFERENCES [ProductionWorkOrders] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE TABLE [ProductionStatusHistories] (
        [Id] int NOT NULL IDENTITY,
        [EntityType] nvarchar(50) NOT NULL,
        [EntityId] int NOT NULL,
        [ProductionWorkOrderId] int NULL,
        [CustomerJobOrderId] int NULL,
        [ProductionRecycleBatchId] int NULL,
        [FromStatus] nvarchar(30) NULL,
        [ToStatus] nvarchar(30) NOT NULL,
        [Reason] nvarchar(1000) NOT NULL,
        [ChangedByUserId] nvarchar(450) NOT NULL,
        [ChangedAt] datetime2 NOT NULL,
        [IsSystemGenerated] bit NOT NULL,
        CONSTRAINT [PK_ProductionStatusHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductionStatusHistories_AspNetUsers_ChangedByUserId] FOREIGN KEY ([ChangedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionStatusHistories_CustomerJobOrders_CustomerJobOrderId] FOREIGN KEY ([CustomerJobOrderId]) REFERENCES [CustomerJobOrders] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionStatusHistories_ProductionRecycleBatches_ProductionRecycleBatchId] FOREIGN KEY ([ProductionRecycleBatchId]) REFERENCES [ProductionRecycleBatches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionStatusHistories_ProductionWorkOrders_ProductionWorkOrderId] FOREIGN KEY ([ProductionWorkOrderId]) REFERENCES [ProductionWorkOrders] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE TABLE [ProductionLossRecords] (
        [Id] int NOT NULL IDENTITY,
        [ProductionWorkOrderId] int NOT NULL,
        [ProductionOperationLogId] int NULL,
        [ProductionLossPolicyId] int NULL,
        [ProductionRecycleBatchId] int NULL,
        [LossType] nvarchar(30) NOT NULL,
        [LossWeight] decimal(18,4) NOT NULL,
        [LossRate] decimal(9,4) NOT NULL,
        [AllowedLossRateSnapshot] decimal(9,4) NOT NULL,
        [EstimatedLossAmount] decimal(18,2) NOT NULL,
        [IsOverTolerance] bit NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [Reason] nvarchar(1000) NOT NULL,
        [EvidenceUrl] nvarchar(500) NULL,
        [ReportedByUserId] nvarchar(450) NOT NULL,
        [ReportedAt] datetime2 NOT NULL,
        [ReviewedByUserId] nvarchar(450) NULL,
        [ReviewedAt] datetime2 NULL,
        [ReviewNote] nvarchar(1000) NULL,
        CONSTRAINT [PK_ProductionLossRecords] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductionLossRecords_AspNetUsers_ReportedByUserId] FOREIGN KEY ([ReportedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionLossRecords_AspNetUsers_ReviewedByUserId] FOREIGN KEY ([ReviewedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionLossRecords_ProductionLossPolicies_ProductionLossPolicyId] FOREIGN KEY ([ProductionLossPolicyId]) REFERENCES [ProductionLossPolicies] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionLossRecords_ProductionOperationLogs_ProductionOperationLogId] FOREIGN KEY ([ProductionOperationLogId]) REFERENCES [ProductionOperationLogs] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionLossRecords_ProductionRecycleBatches_ProductionRecycleBatchId] FOREIGN KEY ([ProductionRecycleBatchId]) REFERENCES [ProductionRecycleBatches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionLossRecords_ProductionWorkOrders_ProductionWorkOrderId] FOREIGN KEY ([ProductionWorkOrderId]) REFERENCES [ProductionWorkOrders] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE TABLE [ProductionQualityInspections] (
        [Id] int NOT NULL IDENTITY,
        [InspectionCode] nvarchar(40) NOT NULL,
        [ProductionWorkOrderId] int NULL,
        [ProductionOperationLogId] int NULL,
        [ProductionRecycleBatchId] int NULL,
        [CustomerJobOrderId] int NULL,
        [InspectionType] nvarchar(30) NOT NULL,
        [MeasuredGrossWeight] decimal(18,4) NOT NULL,
        [MeasuredFineWeight] decimal(18,4) NOT NULL,
        [MeasuredPurityRate] decimal(9,6) NOT NULL,
        [AppearanceResult] nvarchar(30) NOT NULL,
        [LabelCode] nvarchar(100) NULL,
        [Result] nvarchar(30) NOT NULL,
        [ReworkOperationCode] nvarchar(50) NULL,
        [EvidenceUrl] nvarchar(500) NULL,
        [InspectedByUserId] nvarchar(450) NOT NULL,
        [InspectedAt] datetime2 NOT NULL,
        [ApprovedByUserId] nvarchar(450) NULL,
        [ApprovedAt] datetime2 NULL,
        [Note] nvarchar(1000) NULL,
        CONSTRAINT [PK_ProductionQualityInspections] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductionQualityInspections_AspNetUsers_ApprovedByUserId] FOREIGN KEY ([ApprovedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionQualityInspections_AspNetUsers_InspectedByUserId] FOREIGN KEY ([InspectedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionQualityInspections_CustomerJobOrders_CustomerJobOrderId] FOREIGN KEY ([CustomerJobOrderId]) REFERENCES [CustomerJobOrders] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionQualityInspections_ProductionOperationLogs_ProductionOperationLogId] FOREIGN KEY ([ProductionOperationLogId]) REFERENCES [ProductionOperationLogs] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionQualityInspections_ProductionRecycleBatches_ProductionRecycleBatchId] FOREIGN KEY ([ProductionRecycleBatchId]) REFERENCES [ProductionRecycleBatches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionQualityInspections_ProductionWorkOrders_ProductionWorkOrderId] FOREIGN KEY ([ProductionWorkOrderId]) REFERENCES [ProductionWorkOrders] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE TABLE [ProductionReceipts] (
        [Id] int NOT NULL IDENTITY,
        [ReceiptCode] nvarchar(40) NOT NULL,
        [ProductionWorkOrderId] int NOT NULL,
        [ProductionQualityInspectionId] int NOT NULL,
        [WarehouseId] int NOT NULL,
        [InventoryItemId] int NULL,
        [Quantity] int NOT NULL,
        [GrossWeight] decimal(18,4) NOT NULL,
        [FineWeight] decimal(18,4) NOT NULL,
        [UnitCost] decimal(18,2) NOT NULL,
        [TotalCost] decimal(18,2) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [CreatedByUserId] nvarchar(450) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [PostedByUserId] nvarchar(450) NULL,
        [PostedAt] datetime2 NULL,
        [CancelledByUserId] nvarchar(450) NULL,
        [CancelledAt] datetime2 NULL,
        [Note] nvarchar(1000) NULL,
        CONSTRAINT [PK_ProductionReceipts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductionReceipts_AspNetUsers_CancelledByUserId] FOREIGN KEY ([CancelledByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionReceipts_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionReceipts_AspNetUsers_PostedByUserId] FOREIGN KEY ([PostedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionReceipts_InventoryItems_InventoryItemId] FOREIGN KEY ([InventoryItemId]) REFERENCES [InventoryItems] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionReceipts_ProductionQualityInspections_ProductionQualityInspectionId] FOREIGN KEY ([ProductionQualityInspectionId]) REFERENCES [ProductionQualityInspections] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionReceipts_ProductionWorkOrders_ProductionWorkOrderId] FOREIGN KEY ([ProductionWorkOrderId]) REFERENCES [ProductionWorkOrders] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductionReceipts_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_CustomerJobOrders_BranchId_Status_PromisedAt] ON [CustomerJobOrders] ([BranchId], [Status], [PromisedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_CustomerJobOrders_CreatedByUserId] ON [CustomerJobOrders] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_CustomerJobOrders_HandedOverByUserId] ON [CustomerJobOrders] ([HandedOverByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CustomerJobOrders_JobOrderCode] ON [CustomerJobOrders] ([JobOrderCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_CustomerJobOrders_UpdatedByUserId] ON [CustomerJobOrders] ([UpdatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_CustomerJobOrders_WorkshopId] ON [CustomerJobOrders] ([WorkshopId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductionBomItems_ProductionBomId_SequenceNumber] ON [ProductionBomItems] ([ProductionBomId], [SequenceNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductionBomOperations_ProductionBomId_SequenceNumber] ON [ProductionBomOperations] ([ProductionBomId], [SequenceNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionBoms_ApprovedByUserId] ON [ProductionBoms] ([ApprovedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductionBoms_BomCode_Version] ON [ProductionBoms] ([BomCode], [Version]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionBoms_BranchId_ProductId_Status] ON [ProductionBoms] ([BranchId], [ProductId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionBoms_CreatedByUserId] ON [ProductionBoms] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionBoms_ProductId] ON [ProductionBoms] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionBoms_UpdatedByUserId] ON [ProductionBoms] ([UpdatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionLossPolicies_ApprovedByUserId] ON [ProductionLossPolicies] ([ApprovedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionLossPolicies_BranchId_Status_EffectiveFrom] ON [ProductionLossPolicies] ([BranchId], [Status], [EffectiveFrom]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionLossPolicies_CreatedByUserId] ON [ProductionLossPolicies] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductionLossPolicies_PolicyCode] ON [ProductionLossPolicies] ([PolicyCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionLossPolicies_UpdatedByUserId] ON [ProductionLossPolicies] ([UpdatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionLossRecords_ProductionLossPolicyId] ON [ProductionLossRecords] ([ProductionLossPolicyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionLossRecords_ProductionOperationLogId] ON [ProductionLossRecords] ([ProductionOperationLogId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionLossRecords_ProductionRecycleBatchId] ON [ProductionLossRecords] ([ProductionRecycleBatchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionLossRecords_ProductionWorkOrderId_Status_IsOverTolerance] ON [ProductionLossRecords] ([ProductionWorkOrderId], [Status], [IsOverTolerance]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionLossRecords_ReportedByUserId] ON [ProductionLossRecords] ([ReportedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionLossRecords_ReviewedByUserId] ON [ProductionLossRecords] ([ReviewedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionMaterialReservations_IssuedByUserId] ON [ProductionMaterialReservations] ([IssuedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionMaterialReservations_ProductionIssueTransactionId] ON [ProductionMaterialReservations] ([ProductionIssueTransactionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductionMaterialReservations_ProductionWorkOrderId_RawMaterialLotId] ON [ProductionMaterialReservations] ([ProductionWorkOrderId], [RawMaterialLotId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionMaterialReservations_RawMaterialLotId] ON [ProductionMaterialReservations] ([RawMaterialLotId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionMaterialReservations_ReleasedByUserId] ON [ProductionMaterialReservations] ([ReleasedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionMaterialReservations_ReservedByUserId] ON [ProductionMaterialReservations] ([ReservedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionMaterialReservations_ReturnTransactionId] ON [ProductionMaterialReservations] ([ReturnTransactionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionOperationLogs_CreatedByUserId] ON [ProductionOperationLogs] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionOperationLogs_ProductionBomOperationId] ON [ProductionOperationLogs] ([ProductionBomOperationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionOperationLogs_ProductionWorkOrderId] ON [ProductionOperationLogs] ([ProductionWorkOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionOperationLogs_UpdatedByUserId] ON [ProductionOperationLogs] ([UpdatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionOperationLogs_WorkerUserId] ON [ProductionOperationLogs] ([WorkerUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionQualityInspections_ApprovedByUserId] ON [ProductionQualityInspections] ([ApprovedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionQualityInspections_CustomerJobOrderId] ON [ProductionQualityInspections] ([CustomerJobOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionQualityInspections_InspectedByUserId] ON [ProductionQualityInspections] ([InspectedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductionQualityInspections_InspectionCode] ON [ProductionQualityInspections] ([InspectionCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionQualityInspections_ProductionOperationLogId] ON [ProductionQualityInspections] ([ProductionOperationLogId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionQualityInspections_ProductionRecycleBatchId] ON [ProductionQualityInspections] ([ProductionRecycleBatchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionQualityInspections_ProductionWorkOrderId] ON [ProductionQualityInspections] ([ProductionWorkOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionReceipts_CancelledByUserId] ON [ProductionReceipts] ([CancelledByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionReceipts_CreatedByUserId] ON [ProductionReceipts] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionReceipts_InventoryItemId] ON [ProductionReceipts] ([InventoryItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionReceipts_PostedByUserId] ON [ProductionReceipts] ([PostedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductionReceipts_ProductionQualityInspectionId] ON [ProductionReceipts] ([ProductionQualityInspectionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductionReceipts_ProductionWorkOrderId] ON [ProductionReceipts] ([ProductionWorkOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductionReceipts_ReceiptCode] ON [ProductionReceipts] ([ReceiptCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionReceipts_WarehouseId] ON [ProductionReceipts] ([WarehouseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductionRecycleBatches_BatchCode] ON [ProductionRecycleBatches] ([BatchCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionRecycleBatches_BranchId_Status] ON [ProductionRecycleBatches] ([BranchId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionRecycleBatches_CompletedByUserId] ON [ProductionRecycleBatches] ([CompletedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionRecycleBatches_CreatedByUserId] ON [ProductionRecycleBatches] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionRecycleBatches_OutputRawMaterialLotId] ON [ProductionRecycleBatches] ([OutputRawMaterialLotId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionRecycleBatches_ReleasedByUserId] ON [ProductionRecycleBatches] ([ReleasedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionRecycleBatches_StartedByUserId] ON [ProductionRecycleBatches] ([StartedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionRecycleBatches_UpdatedByUserId] ON [ProductionRecycleBatches] ([UpdatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionRecycleBatches_WorkshopId] ON [ProductionRecycleBatches] ([WorkshopId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionStatusHistories_ChangedByUserId] ON [ProductionStatusHistories] ([ChangedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionStatusHistories_CustomerJobOrderId] ON [ProductionStatusHistories] ([CustomerJobOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionStatusHistories_EntityType_EntityId_ChangedAt] ON [ProductionStatusHistories] ([EntityType], [EntityId], [ChangedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionStatusHistories_ProductionRecycleBatchId] ON [ProductionStatusHistories] ([ProductionRecycleBatchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionStatusHistories_ProductionWorkOrderId] ON [ProductionStatusHistories] ([ProductionWorkOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionWorkOrders_ApprovedByUserId] ON [ProductionWorkOrders] ([ApprovedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionWorkOrders_BranchId_Status_PlannedStartAt] ON [ProductionWorkOrders] ([BranchId], [Status], [PlannedStartAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionWorkOrders_ClosedByUserId] ON [ProductionWorkOrders] ([ClosedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionWorkOrders_CreatedByUserId] ON [ProductionWorkOrders] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionWorkOrders_CustomerJobOrderId] ON [ProductionWorkOrders] ([CustomerJobOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionWorkOrders_FinishedGoodsWarehouseId] ON [ProductionWorkOrders] ([FinishedGoodsWarehouseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionWorkOrders_MaterialWarehouseId] ON [ProductionWorkOrders] ([MaterialWarehouseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionWorkOrders_ProductId] ON [ProductionWorkOrders] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionWorkOrders_ProductionBomId] ON [ProductionWorkOrders] ([ProductionBomId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionWorkOrders_ResponsibleUserId] ON [ProductionWorkOrders] ([ResponsibleUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionWorkOrders_UpdatedByUserId] ON [ProductionWorkOrders] ([UpdatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductionWorkOrders_WorkOrderCode] ON [ProductionWorkOrders] ([WorkOrderCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionWorkOrders_WorkshopId] ON [ProductionWorkOrders] ([WorkshopId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionWorkshops_BranchId_IsActive] ON [ProductionWorkshops] ([BranchId], [IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductionWorkshops_Code] ON [ProductionWorkshops] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionWorkshops_CreatedByUserId] ON [ProductionWorkshops] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionWorkshops_LicenseVerifiedByUserId] ON [ProductionWorkshops] ([LicenseVerifiedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_ProductionWorkshops_UpdatedByUserId] ON [ProductionWorkshops] ([UpdatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_RawMaterialLots_BranchId_Status_MaterialType] ON [RawMaterialLots] ([BranchId], [Status], [MaterialType]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_RawMaterialLots_CreatedByUserId] ON [RawMaterialLots] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_RawMaterialLots_InspectedByUserId] ON [RawMaterialLots] ([InspectedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RawMaterialLots_InventoryItemId] ON [RawMaterialLots] ([InventoryItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RawMaterialLots_LotCode] ON [RawMaterialLots] ([LotCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_RawMaterialLots_ReleasedByUserId] ON [RawMaterialLots] ([ReleasedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_RawMaterialLots_SupplierId] ON [RawMaterialLots] ([SupplierId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_RawMaterialLots_UpdatedByUserId] ON [RawMaterialLots] ([UpdatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_RawMaterialLots_WarehouseId] ON [RawMaterialLots] ([WarehouseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903073557_AddProductionModule'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260903073557_AddProductionModule', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903080700_AddProductionWipInventory'
)
BEGIN
    ALTER TABLE [ProductionWorkOrders] ADD [WipInventoryItemId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903080700_AddProductionWipInventory'
)
BEGIN
    CREATE INDEX [IX_ProductionWorkOrders_WipInventoryItemId] ON [ProductionWorkOrders] ([WipInventoryItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903080700_AddProductionWipInventory'
)
BEGIN
    ALTER TABLE [ProductionWorkOrders] ADD CONSTRAINT [FK_ProductionWorkOrders_InventoryItems_WipInventoryItemId] FOREIGN KEY ([WipInventoryItemId]) REFERENCES [InventoryItems] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903080700_AddProductionWipInventory'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260903080700_AddProductionWipInventory', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903090804_AddCustomerMaterialCustody'
)
BEGIN
    CREATE TABLE [CustomerMaterialCustodyRecords] (
        [Id] int NOT NULL IDENTITY,
        [CustomerJobOrderId] int NOT NULL,
        [BranchId] int NOT NULL,
        [MaterialType] nvarchar(50) NOT NULL,
        [InputGrossWeight] decimal(18,4) NOT NULL,
        [InputFineWeight] decimal(18,4) NOT NULL,
        [InputPurityRate] decimal(9,6) NOT NULL,
        [IssuedGrossWeight] decimal(18,4) NOT NULL,
        [OutputGrossWeight] decimal(18,4) NOT NULL,
        [OutputFineWeight] decimal(18,4) NOT NULL,
        [OutputPurityRate] decimal(9,6) NOT NULL,
        [ReturnedGrossWeight] decimal(18,4) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [StorageLocation] nvarchar(200) NOT NULL,
        [IntakeEvidenceUrl] nvarchar(1000) NOT NULL,
        [ReturnEvidenceUrl] nvarchar(1000) NULL,
        [ReturnedByUserId] nvarchar(450) NULL,
        [ReturnedAt] datetime2 NULL,
        [CreatedByUserId] nvarchar(450) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedByUserId] nvarchar(450) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [Note] nvarchar(1000) NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_CustomerMaterialCustodyRecords] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CustomerMaterialCustodyRecords_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CustomerMaterialCustodyRecords_AspNetUsers_ReturnedByUserId] FOREIGN KEY ([ReturnedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CustomerMaterialCustodyRecords_AspNetUsers_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CustomerMaterialCustodyRecords_CustomerJobOrders_CustomerJobOrderId] FOREIGN KEY ([CustomerJobOrderId]) REFERENCES [CustomerJobOrders] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903090804_AddCustomerMaterialCustody'
)
BEGIN
    CREATE INDEX [IX_CustomerMaterialCustodyRecords_CreatedByUserId] ON [CustomerMaterialCustodyRecords] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903090804_AddCustomerMaterialCustody'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CustomerMaterialCustodyRecords_CustomerJobOrderId] ON [CustomerMaterialCustodyRecords] ([CustomerJobOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903090804_AddCustomerMaterialCustody'
)
BEGIN
    CREATE INDEX [IX_CustomerMaterialCustodyRecords_ReturnedByUserId] ON [CustomerMaterialCustodyRecords] ([ReturnedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903090804_AddCustomerMaterialCustody'
)
BEGIN
    CREATE INDEX [IX_CustomerMaterialCustodyRecords_UpdatedByUserId] ON [CustomerMaterialCustodyRecords] ([UpdatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903090804_AddCustomerMaterialCustody'
)
BEGIN

    INSERT INTO [CustomerMaterialCustodyRecords]
        ([CustomerJobOrderId], [BranchId], [MaterialType], [InputGrossWeight], [InputFineWeight], [InputPurityRate],
         [IssuedGrossWeight], [OutputGrossWeight], [OutputFineWeight], [OutputPurityRate], [ReturnedGrossWeight],
         [Status], [StorageLocation], [IntakeEvidenceUrl], [CreatedByUserId], [CreatedAt], [UpdatedAt])
    SELECT
        job.[Id], job.[BranchId], job.[MaterialType], job.[InputGrossWeight], job.[InputFineWeight], job.[InputPurityRate],
        CASE WHEN job.[Status] IN ('InProduction', 'QualityChecked', 'Rework', 'ReadyForHandover', 'HandedOver') THEN job.[InputGrossWeight] ELSE 0 END,
        job.[OutputGrossWeight], job.[OutputFineWeight], job.[OutputPurityRate],
        CASE WHEN job.[Status] = 'HandedOver' THEN job.[OutputGrossWeight] ELSE 0 END,
        CASE job.[Status] WHEN 'HandedOver' THEN 'Returned' WHEN 'ReadyForHandover' THEN 'ReadyForReturn' WHEN 'QualityChecked' THEN 'ReadyForReturn' WHEN 'InProduction' THEN 'InProduction' WHEN 'Rework' THEN 'InProduction' ELSE 'Held' END,
        job.[CustomerOwnedStorageLocation], job.[IntakeImageUrl], job.[CreatedByUserId], job.[CreatedAt], job.[UpdatedAt]
    FROM [CustomerJobOrders] job
    WHERE NOT EXISTS (SELECT 1 FROM [CustomerMaterialCustodyRecords] custody WHERE custody.[CustomerJobOrderId] = job.[Id]);

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903090804_AddCustomerMaterialCustody'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260903090804_AddCustomerMaterialCustody', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903092048_AddCustomerMaterialCustodyIssueWeights'
)
BEGIN
    ALTER TABLE [CustomerMaterialCustodyRecords] ADD [IssuedFineWeight] decimal(18,4) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903092048_AddCustomerMaterialCustodyIssueWeights'
)
BEGIN
    ALTER TABLE [CustomerMaterialCustodyRecords] ADD [IssuedPurityRate] decimal(9,6) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903092048_AddCustomerMaterialCustodyIssueWeights'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260903092048_AddCustomerMaterialCustodyIssueWeights', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903093248_AddProductionAuditLogs'
)
BEGIN
    CREATE TABLE [ProductionAuditLogs] (
        [Id] bigint NOT NULL IDENTITY,
        [Action] nvarchar(120) NOT NULL,
        [EntityType] nvarchar(80) NOT NULL,
        [EntityId] int NULL,
        [BranchId] int NULL,
        [ActorUserId] nvarchar(450) NOT NULL,
        [Snapshot] nvarchar(2000) NOT NULL,
        [Succeeded] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ProductionAuditLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductionAuditLogs_AspNetUsers_ActorUserId] FOREIGN KEY ([ActorUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903093248_AddProductionAuditLogs'
)
BEGIN
    CREATE INDEX [IX_ProductionAuditLogs_ActorUserId] ON [ProductionAuditLogs] ([ActorUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903093248_AddProductionAuditLogs'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260903093248_AddProductionAuditLogs', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903100505_NormalizeProductCatalog'
)
BEGIN
    ALTER TABLE [Products] ADD [Material] nvarchar(20) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903100505_NormalizeProductCatalog'
)
BEGIN
    ALTER TABLE [Products] ADD [ProductForm] nvarchar(30) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903100505_NormalizeProductCatalog'
)
BEGIN
    ALTER TABLE [Products] ADD [ProductLegalClass] nvarchar(50) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903100505_NormalizeProductCatalog'
)
BEGIN
    ALTER TABLE [Products] ADD [PurityDefinitionId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903100505_NormalizeProductCatalog'
)
BEGIN
    ALTER TABLE [Products] ADD [PurityRate] decimal(9,6) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903100505_NormalizeProductCatalog'
)
BEGIN
    ALTER TABLE [Products] ADD [UnitOfMeasure] nvarchar(20) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903100505_NormalizeProductCatalog'
)
BEGIN
    CREATE TABLE [PurityDefinitions] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(40) NOT NULL,
        [Material] nvarchar(20) NOT NULL,
        [DisplayName] nvarchar(120) NOT NULL,
        [Rate] decimal(9,6) NOT NULL,
        [Karat] decimal(5,2) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PurityDefinitions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903100505_NormalizeProductCatalog'
)
BEGIN
    CREATE TABLE [ProductSpecVersions] (
        [Id] int NOT NULL IDENTITY,
        [ProductId] int NOT NULL,
        [Version] nvarchar(30) NOT NULL,
        [Material] nvarchar(20) NOT NULL,
        [ProductForm] nvarchar(30) NOT NULL,
        [ProductLegalClass] nvarchar(50) NOT NULL,
        [PurityDefinitionId] int NULL,
        [PurityRate] decimal(9,6) NOT NULL,
        [UnitOfMeasure] nvarchar(20) NOT NULL,
        [GrossWeight] decimal(18,4) NOT NULL,
        [FineWeight] decimal(18,4) NOT NULL,
        [CreatedByUserId] nvarchar(450) NOT NULL,
        [EffectiveFrom] datetime2 NOT NULL,
        [EffectiveTo] datetime2 NULL,
        [ChangeReason] nvarchar(1000) NULL,
        CONSTRAINT [PK_ProductSpecVersions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductSpecVersions_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductSpecVersions_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductSpecVersions_PurityDefinitions_PurityDefinitionId] FOREIGN KEY ([PurityDefinitionId]) REFERENCES [PurityDefinitions] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903100505_NormalizeProductCatalog'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAt', N'DisplayName', N'IsActive', N'Karat', N'Material', N'Rate') AND [object_id] = OBJECT_ID(N'[PurityDefinitions]'))
        SET IDENTITY_INSERT [PurityDefinitions] ON;
    EXEC(N'INSERT INTO [PurityDefinitions] ([Id], [Code], [CreatedAt], [DisplayName], [IsActive], [Karat], [Material], [Rate])
    VALUES (1, N''GOLD-9999'', ''2025-01-01T00:00:00.0000000Z'', N''Vàng 9999 (24K)'', CAST(1 AS bit), 24.0, N''Gold'', 0.9999),
    (2, N''GOLD-750'', ''2025-01-01T00:00:00.0000000Z'', N''Vàng 750 (18K)'', CAST(1 AS bit), 18.0, N''Gold'', 0.75),
    (3, N''GOLD-585'', ''2025-01-01T00:00:00.0000000Z'', N''Vàng 585 (14K)'', CAST(1 AS bit), 14.0, N''Gold'', 0.585),
    (4, N''SILVER-999'', ''2025-01-01T00:00:00.0000000Z'', N''Bạc 999'', CAST(1 AS bit), NULL, N''Silver'', 0.999),
    (5, N''SILVER-925'', ''2025-01-01T00:00:00.0000000Z'', N''Bạc 925'', CAST(1 AS bit), NULL, N''Silver'', 0.925),
    (6, N''DIAMOND-1000'', ''2025-01-01T00:00:00.0000000Z'', N''Kim cương (không áp dụng hàm lượng kim loại)'', CAST(1 AS bit), NULL, N''Diamond'', 1.0)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAt', N'DisplayName', N'IsActive', N'Karat', N'Material', N'Rate') AND [object_id] = OBJECT_ID(N'[PurityDefinitions]'))
        SET IDENTITY_INSERT [PurityDefinitions] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903100505_NormalizeProductCatalog'
)
BEGIN

    UPDATE Products
    SET Material = CASE
            WHEN ProductLine = 'Silver' THEN 'Silver'
            WHEN ProductLine = 'Diamond' THEN 'Diamond'
            ELSE 'Gold'
        END,
        ProductForm = CASE
            WHEN Category LIKE N'%miếng%' OR Category LIKE N'%bar%' THEN 'Bar'
            WHEN Category LIKE N'%nguyên liệu%' OR Category LIKE N'%nguyên liệu%' THEN 'RawMaterial'
            ELSE 'Jewelry'
        END,
        UnitOfMeasure = 'Tael';

    UPDATE Products
    SET ProductLegalClass = CASE
            WHEN Material = 'Silver' THEN 'SilverJewelry'
            WHEN Material = 'Diamond' THEN 'DiamondExcluded'
            ELSE 'GoldJewelry'
        END,
        PurityDefinitionId = CASE
            WHEN Material = 'Silver' AND (GoldType LIKE N'%999%' OR GoldType LIKE N'%99.9%') THEN 4
            WHEN Material = 'Silver' THEN 5
            WHEN Material = 'Diamond' THEN 6
            WHEN GoldType LIKE N'%750%' OR GoldType LIKE N'%18K%' OR GoldType LIKE N'%18 K%' THEN 2
            WHEN GoldType LIKE N'%585%' OR GoldType LIKE N'%14K%' OR GoldType LIKE N'%14 K%' THEN 3
            ELSE 1
        END;

    UPDATE product
    SET PurityRate = purity.Rate
    FROM Products product
    INNER JOIN PurityDefinitions purity ON purity.Id = product.PurityDefinitionId;

    INSERT INTO ProductSpecVersions
        (ProductId, Version, Material, ProductForm, ProductLegalClass, PurityDefinitionId, PurityRate, UnitOfMeasure, GrossWeight, FineWeight, CreatedByUserId, EffectiveFrom, ChangeReason)
    SELECT product.Id, '1.0', product.Material, product.ProductForm, product.ProductLegalClass, product.PurityDefinitionId, product.PurityRate, product.UnitOfMeasure, product.Weight, product.Weight * product.PurityRate, users.Id, product.CreatedAt, N'Khởi tạo từ dữ liệu danh mục cũ'
    FROM Products product
    CROSS JOIN (SELECT TOP 1 Id FROM AspNetUsers ORDER BY Id) users;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903100505_NormalizeProductCatalog'
)
BEGIN
    CREATE INDEX [IX_Products_PurityDefinitionId] ON [Products] ([PurityDefinitionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903100505_NormalizeProductCatalog'
)
BEGIN
    CREATE INDEX [IX_ProductSpecVersions_CreatedByUserId] ON [ProductSpecVersions] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903100505_NormalizeProductCatalog'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductSpecVersions_ProductId_Version] ON [ProductSpecVersions] ([ProductId], [Version]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903100505_NormalizeProductCatalog'
)
BEGIN
    CREATE INDEX [IX_ProductSpecVersions_PurityDefinitionId] ON [ProductSpecVersions] ([PurityDefinitionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903100505_NormalizeProductCatalog'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PurityDefinitions_Code] ON [PurityDefinitions] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903100505_NormalizeProductCatalog'
)
BEGIN
    ALTER TABLE [Products] ADD CONSTRAINT [FK_Products_PurityDefinitions_PurityDefinitionId] FOREIGN KEY ([PurityDefinitionId]) REFERENCES [PurityDefinitions] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903100505_NormalizeProductCatalog'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260903100505_NormalizeProductCatalog', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903101920_AddPricingWorkflow'
)
BEGIN
    ALTER TABLE [OrderDetails] ADD [PriceSnapshotId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903101920_AddPricingWorkflow'
)
BEGIN
    CREATE TABLE [PriceBooks] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(50) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [BranchId] int NULL,
        [Status] nvarchar(30) NOT NULL,
        [EffectiveFrom] datetime2 NOT NULL,
        [EffectiveTo] datetime2 NULL,
        [CreatedByUserId] nvarchar(450) NULL,
        [SubmittedByUserId] nvarchar(450) NULL,
        [SubmittedAt] datetime2 NULL,
        [ApprovedByUserId] nvarchar(450) NULL,
        [ApprovedAt] datetime2 NULL,
        [PublishedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [Notes] nvarchar(1000) NULL,
        CONSTRAINT [PK_PriceBooks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PriceBooks_AspNetUsers_ApprovedByUserId] FOREIGN KEY ([ApprovedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PriceBooks_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PriceBooks_AspNetUsers_SubmittedByUserId] FOREIGN KEY ([SubmittedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PriceBooks_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903101920_AddPricingWorkflow'
)
BEGIN
    CREATE TABLE [PriceSnapshots] (
        [Id] int NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [ProductId] int NOT NULL,
        [PriceBookId] int NOT NULL,
        [PriceVersionId] int NOT NULL,
        [SellUnitPrice] decimal(18,2) NOT NULL,
        [BuyUnitPrice] decimal(18,2) NOT NULL,
        [ProcessingFee] decimal(18,2) NOT NULL,
        [MaxDiscountRate] decimal(5,2) NOT NULL,
        [CapturedAt] datetime2 NOT NULL,
        [CapturedByUserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_PriceSnapshots] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PriceSnapshots_AspNetUsers_CapturedByUserId] FOREIGN KEY ([CapturedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PriceSnapshots_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PriceSnapshots_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903101920_AddPricingWorkflow'
)
BEGIN
    CREATE TABLE [PriceVersions] (
        [Id] int NOT NULL IDENTITY,
        [PriceBookId] int NOT NULL,
        [Version] nvarchar(30) NOT NULL,
        [EffectiveFrom] datetime2 NOT NULL,
        [EffectiveTo] datetime2 NULL,
        [CreatedByUserId] nvarchar(450) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ChangeReason] nvarchar(1000) NULL,
        CONSTRAINT [PK_PriceVersions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PriceVersions_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PriceVersions_PriceBooks_PriceBookId] FOREIGN KEY ([PriceBookId]) REFERENCES [PriceBooks] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903101920_AddPricingWorkflow'
)
BEGIN
    CREATE TABLE [PriceLines] (
        [Id] int NOT NULL IDENTITY,
        [PriceVersionId] int NOT NULL,
        [ProductId] int NOT NULL,
        [SellUnitPrice] decimal(18,2) NOT NULL,
        [BuyUnitPrice] decimal(18,2) NOT NULL,
        [ProcessingFee] decimal(18,2) NOT NULL,
        [MaxDiscountRate] decimal(5,2) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_PriceLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PriceLines_PriceVersions_PriceVersionId] FOREIGN KEY ([PriceVersionId]) REFERENCES [PriceVersions] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PriceLines_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903101920_AddPricingWorkflow'
)
BEGIN

    DECLARE @effectiveFrom datetime2 = DATEADD(minute, -1, SYSUTCDATETIME());
    INSERT INTO PriceBooks (Code, Name, BranchId, Status, EffectiveFrom, EffectiveTo, CreatedByUserId, SubmittedByUserId, SubmittedAt, ApprovedByUserId, ApprovedAt, PublishedAt, CreatedAt, Notes)
    VALUES ('PB-LEGACY', N'Bảng giá chuyển đổi từ dữ liệu sản phẩm', NULL, 'Published', @effectiveFrom, NULL, NULL, NULL, NULL, NULL, NULL, @effectiveFrom, @effectiveFrom, N'Bảng giá khởi tạo tự động từ giá tham khảo hiện hữu');
    DECLARE @bookId int = CONVERT(int, SCOPE_IDENTITY());
    INSERT INTO PriceVersions (PriceBookId, Version, EffectiveFrom, EffectiveTo, CreatedByUserId, CreatedAt, ChangeReason)
    VALUES (@bookId, '1.0', @effectiveFrom, NULL, NULL, @effectiveFrom, N'Chuyển đổi từ Product.SellPrice và Product.BuyPrice');
    DECLARE @versionId int = CONVERT(int, SCOPE_IDENTITY());
    INSERT INTO PriceLines (PriceVersionId, ProductId, SellUnitPrice, BuyUnitPrice, ProcessingFee, MaxDiscountRate, IsActive)
    SELECT @versionId, Id, SellPrice, BuyPrice, ProcessingFee, 0, 1 FROM Products;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903101920_AddPricingWorkflow'
)
BEGIN
    CREATE INDEX [IX_OrderDetails_PriceSnapshotId] ON [OrderDetails] ([PriceSnapshotId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903101920_AddPricingWorkflow'
)
BEGIN
    CREATE INDEX [IX_PriceBooks_ApprovedByUserId] ON [PriceBooks] ([ApprovedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903101920_AddPricingWorkflow'
)
BEGIN
    CREATE INDEX [IX_PriceBooks_BranchId] ON [PriceBooks] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903101920_AddPricingWorkflow'
)
BEGIN
    CREATE INDEX [IX_PriceBooks_CreatedByUserId] ON [PriceBooks] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903101920_AddPricingWorkflow'
)
BEGIN
    CREATE INDEX [IX_PriceBooks_SubmittedByUserId] ON [PriceBooks] ([SubmittedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903101920_AddPricingWorkflow'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PriceLines_PriceVersionId_ProductId] ON [PriceLines] ([PriceVersionId], [ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903101920_AddPricingWorkflow'
)
BEGIN
    CREATE INDEX [IX_PriceLines_ProductId] ON [PriceLines] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903101920_AddPricingWorkflow'
)
BEGIN
    CREATE INDEX [IX_PriceSnapshots_CapturedByUserId] ON [PriceSnapshots] ([CapturedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903101920_AddPricingWorkflow'
)
BEGIN
    CREATE INDEX [IX_PriceSnapshots_OrderId] ON [PriceSnapshots] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903101920_AddPricingWorkflow'
)
BEGIN
    CREATE INDEX [IX_PriceSnapshots_ProductId] ON [PriceSnapshots] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903101920_AddPricingWorkflow'
)
BEGIN
    CREATE INDEX [IX_PriceVersions_CreatedByUserId] ON [PriceVersions] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903101920_AddPricingWorkflow'
)
BEGIN
    CREATE INDEX [IX_PriceVersions_PriceBookId] ON [PriceVersions] ([PriceBookId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903101920_AddPricingWorkflow'
)
BEGIN
    ALTER TABLE [OrderDetails] ADD CONSTRAINT [FK_OrderDetails_PriceSnapshots_PriceSnapshotId] FOREIGN KEY ([PriceSnapshotId]) REFERENCES [PriceSnapshots] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903101920_AddPricingWorkflow'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260903101920_AddPricingWorkflow', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102850_AddGoldBarCompliance'
)
BEGIN
    ALTER TABLE [PriceBooks] ADD [Scope] nvarchar(30) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102850_AddGoldBarCompliance'
)
BEGIN
    UPDATE PriceBooks SET Scope = 'General' WHERE Scope = '' OR Scope IS NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102850_AddGoldBarCompliance'
)
BEGIN
    CREATE TABLE [BusinessLocations] (
        [Id] int NOT NULL IDENTITY,
        [BranchId] int NOT NULL,
        [Code] nvarchar(30) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Address] nvarchar(500) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_BusinessLocations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BusinessLocations_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102850_AddGoldBarCompliance'
)
BEGIN
    CREATE TABLE [CustomerKycProfiles] (
        [Id] int NOT NULL IDENTITY,
        [FullName] nvarchar(200) NOT NULL,
        [IdentityType] nvarchar(30) NOT NULL,
        [IdentityNumber] nvarchar(50) NOT NULL,
        [TaxCode] nvarchar(30) NULL,
        [Phone] nvarchar(20) NULL,
        [Address] nvarchar(500) NULL,
        [DateOfBirth] datetime2 NULL,
        [IsVerified] bit NOT NULL,
        [VerifiedAt] datetime2 NULL,
        [VerifiedByUserId] nvarchar(450) NULL,
        [RetainUntil] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_CustomerKycProfiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CustomerKycProfiles_AspNetUsers_VerifiedByUserId] FOREIGN KEY ([VerifiedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102850_AddGoldBarCompliance'
)
BEGIN
    CREATE TABLE [BusinessLicenses] (
        [Id] int NOT NULL IDENTITY,
        [BusinessLocationId] int NOT NULL,
        [LicenseType] nvarchar(50) NOT NULL,
        [Number] nvarchar(100) NOT NULL,
        [ValidFrom] datetime2 NOT NULL,
        [ValidTo] datetime2 NULL,
        [IsVerified] bit NOT NULL,
        [VerifiedAt] datetime2 NULL,
        [VerifiedByUserId] nvarchar(450) NULL,
        CONSTRAINT [PK_BusinessLicenses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BusinessLicenses_AspNetUsers_VerifiedByUserId] FOREIGN KEY ([VerifiedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BusinessLicenses_BusinessLocations_BusinessLocationId] FOREIGN KEY ([BusinessLocationId]) REFERENCES [BusinessLocations] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102850_AddGoldBarCompliance'
)
BEGIN
    CREATE TABLE [GoldBarSerials] (
        [Id] int NOT NULL IDENTITY,
        [SerialNumber] nvarchar(100) NOT NULL,
        [ProductId] int NOT NULL,
        [BusinessLocationId] int NOT NULL,
        [PurityCode] nvarchar(50) NOT NULL,
        [GrossWeight] decimal(18,4) NOT NULL,
        [FineWeight] decimal(18,4) NOT NULL,
        [CertificateNumber] nvarchar(100) NULL,
        [Status] nvarchar(30) NOT NULL,
        [ReceivedAt] datetime2 NOT NULL,
        [RetainUntil] datetime2 NOT NULL,
        CONSTRAINT [PK_GoldBarSerials] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GoldBarSerials_BusinessLocations_BusinessLocationId] FOREIGN KEY ([BusinessLocationId]) REFERENCES [BusinessLocations] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GoldBarSerials_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102850_AddGoldBarCompliance'
)
BEGIN
    CREATE TABLE [GoldBarSaleRecords] (
        [Id] int NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [OrderDetailId] int NOT NULL,
        [GoldBarSerialId] int NOT NULL,
        [CustomerKycProfileId] int NOT NULL,
        [BusinessLocationId] int NOT NULL,
        [PriceSnapshotId] int NOT NULL,
        [SoldAt] datetime2 NOT NULL,
        [NhnnSubmissionStatus] nvarchar(30) NOT NULL,
        [NhnnReference] nvarchar(100) NULL,
        [NhnnFailureReason] nvarchar(1000) NULL,
        [CreatedByUserId] nvarchar(450) NOT NULL,
        [RetainUntil] datetime2 NOT NULL,
        CONSTRAINT [PK_GoldBarSaleRecords] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GoldBarSaleRecords_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GoldBarSaleRecords_BusinessLocations_BusinessLocationId] FOREIGN KEY ([BusinessLocationId]) REFERENCES [BusinessLocations] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GoldBarSaleRecords_CustomerKycProfiles_CustomerKycProfileId] FOREIGN KEY ([CustomerKycProfileId]) REFERENCES [CustomerKycProfiles] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GoldBarSaleRecords_GoldBarSerials_GoldBarSerialId] FOREIGN KEY ([GoldBarSerialId]) REFERENCES [GoldBarSerials] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GoldBarSaleRecords_OrderDetails_OrderDetailId] FOREIGN KEY ([OrderDetailId]) REFERENCES [OrderDetails] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GoldBarSaleRecords_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GoldBarSaleRecords_PriceSnapshots_PriceSnapshotId] FOREIGN KEY ([PriceSnapshotId]) REFERENCES [PriceSnapshots] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102850_AddGoldBarCompliance'
)
BEGIN
    CREATE INDEX [IX_BusinessLicenses_BusinessLocationId] ON [BusinessLicenses] ([BusinessLocationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102850_AddGoldBarCompliance'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BusinessLicenses_Number] ON [BusinessLicenses] ([Number]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102850_AddGoldBarCompliance'
)
BEGIN
    CREATE INDEX [IX_BusinessLicenses_VerifiedByUserId] ON [BusinessLicenses] ([VerifiedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102850_AddGoldBarCompliance'
)
BEGIN
    CREATE INDEX [IX_BusinessLocations_BranchId] ON [BusinessLocations] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102850_AddGoldBarCompliance'
)
BEGIN
    CREATE INDEX [IX_CustomerKycProfiles_VerifiedByUserId] ON [CustomerKycProfiles] ([VerifiedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102850_AddGoldBarCompliance'
)
BEGIN
    CREATE INDEX [IX_GoldBarSaleRecords_BusinessLocationId] ON [GoldBarSaleRecords] ([BusinessLocationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102850_AddGoldBarCompliance'
)
BEGIN
    CREATE INDEX [IX_GoldBarSaleRecords_CreatedByUserId] ON [GoldBarSaleRecords] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102850_AddGoldBarCompliance'
)
BEGIN
    CREATE INDEX [IX_GoldBarSaleRecords_CustomerKycProfileId] ON [GoldBarSaleRecords] ([CustomerKycProfileId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102850_AddGoldBarCompliance'
)
BEGIN
    CREATE INDEX [IX_GoldBarSaleRecords_GoldBarSerialId] ON [GoldBarSaleRecords] ([GoldBarSerialId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102850_AddGoldBarCompliance'
)
BEGIN
    CREATE INDEX [IX_GoldBarSaleRecords_OrderDetailId] ON [GoldBarSaleRecords] ([OrderDetailId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102850_AddGoldBarCompliance'
)
BEGIN
    CREATE INDEX [IX_GoldBarSaleRecords_OrderId] ON [GoldBarSaleRecords] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102850_AddGoldBarCompliance'
)
BEGIN
    CREATE INDEX [IX_GoldBarSaleRecords_PriceSnapshotId] ON [GoldBarSaleRecords] ([PriceSnapshotId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102850_AddGoldBarCompliance'
)
BEGIN
    CREATE INDEX [IX_GoldBarSerials_BusinessLocationId] ON [GoldBarSerials] ([BusinessLocationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102850_AddGoldBarCompliance'
)
BEGIN
    CREATE INDEX [IX_GoldBarSerials_ProductId] ON [GoldBarSerials] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102850_AddGoldBarCompliance'
)
BEGIN
    CREATE UNIQUE INDEX [IX_GoldBarSerials_SerialNumber] ON [GoldBarSerials] ([SerialNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102850_AddGoldBarCompliance'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260903102850_AddGoldBarCompliance', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102945_AddGoldBarSaleConstraints'
)
BEGIN
    DROP INDEX [IX_GoldBarSaleRecords_GoldBarSerialId] ON [GoldBarSaleRecords];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102945_AddGoldBarSaleConstraints'
)
BEGIN
    DROP INDEX [IX_GoldBarSaleRecords_OrderDetailId] ON [GoldBarSaleRecords];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102945_AddGoldBarSaleConstraints'
)
BEGIN
    CREATE UNIQUE INDEX [IX_GoldBarSaleRecords_GoldBarSerialId] ON [GoldBarSaleRecords] ([GoldBarSerialId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102945_AddGoldBarSaleConstraints'
)
BEGIN
    CREATE UNIQUE INDEX [IX_GoldBarSaleRecords_OrderDetailId] ON [GoldBarSaleRecords] ([OrderDetailId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903102945_AddGoldBarSaleConstraints'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260903102945_AddGoldBarSaleConstraints', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    ALTER TABLE [Orders] ADD [DiscountAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    ALTER TABLE [Orders] ADD [NetAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    ALTER TABLE [Orders] ADD [PosQuoteId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    ALTER TABLE [OrderDetails] ADD [DiscountAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    ALTER TABLE [OrderDetails] ADD [ProcessingFee] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    ALTER TABLE [InventoryIssues] ADD [OrderId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    CREATE TABLE [OrderDeliveries] (
        [Id] int NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [RecipientName] nvarchar(200) NOT NULL,
        [RecipientPhone] nvarchar(20) NOT NULL,
        [Address] nvarchar(500) NOT NULL,
        [Carrier] nvarchar(100) NULL,
        [TrackingNumber] nvarchar(100) NULL,
        [Status] nvarchar(30) NOT NULL,
        [ShippedAt] datetime2 NULL,
        [DeliveredAt] datetime2 NULL,
        [FailureReason] nvarchar(500) NULL,
        CONSTRAINT [PK_OrderDeliveries] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderDeliveries_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    CREATE TABLE [PosQuotes] (
        [Id] int NOT NULL IDENTITY,
        [QuoteNumber] nvarchar(40) NOT NULL,
        [BranchId] int NOT NULL,
        [CustomerName] nvarchar(200) NOT NULL,
        [CustomerPhone] nvarchar(20) NOT NULL,
        [Subtotal] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [CreatedByUserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_PosQuotes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PosQuotes_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PosQuotes_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    CREATE TABLE [DeliveryEvidences] (
        [Id] int NOT NULL IDENTITY,
        [OrderDeliveryId] int NOT NULL,
        [EvidenceType] nvarchar(30) NOT NULL,
        [FileUrl] nvarchar(1000) NOT NULL,
        [FileHash] nvarchar(128) NOT NULL,
        [UploadedByUserId] nvarchar(450) NOT NULL,
        [UploadedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_DeliveryEvidences] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DeliveryEvidences_AspNetUsers_UploadedByUserId] FOREIGN KEY ([UploadedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DeliveryEvidences_OrderDeliveries_OrderDeliveryId] FOREIGN KEY ([OrderDeliveryId]) REFERENCES [OrderDeliveries] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    CREATE TABLE [DiscountApprovals] (
        [Id] int NOT NULL IDENTITY,
        [PosQuoteId] int NULL,
        [OrderId] int NULL,
        [RequestedAmount] decimal(18,2) NOT NULL,
        [RequestedRate] decimal(5,2) NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [RequestedByUserId] nvarchar(450) NOT NULL,
        [ApprovedByUserId] nvarchar(450) NULL,
        [RequestedAt] datetime2 NOT NULL,
        [ApprovedAt] datetime2 NULL,
        CONSTRAINT [PK_DiscountApprovals] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DiscountApprovals_AspNetUsers_ApprovedByUserId] FOREIGN KEY ([ApprovedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DiscountApprovals_AspNetUsers_RequestedByUserId] FOREIGN KEY ([RequestedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DiscountApprovals_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DiscountApprovals_PosQuotes_PosQuoteId] FOREIGN KEY ([PosQuoteId]) REFERENCES [PosQuotes] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    CREATE TABLE [PosQuoteLines] (
        [Id] int NOT NULL IDENTITY,
        [PosQuoteId] int NOT NULL,
        [ProductId] int NOT NULL,
        [PriceSnapshotId] int NULL,
        [PriceBookId] int NOT NULL,
        [PriceVersionId] int NOT NULL,
        [Quantity] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [ProcessingFee] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_PosQuoteLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PosQuoteLines_PosQuotes_PosQuoteId] FOREIGN KEY ([PosQuoteId]) REFERENCES [PosQuotes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PosQuoteLines_PriceSnapshots_PriceSnapshotId] FOREIGN KEY ([PriceSnapshotId]) REFERENCES [PriceSnapshots] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PosQuoteLines_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    CREATE INDEX [IX_Orders_PosQuoteId] ON [Orders] ([PosQuoteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    CREATE INDEX [IX_InventoryIssues_OrderId] ON [InventoryIssues] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    CREATE INDEX [IX_DeliveryEvidences_OrderDeliveryId] ON [DeliveryEvidences] ([OrderDeliveryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    CREATE INDEX [IX_DeliveryEvidences_UploadedByUserId] ON [DeliveryEvidences] ([UploadedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    CREATE INDEX [IX_DiscountApprovals_ApprovedByUserId] ON [DiscountApprovals] ([ApprovedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_DiscountApprovals_OrderId] ON [DiscountApprovals] ([OrderId]) WHERE [OrderId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    CREATE INDEX [IX_DiscountApprovals_PosQuoteId] ON [DiscountApprovals] ([PosQuoteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    CREATE INDEX [IX_DiscountApprovals_RequestedByUserId] ON [DiscountApprovals] ([RequestedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OrderDeliveries_OrderId] ON [OrderDeliveries] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PosQuoteLines_PosQuoteId_ProductId] ON [PosQuoteLines] ([PosQuoteId], [ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    CREATE INDEX [IX_PosQuoteLines_PriceSnapshotId] ON [PosQuoteLines] ([PriceSnapshotId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    CREATE INDEX [IX_PosQuoteLines_ProductId] ON [PosQuoteLines] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    CREATE INDEX [IX_PosQuotes_BranchId] ON [PosQuotes] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    CREATE INDEX [IX_PosQuotes_CreatedByUserId] ON [PosQuotes] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    ALTER TABLE [InventoryIssues] ADD CONSTRAINT [FK_InventoryIssues_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_PosQuotes_PosQuoteId] FOREIGN KEY ([PosQuoteId]) REFERENCES [PosQuotes] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903104400_AddPosWorkflow'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260903104400_AddPosWorkflow', N'9.0.0');
END;

COMMIT;
GO

