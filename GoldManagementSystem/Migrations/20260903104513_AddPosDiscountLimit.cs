using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPosDiscountLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DiscountApprovals_OrderId",
                table: "DiscountApprovals");

            migrationBuilder.AddColumn<decimal>(
                name: "MaxDiscountRate",
                table: "PosQuoteLines",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_DiscountApprovals_OrderId",
                table: "DiscountApprovals",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DiscountApprovals_OrderId",
                table: "DiscountApprovals");

            migrationBuilder.DropColumn(
                name: "MaxDiscountRate",
                table: "PosQuoteLines");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountApprovals_OrderId",
                table: "DiscountApprovals",
                column: "OrderId",
                unique: true,
                filter: "[OrderId] IS NOT NULL");
        }
    }
}
