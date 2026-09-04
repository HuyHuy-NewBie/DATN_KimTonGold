using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerMaterialCustody : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerMaterialCustodyRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerJobOrderId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    MaterialType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InputGrossWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    InputFineWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    InputPurityRate = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    IssuedGrossWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    OutputGrossWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    OutputFineWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    OutputPurityRate = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    ReturnedGrossWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    StorageLocation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IntakeEvidenceUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ReturnEvidenceUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReturnedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ReturnedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerMaterialCustodyRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerMaterialCustodyRecords_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerMaterialCustodyRecords_AspNetUsers_ReturnedByUserId",
                        column: x => x.ReturnedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerMaterialCustodyRecords_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerMaterialCustodyRecords_CustomerJobOrders_CustomerJobOrderId",
                        column: x => x.CustomerJobOrderId,
                        principalTable: "CustomerJobOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerMaterialCustodyRecords_CreatedByUserId",
                table: "CustomerMaterialCustodyRecords",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerMaterialCustodyRecords_CustomerJobOrderId",
                table: "CustomerMaterialCustodyRecords",
                column: "CustomerJobOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerMaterialCustodyRecords_ReturnedByUserId",
                table: "CustomerMaterialCustodyRecords",
                column: "ReturnedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerMaterialCustodyRecords_UpdatedByUserId",
                table: "CustomerMaterialCustodyRecords",
                column: "UpdatedByUserId");

            migrationBuilder.Sql(@"
INSERT INTO [CustomerMaterialCustodyRecords]
    ([CustomerJobOrderId], [BranchId], [MaterialType], [InputGrossWeight], [InputFineWeight], [InputPurityRate],
     [IssuedGrossWeight], [OutputGrossWeight], [OutputFineWeight], [OutputPurityRate], [ReturnedGrossWeight],
     [Status], [StorageLocation], [IntakeEvidenceUrl], [CreatedByUserId], [CreatedAt], [UpdatedAt])
SELECT
    job.[Id], job.[BranchId], job.[MaterialType], job.[InputGrossWeight], job.[InputFineWeight], job.[InputPurityRate],
    CASE WHEN job.[Status] IN ('InProduction', 'QualityChecked', 'Rework', 'ReadyForHandover', 'HandedOver') THEN job.[InputGrossWeight] ELSE 0 END,
    job.[OutputGrossWeight], job.[OutputFineWeight], job.[OutputPurityRate],
    CASE WHEN job.[Status] = 'HandedOver' THEN job.[OutputGrossWeight] ELSE 0 END,
    CASE job.[Status] WHEN 'HandedOver' THEN 'Returned' WHEN 'ReadyForHandover' THEN 'ReadyForReturn' WHEN 'QualityChecked' THEN 'ReadyForReturn' WHEN 'InProduction' THEN 'InProduction' WHEN 'Rework' THEN 'InProduction' ELSE 'Held' END,
    job.[CustomerOwnedStorageLocation], job.[IntakeImageUrl], job.[CreatedByUserId], job.[CreatedAt], job.[UpdatedAt]
FROM [CustomerJobOrders] job
WHERE NOT EXISTS (SELECT 1 FROM [CustomerMaterialCustodyRecords] custody WHERE custody.[CustomerJobOrderId] = job.[Id]);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerMaterialCustodyRecords");
        }
    }
}
