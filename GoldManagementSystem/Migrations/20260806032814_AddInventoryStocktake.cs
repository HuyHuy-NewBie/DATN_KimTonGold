using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryStocktake : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventoryStocktakes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StocktakeCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CountedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TotalLines = table.Column<int>(type: "int", nullable: false),
                    DifferenceLines = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryStocktakes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryStocktakes_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryStocktakes_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryStocktakeDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryStocktakeId = table.Column<int>(type: "int", nullable: false),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    SystemQuantity = table.Column<int>(type: "int", nullable: false),
                    ActualQuantity = table.Column<int>(type: "int", nullable: false),
                    QuantityDifference = table.Column<int>(type: "int", nullable: false),
                    SystemWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ActualWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    WeightDifference = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SystemCarat = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ActualCarat = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CaratDifference = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DifferenceNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryStocktakeDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryStocktakeDetails_InventoryItems_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryStocktakeDetails_InventoryStocktakes_InventoryStocktakeId",
                        column: x => x.InventoryStocktakeId,
                        principalTable: "InventoryStocktakes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocktakeDetails_InventoryItemId",
                table: "InventoryStocktakeDetails",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocktakeDetails_InventoryStocktakeId_InventoryItemId",
                table: "InventoryStocktakeDetails",
                columns: new[] { "InventoryStocktakeId", "InventoryItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocktakes_CreatedByUserId",
                table: "InventoryStocktakes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocktakes_StocktakeCode",
                table: "InventoryStocktakes",
                column: "StocktakeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocktakes_WarehouseId",
                table: "InventoryStocktakes",
                column: "WarehouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryStocktakeDetails");

            migrationBuilder.DropTable(
                name: "InventoryStocktakes");
        }
    }
}
