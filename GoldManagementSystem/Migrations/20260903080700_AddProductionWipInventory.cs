using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionWipInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WipInventoryItemId",
                table: "ProductionWorkOrders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkOrders_WipInventoryItemId",
                table: "ProductionWorkOrders",
                column: "WipInventoryItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionWorkOrders_InventoryItems_WipInventoryItemId",
                table: "ProductionWorkOrders",
                column: "WipInventoryItemId",
                principalTable: "InventoryItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionWorkOrders_InventoryItems_WipInventoryItemId",
                table: "ProductionWorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_ProductionWorkOrders_WipInventoryItemId",
                table: "ProductionWorkOrders");

            migrationBuilder.DropColumn(
                name: "WipInventoryItemId",
                table: "ProductionWorkOrders");
        }
    }
}
