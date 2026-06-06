using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class SplitProductCatalogTablesByLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiamondProductCatalogEntries",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiamondProductCatalogEntries", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_DiamondProductCatalogEntries_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GoldDiamondProductCatalogEntries",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoldDiamondProductCatalogEntries", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_GoldDiamondProductCatalogEntries_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GoldProductCatalogEntries",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoldProductCatalogEntries", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_GoldProductCatalogEntries_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GoldSilverProductCatalogEntries",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoldSilverProductCatalogEntries", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_GoldSilverProductCatalogEntries_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SilverDiamondProductCatalogEntries",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SilverDiamondProductCatalogEntries", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_SilverDiamondProductCatalogEntries_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SilverProductCatalogEntries",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SilverProductCatalogEntries", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_SilverProductCatalogEntries_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(@"
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
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiamondProductCatalogEntries");

            migrationBuilder.DropTable(
                name: "GoldDiamondProductCatalogEntries");

            migrationBuilder.DropTable(
                name: "GoldProductCatalogEntries");

            migrationBuilder.DropTable(
                name: "GoldSilverProductCatalogEntries");

            migrationBuilder.DropTable(
                name: "SilverDiamondProductCatalogEntries");

            migrationBuilder.DropTable(
                name: "SilverProductCatalogEntries");
        }
    }
}
