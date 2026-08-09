using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceIssueRecipientWithReceiver : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecipientName",
                table: "InventoryIssues");

            migrationBuilder.DropColumn(
                name: "RecipientPhone",
                table: "InventoryIssues");

            migrationBuilder.AddColumn<string>(
                name: "ReceiverUserId",
                table: "InventoryIssues",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryIssues_ReceiverUserId",
                table: "InventoryIssues",
                column: "ReceiverUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryIssues_AspNetUsers_ReceiverUserId",
                table: "InventoryIssues",
                column: "ReceiverUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryIssues_AspNetUsers_ReceiverUserId",
                table: "InventoryIssues");

            migrationBuilder.DropIndex(
                name: "IX_InventoryIssues_ReceiverUserId",
                table: "InventoryIssues");

            migrationBuilder.DropColumn(
                name: "ReceiverUserId",
                table: "InventoryIssues");

            migrationBuilder.AddColumn<string>(
                name: "RecipientName",
                table: "InventoryIssues",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RecipientPhone",
                table: "InventoryIssues",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }
    }
}
