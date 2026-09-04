using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddGoldBarSaleConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GoldBarSaleRecords_GoldBarSerialId",
                table: "GoldBarSaleRecords");

            migrationBuilder.DropIndex(
                name: "IX_GoldBarSaleRecords_OrderDetailId",
                table: "GoldBarSaleRecords");

            migrationBuilder.CreateIndex(
                name: "IX_GoldBarSaleRecords_GoldBarSerialId",
                table: "GoldBarSaleRecords",
                column: "GoldBarSerialId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoldBarSaleRecords_OrderDetailId",
                table: "GoldBarSaleRecords",
                column: "OrderDetailId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GoldBarSaleRecords_GoldBarSerialId",
                table: "GoldBarSaleRecords");

            migrationBuilder.DropIndex(
                name: "IX_GoldBarSaleRecords_OrderDetailId",
                table: "GoldBarSaleRecords");

            migrationBuilder.CreateIndex(
                name: "IX_GoldBarSaleRecords_GoldBarSerialId",
                table: "GoldBarSaleRecords",
                column: "GoldBarSerialId");

            migrationBuilder.CreateIndex(
                name: "IX_GoldBarSaleRecords_OrderDetailId",
                table: "GoldBarSaleRecords",
                column: "OrderDetailId");
        }
    }
}
