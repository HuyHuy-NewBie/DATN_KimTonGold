using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPricingWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PriceSnapshotId",
                table: "OrderDetails",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PriceBooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    SubmittedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceBooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceBooks_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PriceBooks_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PriceBooks_AspNetUsers_SubmittedByUserId",
                        column: x => x.SubmittedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PriceBooks_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PriceSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    PriceBookId = table.Column<int>(type: "int", nullable: false),
                    PriceVersionId = table.Column<int>(type: "int", nullable: false),
                    SellUnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BuyUnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProcessingFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxDiscountRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CapturedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CapturedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceSnapshots_AspNetUsers_CapturedByUserId",
                        column: x => x.CapturedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PriceSnapshots_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PriceSnapshots_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PriceVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PriceBookId = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceVersions_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PriceVersions_PriceBooks_PriceBookId",
                        column: x => x.PriceBookId,
                        principalTable: "PriceBooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PriceLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PriceVersionId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    SellUnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BuyUnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProcessingFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxDiscountRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceLines_PriceVersions_PriceVersionId",
                        column: x => x.PriceVersionId,
                        principalTable: "PriceVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PriceLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql(@"
DECLARE @effectiveFrom datetime2 = DATEADD(minute, -1, SYSUTCDATETIME());
INSERT INTO PriceBooks (Code, Name, BranchId, Status, EffectiveFrom, EffectiveTo, CreatedByUserId, SubmittedByUserId, SubmittedAt, ApprovedByUserId, ApprovedAt, PublishedAt, CreatedAt, Notes)
VALUES ('PB-LEGACY', N'Bảng giá chuyển đổi từ dữ liệu sản phẩm', NULL, 'Published', @effectiveFrom, NULL, NULL, NULL, NULL, NULL, NULL, @effectiveFrom, @effectiveFrom, N'Bảng giá khởi tạo tự động từ giá tham khảo hiện hữu');
DECLARE @bookId int = CONVERT(int, SCOPE_IDENTITY());
INSERT INTO PriceVersions (PriceBookId, Version, EffectiveFrom, EffectiveTo, CreatedByUserId, CreatedAt, ChangeReason)
VALUES (@bookId, '1.0', @effectiveFrom, NULL, NULL, @effectiveFrom, N'Chuyển đổi từ Product.SellPrice và Product.BuyPrice');
DECLARE @versionId int = CONVERT(int, SCOPE_IDENTITY());
INSERT INTO PriceLines (PriceVersionId, ProductId, SellUnitPrice, BuyUnitPrice, ProcessingFee, MaxDiscountRate, IsActive)
SELECT @versionId, Id, SellPrice, BuyPrice, ProcessingFee, 0, 1 FROM Products;");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_PriceSnapshotId",
                table: "OrderDetails",
                column: "PriceSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceBooks_ApprovedByUserId",
                table: "PriceBooks",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceBooks_BranchId",
                table: "PriceBooks",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceBooks_CreatedByUserId",
                table: "PriceBooks",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceBooks_SubmittedByUserId",
                table: "PriceBooks",
                column: "SubmittedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceLines_PriceVersionId_ProductId",
                table: "PriceLines",
                columns: new[] { "PriceVersionId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceLines_ProductId",
                table: "PriceLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceSnapshots_CapturedByUserId",
                table: "PriceSnapshots",
                column: "CapturedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceSnapshots_OrderId",
                table: "PriceSnapshots",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceSnapshots_ProductId",
                table: "PriceSnapshots",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceVersions_CreatedByUserId",
                table: "PriceVersions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceVersions_PriceBookId",
                table: "PriceVersions",
                column: "PriceBookId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_PriceSnapshots_PriceSnapshotId",
                table: "OrderDetails",
                column: "PriceSnapshotId",
                principalTable: "PriceSnapshots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_PriceSnapshots_PriceSnapshotId",
                table: "OrderDetails");

            migrationBuilder.DropTable(
                name: "PriceLines");

            migrationBuilder.DropTable(
                name: "PriceSnapshots");

            migrationBuilder.DropTable(
                name: "PriceVersions");

            migrationBuilder.DropTable(
                name: "PriceBooks");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_PriceSnapshotId",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "PriceSnapshotId",
                table: "OrderDetails");
        }
    }
}
