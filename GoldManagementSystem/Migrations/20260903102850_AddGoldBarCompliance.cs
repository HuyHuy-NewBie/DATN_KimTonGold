using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddGoldBarCompliance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Scope",
                table: "PriceBooks",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE PriceBooks SET Scope = 'General' WHERE Scope = '' OR Scope IS NULL;");

            migrationBuilder.CreateTable(
                name: "BusinessLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessLocations_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomerKycProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IdentityType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IdentityNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TaxCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RetainUntil = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerKycProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerKycProfiles_AspNetUsers_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BusinessLicenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessLocationId = table.Column<int>(type: "int", nullable: false),
                    LicenseType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Number = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessLicenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessLicenses_AspNetUsers_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BusinessLicenses_BusinessLocations_BusinessLocationId",
                        column: x => x.BusinessLocationId,
                        principalTable: "BusinessLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GoldBarSerials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    BusinessLocationId = table.Column<int>(type: "int", nullable: false),
                    PurityCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GrossWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    FineWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CertificateNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RetainUntil = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoldBarSerials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoldBarSerials_BusinessLocations_BusinessLocationId",
                        column: x => x.BusinessLocationId,
                        principalTable: "BusinessLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoldBarSerials_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GoldBarSaleRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    OrderDetailId = table.Column<int>(type: "int", nullable: false),
                    GoldBarSerialId = table.Column<int>(type: "int", nullable: false),
                    CustomerKycProfileId = table.Column<int>(type: "int", nullable: false),
                    BusinessLocationId = table.Column<int>(type: "int", nullable: false),
                    PriceSnapshotId = table.Column<int>(type: "int", nullable: false),
                    SoldAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NhnnSubmissionStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NhnnReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NhnnFailureReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    RetainUntil = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoldBarSaleRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoldBarSaleRecords_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoldBarSaleRecords_BusinessLocations_BusinessLocationId",
                        column: x => x.BusinessLocationId,
                        principalTable: "BusinessLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoldBarSaleRecords_CustomerKycProfiles_CustomerKycProfileId",
                        column: x => x.CustomerKycProfileId,
                        principalTable: "CustomerKycProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoldBarSaleRecords_GoldBarSerials_GoldBarSerialId",
                        column: x => x.GoldBarSerialId,
                        principalTable: "GoldBarSerials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoldBarSaleRecords_OrderDetails_OrderDetailId",
                        column: x => x.OrderDetailId,
                        principalTable: "OrderDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoldBarSaleRecords_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoldBarSaleRecords_PriceSnapshots_PriceSnapshotId",
                        column: x => x.PriceSnapshotId,
                        principalTable: "PriceSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessLicenses_BusinessLocationId",
                table: "BusinessLicenses",
                column: "BusinessLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessLicenses_Number",
                table: "BusinessLicenses",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessLicenses_VerifiedByUserId",
                table: "BusinessLicenses",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessLocations_BranchId",
                table: "BusinessLocations",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerKycProfiles_VerifiedByUserId",
                table: "CustomerKycProfiles",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GoldBarSaleRecords_BusinessLocationId",
                table: "GoldBarSaleRecords",
                column: "BusinessLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_GoldBarSaleRecords_CreatedByUserId",
                table: "GoldBarSaleRecords",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GoldBarSaleRecords_CustomerKycProfileId",
                table: "GoldBarSaleRecords",
                column: "CustomerKycProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_GoldBarSaleRecords_GoldBarSerialId",
                table: "GoldBarSaleRecords",
                column: "GoldBarSerialId");

            migrationBuilder.CreateIndex(
                name: "IX_GoldBarSaleRecords_OrderDetailId",
                table: "GoldBarSaleRecords",
                column: "OrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_GoldBarSaleRecords_OrderId",
                table: "GoldBarSaleRecords",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_GoldBarSaleRecords_PriceSnapshotId",
                table: "GoldBarSaleRecords",
                column: "PriceSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_GoldBarSerials_BusinessLocationId",
                table: "GoldBarSerials",
                column: "BusinessLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_GoldBarSerials_ProductId",
                table: "GoldBarSerials",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_GoldBarSerials_SerialNumber",
                table: "GoldBarSerials",
                column: "SerialNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessLicenses");

            migrationBuilder.DropTable(
                name: "GoldBarSaleRecords");

            migrationBuilder.DropTable(
                name: "CustomerKycProfiles");

            migrationBuilder.DropTable(
                name: "GoldBarSerials");

            migrationBuilder.DropTable(
                name: "BusinessLocations");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "PriceBooks");
        }
    }
}
