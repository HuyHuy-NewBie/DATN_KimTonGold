using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductionBoms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BomCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StandardOutputQuantity = table.Column<int>(type: "int", nullable: false),
                    StandardOutputWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ExpectedLossRate = table.Column<decimal>(type: "decimal(9,4)", nullable: false),
                    EstimatedMaterialCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstimatedLaborCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstimatedOverheadCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionBoms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionBoms_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionBoms_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionBoms_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionBoms_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionBoms_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionLossPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PolicyCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    MaterialType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MinimumPurityRate = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    MaximumPurityRate = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    OperationCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MaximumLossRate = table.Column<decimal>(type: "decimal(9,4)", nullable: false),
                    ApprovalWeightLimit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ApprovalAmountLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionLossPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionLossPolicies_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionLossPolicies_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionLossPolicies_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionLossPolicies_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionWorkshops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsProductionAuthorized = table.Column<bool>(type: "bit", nullable: false),
                    LicenseNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LicenseValidFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LicenseValidTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LicenseVerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LicenseVerifiedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionWorkshops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionWorkshops_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionWorkshops_AspNetUsers_LicenseVerifiedByUserId",
                        column: x => x.LicenseVerifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionWorkshops_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionWorkshops_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RawMaterialLots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LotCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    MaterialType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PurityRate = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    GrossWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    FineWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    AvailableWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SourceReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SourceDocumentNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    QualityStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    QualityNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    InspectedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    InspectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReleasedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ReleasedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawMaterialLots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RawMaterialLots_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RawMaterialLots_AspNetUsers_InspectedByUserId",
                        column: x => x.InspectedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RawMaterialLots_AspNetUsers_ReleasedByUserId",
                        column: x => x.ReleasedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RawMaterialLots_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RawMaterialLots_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RawMaterialLots_InventoryItems_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RawMaterialLots_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RawMaterialLots_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionBomItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionBomId = table.Column<int>(type: "int", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    MaterialType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequiredPurityRate = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    RequiredWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    WasteAllowanceRate = table.Column<decimal>(type: "decimal(9,4)", nullable: false),
                    EstimatedUnitCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsRecoverable = table.Column<bool>(type: "bit", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionBomItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionBomItems_ProductionBoms_ProductionBomId",
                        column: x => x.ProductionBomId,
                        principalTable: "ProductionBoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionBomOperations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionBomId = table.Column<int>(type: "int", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    OperationCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OperationName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    WorkCenter = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    StandardMinutes = table.Column<int>(type: "int", nullable: false),
                    ExpectedLossRate = table.Column<decimal>(type: "decimal(9,4)", nullable: false),
                    EstimatedLaborCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RequiresQualityCheck = table.Column<bool>(type: "bit", nullable: false),
                    Instruction = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionBomOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionBomOperations_ProductionBoms_ProductionBomId",
                        column: x => x.ProductionBomId,
                        principalTable: "ProductionBoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomerJobOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobOrderCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    WorkshopId = table.Column<int>(type: "int", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CustomerPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CustomerIdentityReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    JobType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MaterialType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InputGrossWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    InputFineWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    InputPurityRate = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    MaterialCondition = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IntakeImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CustomerOwnedStorageLocation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AgreedLossRate = table.Column<decimal>(type: "decimal(9,4)", nullable: false),
                    DesignDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DesignImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DesignApprovalReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DesignApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    QuotedLaborCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QuotedAdditionalMaterialCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QuotedTotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepositAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PromisedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OutputGrossWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    OutputFineWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    OutputPurityRate = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    FinalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    QualityResult = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    HandoverReceiverName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    HandoverEvidenceUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    HandoverAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HandedOverByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerJobOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerJobOrders_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerJobOrders_AspNetUsers_HandedOverByUserId",
                        column: x => x.HandedOverByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerJobOrders_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerJobOrders_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerJobOrders_ProductionWorkshops_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "ProductionWorkshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionRecycleBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    WorkshopId = table.Column<int>(type: "int", nullable: false),
                    MaterialType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    InputGrossWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    InputFineWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    OutputGrossWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    OutputFineWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    OutputPurityRate = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    OutputRawMaterialLotId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReleasedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ReleasedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionRecycleBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionRecycleBatches_AspNetUsers_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionRecycleBatches_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionRecycleBatches_AspNetUsers_ReleasedByUserId",
                        column: x => x.ReleasedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionRecycleBatches_AspNetUsers_StartedByUserId",
                        column: x => x.StartedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionRecycleBatches_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionRecycleBatches_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionRecycleBatches_ProductionWorkshops_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "ProductionWorkshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionRecycleBatches_RawMaterialLots_OutputRawMaterialLotId",
                        column: x => x.OutputRawMaterialLotId,
                        principalTable: "RawMaterialLots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionWorkOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    WorkshopId = table.Column<int>(type: "int", nullable: false),
                    ProductionBomId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CustomerJobOrderId = table.Column<int>(type: "int", nullable: true),
                    MaterialWarehouseId = table.Column<int>(type: "int", nullable: false),
                    FinishedGoodsWarehouseId = table.Column<int>(type: "int", nullable: false),
                    PlannedQuantity = table.Column<int>(type: "int", nullable: false),
                    CompletedQuantity = table.Column<int>(type: "int", nullable: false),
                    RejectedQuantity = table.Column<int>(type: "int", nullable: false),
                    PlannedOutputWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ActualOutputWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ReservedMaterialWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IssuedMaterialWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ActualLossWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MaterialCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LaborCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OverheadCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PlannedStartAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlannedEndAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualStartAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualEndAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CurrentOperationCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ResponsibleUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HoldReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionWorkOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionWorkOrders_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionWorkOrders_AspNetUsers_ClosedByUserId",
                        column: x => x.ClosedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionWorkOrders_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionWorkOrders_AspNetUsers_ResponsibleUserId",
                        column: x => x.ResponsibleUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionWorkOrders_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionWorkOrders_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionWorkOrders_CustomerJobOrders_CustomerJobOrderId",
                        column: x => x.CustomerJobOrderId,
                        principalTable: "CustomerJobOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionWorkOrders_ProductionBoms_ProductionBomId",
                        column: x => x.ProductionBomId,
                        principalTable: "ProductionBoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionWorkOrders_ProductionWorkshops_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "ProductionWorkshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionWorkOrders_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionWorkOrders_Warehouses_FinishedGoodsWarehouseId",
                        column: x => x.FinishedGoodsWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionWorkOrders_Warehouses_MaterialWarehouseId",
                        column: x => x.MaterialWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionMaterialReservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionWorkOrderId = table.Column<int>(type: "int", nullable: false),
                    RawMaterialLotId = table.Column<int>(type: "int", nullable: false),
                    ReservedWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IssuedWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ReturnedWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProductionIssueTransactionId = table.Column<int>(type: "int", nullable: true),
                    ReturnTransactionId = table.Column<int>(type: "int", nullable: true),
                    ReservedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ReservedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IssuedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReleasedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ReleasedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionMaterialReservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionMaterialReservations_AspNetUsers_IssuedByUserId",
                        column: x => x.IssuedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionMaterialReservations_AspNetUsers_ReleasedByUserId",
                        column: x => x.ReleasedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionMaterialReservations_AspNetUsers_ReservedByUserId",
                        column: x => x.ReservedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionMaterialReservations_InventoryTransactions_ProductionIssueTransactionId",
                        column: x => x.ProductionIssueTransactionId,
                        principalTable: "InventoryTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionMaterialReservations_InventoryTransactions_ReturnTransactionId",
                        column: x => x.ReturnTransactionId,
                        principalTable: "InventoryTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionMaterialReservations_ProductionWorkOrders_ProductionWorkOrderId",
                        column: x => x.ProductionWorkOrderId,
                        principalTable: "ProductionWorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionMaterialReservations_RawMaterialLots_RawMaterialLotId",
                        column: x => x.RawMaterialLotId,
                        principalTable: "RawMaterialLots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionOperationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionWorkOrderId = table.Column<int>(type: "int", nullable: false),
                    ProductionBomOperationId = table.Column<int>(type: "int", nullable: true),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    OperationCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OperationName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    InputWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    OutputWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ScrapWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    WorkerUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EvidenceUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOperationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionOperationLogs_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOperationLogs_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOperationLogs_AspNetUsers_WorkerUserId",
                        column: x => x.WorkerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOperationLogs_ProductionBomOperations_ProductionBomOperationId",
                        column: x => x.ProductionBomOperationId,
                        principalTable: "ProductionBomOperations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOperationLogs_ProductionWorkOrders_ProductionWorkOrderId",
                        column: x => x.ProductionWorkOrderId,
                        principalTable: "ProductionWorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionStatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    ProductionWorkOrderId = table.Column<int>(type: "int", nullable: true),
                    CustomerJobOrderId = table.Column<int>(type: "int", nullable: true),
                    ProductionRecycleBatchId = table.Column<int>(type: "int", nullable: true),
                    FromStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ToStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ChangedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSystemGenerated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionStatusHistories_AspNetUsers_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionStatusHistories_CustomerJobOrders_CustomerJobOrderId",
                        column: x => x.CustomerJobOrderId,
                        principalTable: "CustomerJobOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionStatusHistories_ProductionRecycleBatches_ProductionRecycleBatchId",
                        column: x => x.ProductionRecycleBatchId,
                        principalTable: "ProductionRecycleBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionStatusHistories_ProductionWorkOrders_ProductionWorkOrderId",
                        column: x => x.ProductionWorkOrderId,
                        principalTable: "ProductionWorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionLossRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionWorkOrderId = table.Column<int>(type: "int", nullable: false),
                    ProductionOperationLogId = table.Column<int>(type: "int", nullable: true),
                    ProductionLossPolicyId = table.Column<int>(type: "int", nullable: true),
                    ProductionRecycleBatchId = table.Column<int>(type: "int", nullable: true),
                    LossType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LossWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    LossRate = table.Column<decimal>(type: "decimal(9,4)", nullable: false),
                    AllowedLossRateSnapshot = table.Column<decimal>(type: "decimal(9,4)", nullable: false),
                    EstimatedLossAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsOverTolerance = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    EvidenceUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReportedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ReportedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionLossRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionLossRecords_AspNetUsers_ReportedByUserId",
                        column: x => x.ReportedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionLossRecords_AspNetUsers_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionLossRecords_ProductionLossPolicies_ProductionLossPolicyId",
                        column: x => x.ProductionLossPolicyId,
                        principalTable: "ProductionLossPolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionLossRecords_ProductionOperationLogs_ProductionOperationLogId",
                        column: x => x.ProductionOperationLogId,
                        principalTable: "ProductionOperationLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionLossRecords_ProductionRecycleBatches_ProductionRecycleBatchId",
                        column: x => x.ProductionRecycleBatchId,
                        principalTable: "ProductionRecycleBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionLossRecords_ProductionWorkOrders_ProductionWorkOrderId",
                        column: x => x.ProductionWorkOrderId,
                        principalTable: "ProductionWorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionQualityInspections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectionCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProductionWorkOrderId = table.Column<int>(type: "int", nullable: true),
                    ProductionOperationLogId = table.Column<int>(type: "int", nullable: true),
                    ProductionRecycleBatchId = table.Column<int>(type: "int", nullable: true),
                    CustomerJobOrderId = table.Column<int>(type: "int", nullable: true),
                    InspectionType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MeasuredGrossWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MeasuredFineWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MeasuredPurityRate = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    AppearanceResult = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LabelCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Result = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReworkOperationCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EvidenceUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InspectedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    InspectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionQualityInspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionQualityInspections_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionQualityInspections_AspNetUsers_InspectedByUserId",
                        column: x => x.InspectedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionQualityInspections_CustomerJobOrders_CustomerJobOrderId",
                        column: x => x.CustomerJobOrderId,
                        principalTable: "CustomerJobOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionQualityInspections_ProductionOperationLogs_ProductionOperationLogId",
                        column: x => x.ProductionOperationLogId,
                        principalTable: "ProductionOperationLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionQualityInspections_ProductionRecycleBatches_ProductionRecycleBatchId",
                        column: x => x.ProductionRecycleBatchId,
                        principalTable: "ProductionRecycleBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionQualityInspections_ProductionWorkOrders_ProductionWorkOrderId",
                        column: x => x.ProductionWorkOrderId,
                        principalTable: "ProductionWorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionReceipts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceiptCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProductionWorkOrderId = table.Column<int>(type: "int", nullable: false),
                    ProductionQualityInspectionId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    InventoryItemId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    GrossWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    FineWeight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    PostedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionReceipts_AspNetUsers_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReceipts_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReceipts_AspNetUsers_PostedByUserId",
                        column: x => x.PostedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReceipts_InventoryItems_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReceipts_ProductionQualityInspections_ProductionQualityInspectionId",
                        column: x => x.ProductionQualityInspectionId,
                        principalTable: "ProductionQualityInspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReceipts_ProductionWorkOrders_ProductionWorkOrderId",
                        column: x => x.ProductionWorkOrderId,
                        principalTable: "ProductionWorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReceipts_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerJobOrders_BranchId_Status_PromisedAt",
                table: "CustomerJobOrders",
                columns: new[] { "BranchId", "Status", "PromisedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerJobOrders_CreatedByUserId",
                table: "CustomerJobOrders",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerJobOrders_HandedOverByUserId",
                table: "CustomerJobOrders",
                column: "HandedOverByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerJobOrders_JobOrderCode",
                table: "CustomerJobOrders",
                column: "JobOrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerJobOrders_UpdatedByUserId",
                table: "CustomerJobOrders",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerJobOrders_WorkshopId",
                table: "CustomerJobOrders",
                column: "WorkshopId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionBomItems_ProductionBomId_SequenceNumber",
                table: "ProductionBomItems",
                columns: new[] { "ProductionBomId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionBomOperations_ProductionBomId_SequenceNumber",
                table: "ProductionBomOperations",
                columns: new[] { "ProductionBomId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionBoms_ApprovedByUserId",
                table: "ProductionBoms",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionBoms_BomCode_Version",
                table: "ProductionBoms",
                columns: new[] { "BomCode", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionBoms_BranchId_ProductId_Status",
                table: "ProductionBoms",
                columns: new[] { "BranchId", "ProductId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionBoms_CreatedByUserId",
                table: "ProductionBoms",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionBoms_ProductId",
                table: "ProductionBoms",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionBoms_UpdatedByUserId",
                table: "ProductionBoms",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLossPolicies_ApprovedByUserId",
                table: "ProductionLossPolicies",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLossPolicies_BranchId_Status_EffectiveFrom",
                table: "ProductionLossPolicies",
                columns: new[] { "BranchId", "Status", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLossPolicies_CreatedByUserId",
                table: "ProductionLossPolicies",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLossPolicies_PolicyCode",
                table: "ProductionLossPolicies",
                column: "PolicyCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLossPolicies_UpdatedByUserId",
                table: "ProductionLossPolicies",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLossRecords_ProductionLossPolicyId",
                table: "ProductionLossRecords",
                column: "ProductionLossPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLossRecords_ProductionOperationLogId",
                table: "ProductionLossRecords",
                column: "ProductionOperationLogId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLossRecords_ProductionRecycleBatchId",
                table: "ProductionLossRecords",
                column: "ProductionRecycleBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLossRecords_ProductionWorkOrderId_Status_IsOverTolerance",
                table: "ProductionLossRecords",
                columns: new[] { "ProductionWorkOrderId", "Status", "IsOverTolerance" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLossRecords_ReportedByUserId",
                table: "ProductionLossRecords",
                column: "ReportedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLossRecords_ReviewedByUserId",
                table: "ProductionLossRecords",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionMaterialReservations_IssuedByUserId",
                table: "ProductionMaterialReservations",
                column: "IssuedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionMaterialReservations_ProductionIssueTransactionId",
                table: "ProductionMaterialReservations",
                column: "ProductionIssueTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionMaterialReservations_ProductionWorkOrderId_RawMaterialLotId",
                table: "ProductionMaterialReservations",
                columns: new[] { "ProductionWorkOrderId", "RawMaterialLotId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionMaterialReservations_RawMaterialLotId",
                table: "ProductionMaterialReservations",
                column: "RawMaterialLotId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionMaterialReservations_ReleasedByUserId",
                table: "ProductionMaterialReservations",
                column: "ReleasedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionMaterialReservations_ReservedByUserId",
                table: "ProductionMaterialReservations",
                column: "ReservedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionMaterialReservations_ReturnTransactionId",
                table: "ProductionMaterialReservations",
                column: "ReturnTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOperationLogs_CreatedByUserId",
                table: "ProductionOperationLogs",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOperationLogs_ProductionBomOperationId",
                table: "ProductionOperationLogs",
                column: "ProductionBomOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOperationLogs_ProductionWorkOrderId",
                table: "ProductionOperationLogs",
                column: "ProductionWorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOperationLogs_UpdatedByUserId",
                table: "ProductionOperationLogs",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOperationLogs_WorkerUserId",
                table: "ProductionOperationLogs",
                column: "WorkerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionQualityInspections_ApprovedByUserId",
                table: "ProductionQualityInspections",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionQualityInspections_CustomerJobOrderId",
                table: "ProductionQualityInspections",
                column: "CustomerJobOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionQualityInspections_InspectedByUserId",
                table: "ProductionQualityInspections",
                column: "InspectedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionQualityInspections_InspectionCode",
                table: "ProductionQualityInspections",
                column: "InspectionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionQualityInspections_ProductionOperationLogId",
                table: "ProductionQualityInspections",
                column: "ProductionOperationLogId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionQualityInspections_ProductionRecycleBatchId",
                table: "ProductionQualityInspections",
                column: "ProductionRecycleBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionQualityInspections_ProductionWorkOrderId",
                table: "ProductionQualityInspections",
                column: "ProductionWorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReceipts_CancelledByUserId",
                table: "ProductionReceipts",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReceipts_CreatedByUserId",
                table: "ProductionReceipts",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReceipts_InventoryItemId",
                table: "ProductionReceipts",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReceipts_PostedByUserId",
                table: "ProductionReceipts",
                column: "PostedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReceipts_ProductionQualityInspectionId",
                table: "ProductionReceipts",
                column: "ProductionQualityInspectionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReceipts_ProductionWorkOrderId",
                table: "ProductionReceipts",
                column: "ProductionWorkOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReceipts_ReceiptCode",
                table: "ProductionReceipts",
                column: "ReceiptCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReceipts_WarehouseId",
                table: "ProductionReceipts",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionRecycleBatches_BatchCode",
                table: "ProductionRecycleBatches",
                column: "BatchCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionRecycleBatches_BranchId_Status",
                table: "ProductionRecycleBatches",
                columns: new[] { "BranchId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionRecycleBatches_CompletedByUserId",
                table: "ProductionRecycleBatches",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionRecycleBatches_CreatedByUserId",
                table: "ProductionRecycleBatches",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionRecycleBatches_OutputRawMaterialLotId",
                table: "ProductionRecycleBatches",
                column: "OutputRawMaterialLotId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionRecycleBatches_ReleasedByUserId",
                table: "ProductionRecycleBatches",
                column: "ReleasedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionRecycleBatches_StartedByUserId",
                table: "ProductionRecycleBatches",
                column: "StartedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionRecycleBatches_UpdatedByUserId",
                table: "ProductionRecycleBatches",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionRecycleBatches_WorkshopId",
                table: "ProductionRecycleBatches",
                column: "WorkshopId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStatusHistories_ChangedByUserId",
                table: "ProductionStatusHistories",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStatusHistories_CustomerJobOrderId",
                table: "ProductionStatusHistories",
                column: "CustomerJobOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStatusHistories_EntityType_EntityId_ChangedAt",
                table: "ProductionStatusHistories",
                columns: new[] { "EntityType", "EntityId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStatusHistories_ProductionRecycleBatchId",
                table: "ProductionStatusHistories",
                column: "ProductionRecycleBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStatusHistories_ProductionWorkOrderId",
                table: "ProductionStatusHistories",
                column: "ProductionWorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkOrders_ApprovedByUserId",
                table: "ProductionWorkOrders",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkOrders_BranchId_Status_PlannedStartAt",
                table: "ProductionWorkOrders",
                columns: new[] { "BranchId", "Status", "PlannedStartAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkOrders_ClosedByUserId",
                table: "ProductionWorkOrders",
                column: "ClosedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkOrders_CreatedByUserId",
                table: "ProductionWorkOrders",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkOrders_CustomerJobOrderId",
                table: "ProductionWorkOrders",
                column: "CustomerJobOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkOrders_FinishedGoodsWarehouseId",
                table: "ProductionWorkOrders",
                column: "FinishedGoodsWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkOrders_MaterialWarehouseId",
                table: "ProductionWorkOrders",
                column: "MaterialWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkOrders_ProductId",
                table: "ProductionWorkOrders",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkOrders_ProductionBomId",
                table: "ProductionWorkOrders",
                column: "ProductionBomId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkOrders_ResponsibleUserId",
                table: "ProductionWorkOrders",
                column: "ResponsibleUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkOrders_UpdatedByUserId",
                table: "ProductionWorkOrders",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkOrders_WorkOrderCode",
                table: "ProductionWorkOrders",
                column: "WorkOrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkOrders_WorkshopId",
                table: "ProductionWorkOrders",
                column: "WorkshopId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkshops_BranchId_IsActive",
                table: "ProductionWorkshops",
                columns: new[] { "BranchId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkshops_Code",
                table: "ProductionWorkshops",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkshops_CreatedByUserId",
                table: "ProductionWorkshops",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkshops_LicenseVerifiedByUserId",
                table: "ProductionWorkshops",
                column: "LicenseVerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkshops_UpdatedByUserId",
                table: "ProductionWorkshops",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterialLots_BranchId_Status_MaterialType",
                table: "RawMaterialLots",
                columns: new[] { "BranchId", "Status", "MaterialType" });

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterialLots_CreatedByUserId",
                table: "RawMaterialLots",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterialLots_InspectedByUserId",
                table: "RawMaterialLots",
                column: "InspectedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterialLots_InventoryItemId",
                table: "RawMaterialLots",
                column: "InventoryItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterialLots_LotCode",
                table: "RawMaterialLots",
                column: "LotCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterialLots_ReleasedByUserId",
                table: "RawMaterialLots",
                column: "ReleasedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterialLots_SupplierId",
                table: "RawMaterialLots",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterialLots_UpdatedByUserId",
                table: "RawMaterialLots",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterialLots_WarehouseId",
                table: "RawMaterialLots",
                column: "WarehouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionBomItems");

            migrationBuilder.DropTable(
                name: "ProductionLossRecords");

            migrationBuilder.DropTable(
                name: "ProductionMaterialReservations");

            migrationBuilder.DropTable(
                name: "ProductionReceipts");

            migrationBuilder.DropTable(
                name: "ProductionStatusHistories");

            migrationBuilder.DropTable(
                name: "ProductionLossPolicies");

            migrationBuilder.DropTable(
                name: "ProductionQualityInspections");

            migrationBuilder.DropTable(
                name: "ProductionOperationLogs");

            migrationBuilder.DropTable(
                name: "ProductionRecycleBatches");

            migrationBuilder.DropTable(
                name: "ProductionBomOperations");

            migrationBuilder.DropTable(
                name: "ProductionWorkOrders");

            migrationBuilder.DropTable(
                name: "RawMaterialLots");

            migrationBuilder.DropTable(
                name: "CustomerJobOrders");

            migrationBuilder.DropTable(
                name: "ProductionBoms");

            migrationBuilder.DropTable(
                name: "ProductionWorkshops");
        }
    }
}
