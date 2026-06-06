using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddDiamondCatalogAndSmsFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Products",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiamondCarat",
                table: "Products",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiamondCertificate",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiamondClarity",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiamondColor",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiamondCut",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiamondShape",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiamondSize",
                table: "Products",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductLine",
                table: "Products",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DiamondCarat",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DiamondCertificate",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DiamondClarity",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DiamondColor",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DiamondCut",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DiamondShape",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DiamondSize",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductLine",
                table: "Products");
        }
    }
}
