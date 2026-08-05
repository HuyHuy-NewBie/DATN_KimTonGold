using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryDisplayLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LocationType",
                table: "Warehouses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Kho lưu trữ");

            migrationBuilder.AddColumn<int>(
                name: "DestinationWarehouseId",
                table: "InventoryIssues",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryIssues_DestinationWarehouseId",
                table: "InventoryIssues",
                column: "DestinationWarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryIssues_Warehouses_DestinationWarehouseId",
                table: "InventoryIssues",
                column: "DestinationWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryIssues_Warehouses_DestinationWarehouseId",
                table: "InventoryIssues");

            migrationBuilder.DropIndex(
                name: "IX_InventoryIssues_DestinationWarehouseId",
                table: "InventoryIssues");

            migrationBuilder.DropColumn(
                name: "LocationType",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "DestinationWarehouseId",
                table: "InventoryIssues");
        }
    }
}
