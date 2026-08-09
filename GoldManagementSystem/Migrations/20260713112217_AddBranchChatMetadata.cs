using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
//2//

namespace GoldManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchChatMetadata : Migration
    { 
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrderProcessInfo",
                table: "Branches",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductPriceInfo",
                table: "Branches",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SizeSelectionInfo",
                table: "Branches",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TradeInPolicyInfo",
                table: "Branches",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WarrantyInfo",
                table: "Branches",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderProcessInfo",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "ProductPriceInfo",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "SizeSelectionInfo",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "TradeInPolicyInfo",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "WarrantyInfo",
                table: "Branches");
        }
    }
    
}
