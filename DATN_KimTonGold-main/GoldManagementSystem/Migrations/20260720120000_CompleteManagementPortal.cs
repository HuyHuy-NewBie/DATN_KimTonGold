using System;
using GoldManagementSystem.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldManagementSystem.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260720120000_CompleteManagementPortal")]
    public partial class CompleteManagementPortal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_WorkShifts_BranchId", table: "WorkShifts");
            migrationBuilder.DropIndex(name: "IX_ShiftAssignments_WorkShiftId", table: "ShiftAssignments");
            migrationBuilder.AddColumn<bool>(name: "IsPriority", table: "Products", type: "bit", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<int>(name: "PriorityOrder", table: "Products", type: "int", nullable: false, defaultValue: 0);
            migrationBuilder.AddColumn<string>(name: "ManagerNote", table: "WorkShifts", type: "nvarchar(1000)", maxLength: 1000, nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "UpdatedAt", table: "WorkShifts", type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()");
            migrationBuilder.AddColumn<string>(name: "ChangeType", table: "ShiftChangeLogs", type: "nvarchar(30)", maxLength: 30, nullable: true, defaultValue: "Supplemental");
            migrationBuilder.AddColumn<int>(name: "BranchId", table: "UserFeaturePermissions", type: "int", nullable: true);
            migrationBuilder.AddColumn<string>(name: "GrantedByUserId", table: "UserFeaturePermissions", type: "nvarchar(450)", maxLength: 450, nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "UpdatedAt", table: "UserFeaturePermissions", type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()");

            migrationBuilder.CreateTable(
                name: "BranchWarehouseAccesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchWarehouseAccesses", x => x.Id);
                    table.ForeignKey("FK_BranchWarehouseAccesses_Branches_BranchId", x => x.BranchId, "Branches", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_BranchWarehouseAccesses_Warehouses_WarehouseId", x => x.WarehouseId, "Warehouses", "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeManagementNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    SystemNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ManagerNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeManagementNotes", x => x.Id);
                    table.ForeignKey("FK_EmployeeManagementNotes_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_EmployeeManagementNotes_Branches_BranchId", x => x.BranchId, "Branches", "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ManagementAuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Area = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    HttpMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    EntityId = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Succeeded = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                }, constraints: table => table.PrimaryKey("PK_ManagementAuditLogs", x => x.Id));

            migrationBuilder.CreateTable(
                name: "SystemNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Link = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemNotifications", x => x.Id);
                    table.ForeignKey("FK_SystemNotifications_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddForeignKey("FK_ShiftChangeLogs_WorkShifts_WorkShiftId", "ShiftChangeLogs", "WorkShiftId", "WorkShifts", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey("FK_UserFeaturePermissions_Branches_BranchId", "UserFeaturePermissions", "BranchId", "Branches", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.CreateIndex("IX_BranchWarehouseAccesses_BranchId_WarehouseId", "BranchWarehouseAccesses", new[] { "BranchId", "WarehouseId" }, unique: true);
            migrationBuilder.CreateIndex("IX_BranchWarehouseAccesses_WarehouseId", "BranchWarehouseAccesses", "WarehouseId");
            migrationBuilder.CreateIndex("IX_EmployeeManagementNotes_BranchId", "EmployeeManagementNotes", "BranchId");
            migrationBuilder.CreateIndex("IX_EmployeeManagementNotes_UserId_BranchId", "EmployeeManagementNotes", new[] { "UserId", "BranchId" }, unique: true, filter: "[UserId] IS NOT NULL");
            migrationBuilder.CreateIndex("IX_ManagementAuditLogs_Area_CreatedAt", "ManagementAuditLogs", new[] { "Area", "CreatedAt" });
            migrationBuilder.CreateIndex("IX_SystemNotifications_UserId_IsRead_CreatedAt", "SystemNotifications", new[] { "UserId", "IsRead", "CreatedAt" });
            migrationBuilder.CreateIndex("IX_UserFeaturePermissions_BranchId", "UserFeaturePermissions", "BranchId");
            migrationBuilder.CreateIndex("IX_UserFeaturePermissions_UserId_FeatureKey_BranchId", "UserFeaturePermissions", new[] { "UserId", "FeatureKey", "BranchId" }, unique: true, filter: "[UserId] IS NOT NULL AND [FeatureKey] IS NOT NULL AND [BranchId] IS NOT NULL");
            migrationBuilder.CreateIndex("IX_WorkShifts_BranchId_ShiftDate_ShiftType", "WorkShifts", new[] { "BranchId", "ShiftDate", "ShiftType" }, unique: true, filter: "[ShiftType] IS NOT NULL");
            migrationBuilder.CreateIndex("IX_ShiftAssignments_WorkShiftId_UserId", "ShiftAssignments", new[] { "WorkShiftId", "UserId" }, unique: true, filter: "[UserId] IS NOT NULL");

            // Các kho hiện hữu mặc định thuộc và phục vụ chi nhánh sở hữu.
            migrationBuilder.Sql(@"INSERT INTO BranchWarehouseAccesses (BranchId, WarehouseId, IsPrimary, CreatedAt)
SELECT BranchId, Id, 1, GETUTCDATE() FROM Warehouses w
WHERE NOT EXISTS (SELECT 1 FROM BranchWarehouseAccesses a WHERE a.BranchId = w.BranchId AND a.WarehouseId = w.Id);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_ShiftChangeLogs_WorkShifts_WorkShiftId", "ShiftChangeLogs");
            migrationBuilder.DropForeignKey("FK_UserFeaturePermissions_Branches_BranchId", "UserFeaturePermissions");
            migrationBuilder.DropTable("BranchWarehouseAccesses");
            migrationBuilder.DropTable("EmployeeManagementNotes");
            migrationBuilder.DropTable("ManagementAuditLogs");
            migrationBuilder.DropTable("SystemNotifications");
            migrationBuilder.DropIndex("IX_ShiftAssignments_WorkShiftId_UserId", "ShiftAssignments");
            migrationBuilder.DropIndex("IX_WorkShifts_BranchId_ShiftDate_ShiftType", "WorkShifts");
            migrationBuilder.DropIndex("IX_UserFeaturePermissions_UserId_FeatureKey_BranchId", "UserFeaturePermissions");
            migrationBuilder.DropIndex("IX_UserFeaturePermissions_BranchId", "UserFeaturePermissions");
            migrationBuilder.CreateIndex(name: "IX_WorkShifts_BranchId", table: "WorkShifts", column: "BranchId");
            migrationBuilder.CreateIndex(name: "IX_ShiftAssignments_WorkShiftId", table: "ShiftAssignments", column: "WorkShiftId");
            migrationBuilder.DropColumn("IsPriority", "Products");
            migrationBuilder.DropColumn("PriorityOrder", "Products");
            migrationBuilder.DropColumn("ManagerNote", "WorkShifts");
            migrationBuilder.DropColumn("UpdatedAt", "WorkShifts");
            migrationBuilder.DropColumn("ChangeType", "ShiftChangeLogs");
            migrationBuilder.DropColumn("BranchId", "UserFeaturePermissions");
            migrationBuilder.DropColumn("GrantedByUserId", "UserFeaturePermissions");
            migrationBuilder.DropColumn("UpdatedAt", "UserFeaturePermissions");
        }
    }
}
