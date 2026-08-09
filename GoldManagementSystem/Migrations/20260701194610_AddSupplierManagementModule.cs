using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierManagementModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    TaxCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    SupplierType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    PaymentTermDays = table.Column<int>(type: "int", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BankAccountName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupplierPurchaseOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierPurchaseOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierPurchaseOrders_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplierPurchaseOrders_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplierPurchaseOrders_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupplierGoodsReceipts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceiptCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SupplierPurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAcceptedValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierGoodsReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierGoodsReceipts_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplierGoodsReceipts_SupplierPurchaseOrders_SupplierPurchaseOrderId",
                        column: x => x.SupplierPurchaseOrderId,
                        principalTable: "SupplierPurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupplierPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    SupplierPurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierPayments_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplierPayments_SupplierPurchaseOrders_SupplierPurchaseOrderId",
                        column: x => x.SupplierPurchaseOrderId,
                        principalTable: "SupplierPurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplierPayments_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupplierPurchaseOrderDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierPurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    ProductLine = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(220)", maxLength: 220, nullable: false),
                    GoldType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiamondCarat = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DiamondCertificate = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReceivedQuantity = table.Column<int>(type: "int", nullable: false),
                    AcceptedQuantity = table.Column<int>(type: "int", nullable: false),
                    RejectedQuantity = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierPurchaseOrderDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierPurchaseOrderDetails_SupplierPurchaseOrders_SupplierPurchaseOrderId",
                        column: x => x.SupplierPurchaseOrderId,
                        principalTable: "SupplierPurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierGoodsReceiptDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierGoodsReceiptId = table.Column<int>(type: "int", nullable: false),
                    SupplierPurchaseOrderDetailId = table.Column<int>(type: "int", nullable: false),
                    ReceivedQuantity = table.Column<int>(type: "int", nullable: false),
                    AcceptedQuantity = table.Column<int>(type: "int", nullable: false),
                    RejectedQuantity = table.Column<int>(type: "int", nullable: false),
                    ActualUnitCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QualityNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierGoodsReceiptDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierGoodsReceiptDetails_SupplierGoodsReceipts_SupplierGoodsReceiptId",
                        column: x => x.SupplierGoodsReceiptId,
                        principalTable: "SupplierGoodsReceipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplierGoodsReceiptDetails_SupplierPurchaseOrderDetails_SupplierPurchaseOrderDetailId",
                        column: x => x.SupplierPurchaseOrderDetailId,
                        principalTable: "SupplierPurchaseOrderDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierGoodsReceiptDetails_SupplierGoodsReceiptId",
                table: "SupplierGoodsReceiptDetails",
                column: "SupplierGoodsReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierGoodsReceiptDetails_SupplierPurchaseOrderDetailId",
                table: "SupplierGoodsReceiptDetails",
                column: "SupplierPurchaseOrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierGoodsReceipts_CreatedByUserId",
                table: "SupplierGoodsReceipts",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierGoodsReceipts_SupplierPurchaseOrderId",
                table: "SupplierGoodsReceipts",
                column: "SupplierPurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPayments_CreatedByUserId",
                table: "SupplierPayments",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPayments_SupplierId",
                table: "SupplierPayments",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPayments_SupplierPurchaseOrderId",
                table: "SupplierPayments",
                column: "SupplierPurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPurchaseOrderDetails_SupplierPurchaseOrderId",
                table: "SupplierPurchaseOrderDetails",
                column: "SupplierPurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPurchaseOrders_BranchId",
                table: "SupplierPurchaseOrders",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPurchaseOrders_CreatedByUserId",
                table: "SupplierPurchaseOrders",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPurchaseOrders_SupplierId",
                table: "SupplierPurchaseOrders",
                column: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupplierGoodsReceiptDetails");

            migrationBuilder.DropTable(
                name: "SupplierPayments");

            migrationBuilder.DropTable(
                name: "SupplierGoodsReceipts");

            migrationBuilder.DropTable(
                name: "SupplierPurchaseOrderDetails");

            migrationBuilder.DropTable(
                name: "SupplierPurchaseOrders");

            migrationBuilder.DropTable(
                name: "Suppliers");
        }
    }
}
