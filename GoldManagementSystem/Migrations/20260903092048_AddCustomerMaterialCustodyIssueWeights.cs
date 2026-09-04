using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerMaterialCustodyIssueWeights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "IssuedFineWeight",
                table: "CustomerMaterialCustodyRecords",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "IssuedPurityRate",
                table: "CustomerMaterialCustodyRecords",
                type: "decimal(9,6)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IssuedFineWeight",
                table: "CustomerMaterialCustodyRecords");

            migrationBuilder.DropColumn(
                name: "IssuedPurityRate",
                table: "CustomerMaterialCustodyRecords");
        }
    }
}
