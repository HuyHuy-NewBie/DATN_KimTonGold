using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GoldManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeProductCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Material",
                table: "Products",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductForm",
                table: "Products",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductLegalClass",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PurityDefinitionId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PurityRate",
                table: "Products",
                type: "decimal(9,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "UnitOfMeasure",
                table: "Products",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PurityDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Material = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    Karat = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurityDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductSpecVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Material = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProductForm = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProductLegalClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PurityDefinitionId = table.Column<int>(type: "int", nullable: true),
                    PurityRate = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GrossWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    FineWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChangeReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSpecVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductSpecVersions_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductSpecVersions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductSpecVersions_PurityDefinitions_PurityDefinitionId",
                        column: x => x.PurityDefinitionId,
                        principalTable: "PurityDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "PurityDefinitions",
                columns: new[] { "Id", "Code", "CreatedAt", "DisplayName", "IsActive", "Karat", "Material", "Rate" },
                values: new object[,]
                {
                    { 1, "GOLD-9999", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Vàng 9999 (24K)", true, 24m, "Gold", 0.9999m },
                    { 2, "GOLD-750", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Vàng 750 (18K)", true, 18m, "Gold", 0.7500m },
                    { 3, "GOLD-585", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Vàng 585 (14K)", true, 14m, "Gold", 0.5850m },
                    { 4, "SILVER-999", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bạc 999", true, null, "Silver", 0.9990m },
                    { 5, "SILVER-925", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bạc 925", true, null, "Silver", 0.9250m },
                    { 6, "DIAMOND-1000", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kim cương (không áp dụng hàm lượng kim loại)", true, null, "Diamond", 1.0000m }
                });

            migrationBuilder.Sql(@"
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
CROSS JOIN (SELECT TOP 1 Id FROM AspNetUsers ORDER BY Id) users;");

            migrationBuilder.CreateIndex(
                name: "IX_Products_PurityDefinitionId",
                table: "Products",
                column: "PurityDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSpecVersions_CreatedByUserId",
                table: "ProductSpecVersions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSpecVersions_ProductId_Version",
                table: "ProductSpecVersions",
                columns: new[] { "ProductId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductSpecVersions_PurityDefinitionId",
                table: "ProductSpecVersions",
                column: "PurityDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_PurityDefinitions_Code",
                table: "PurityDefinitions",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_PurityDefinitions_PurityDefinitionId",
                table: "Products",
                column: "PurityDefinitionId",
                principalTable: "PurityDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_PurityDefinitions_PurityDefinitionId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "ProductSpecVersions");

            migrationBuilder.DropTable(
                name: "PurityDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_Products_PurityDefinitionId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Material",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductForm",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductLegalClass",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PurityDefinitionId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PurityRate",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasure",
                table: "Products");
        }
    }
}
