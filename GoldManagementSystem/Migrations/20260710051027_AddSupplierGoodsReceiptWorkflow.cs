using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierGoodsReceiptWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveredBy",
                table: "SupplierGoodsReceipts",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryDocumentNumber",
                table: "SupplierGoodsReceipts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "SupplierGoodsReceipts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "SupplierGoodsReceipts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualDiamondCarat",
                table: "SupplierGoodsReceiptDetails",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActualDiamondCertificate",
                table: "SupplierGoodsReceiptDetails",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualWeight",
                table: "SupplierGoodsReceiptDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "QualityStatus",
                table: "SupplierGoodsReceiptDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReceivingNote",
                table: "SupplierGoodsReceiptDetails",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "SupplierGoodsReceiptDetails",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Resolution",
                table: "SupplierGoodsReceiptDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplierGoodsReceipts_ReceiptCode",
                table: "SupplierGoodsReceipts",
                column: "ReceiptCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplierGoodsReceipts_WarehouseId",
                table: "SupplierGoodsReceipts",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierGoodsReceipts_Warehouses_WarehouseId",
                table: "SupplierGoodsReceipts",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupplierGoodsReceipts_Warehouses_WarehouseId",
                table: "SupplierGoodsReceipts");

            migrationBuilder.DropIndex(
                name: "IX_SupplierGoodsReceipts_ReceiptCode",
                table: "SupplierGoodsReceipts");

            migrationBuilder.DropIndex(
                name: "IX_SupplierGoodsReceipts_WarehouseId",
                table: "SupplierGoodsReceipts");

            migrationBuilder.DropColumn(
                name: "DeliveredBy",
                table: "SupplierGoodsReceipts");

            migrationBuilder.DropColumn(
                name: "DeliveryDocumentNumber",
                table: "SupplierGoodsReceipts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "SupplierGoodsReceipts");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "SupplierGoodsReceipts");

            migrationBuilder.DropColumn(
                name: "ActualDiamondCarat",
                table: "SupplierGoodsReceiptDetails");

            migrationBuilder.DropColumn(
                name: "ActualDiamondCertificate",
                table: "SupplierGoodsReceiptDetails");

            migrationBuilder.DropColumn(
                name: "ActualWeight",
                table: "SupplierGoodsReceiptDetails");

            migrationBuilder.DropColumn(
                name: "QualityStatus",
                table: "SupplierGoodsReceiptDetails");

            migrationBuilder.DropColumn(
                name: "ReceivingNote",
                table: "SupplierGoodsReceiptDetails");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "SupplierGoodsReceiptDetails");

            migrationBuilder.DropColumn(
                name: "Resolution",
                table: "SupplierGoodsReceiptDetails");
        }
    }
}
