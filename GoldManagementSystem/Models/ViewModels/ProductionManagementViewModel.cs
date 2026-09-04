using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GoldManagementSystem.Models.ViewModels
{
    /// <summary>
    /// Dữ liệu tổng hợp cho màn quản lý chế tác - gia công.
    /// </summary>
    public class ProductionManagementViewModel
    {
        public string SearchTerm { get; set; }

        public int? BranchId { get; set; }

        public int? WorkshopId { get; set; }

        public string StatusFilter { get; set; }

        public string ActiveTab { get; set; } = "work-orders";

        public bool CanView { get; set; }

        public bool CanOperate { get; set; }

        public bool CanApprove { get; set; }

        public bool CanManageCustomerJobs { get; set; }

        public int TotalRawMaterialLots { get; set; }

        public int ReleasedRawMaterialLots { get; set; }

        public decimal AvailableRawMaterialWeight { get; set; }

        public int ActiveWorkOrders { get; set; }

        public int OnHoldWorkOrders { get; set; }

        public int PendingLossApprovals { get; set; }

        public int PendingQualityInspections { get; set; }

        public int OpenCustomerJobs { get; set; }

        public decimal RecycleWeight { get; set; }

        public decimal WipWeight { get; set; }

        public IReadOnlyList<ProductionWorkOrder> LateWorkOrders { get; set; } = Array.Empty<ProductionWorkOrder>();

        public IReadOnlyList<ProductionLossRecord> OverToleranceLosses { get; set; } = Array.Empty<ProductionLossRecord>();

        public IReadOnlyList<ProductionWorkshop> Workshops { get; set; }
            = Array.Empty<ProductionWorkshop>();

        public IReadOnlyList<ProductionLossPolicy> LossPolicies { get; set; }
            = Array.Empty<ProductionLossPolicy>();

        public IReadOnlyList<RawMaterialLot> RawMaterialLots { get; set; }
            = Array.Empty<RawMaterialLot>();

        public IReadOnlyList<ProductionBom> Boms { get; set; }
            = Array.Empty<ProductionBom>();

        public IReadOnlyList<ProductionWorkOrder> WorkOrders { get; set; }
            = Array.Empty<ProductionWorkOrder>();

        public IReadOnlyList<ProductionLossRecord> LossRecords { get; set; }
            = Array.Empty<ProductionLossRecord>();

        public IReadOnlyList<ProductionQualityInspection> QualityInspections { get; set; }
            = Array.Empty<ProductionQualityInspection>();

        public IReadOnlyList<ProductionReceipt> Receipts { get; set; }
            = Array.Empty<ProductionReceipt>();

        public IReadOnlyList<ProductionRecycleBatch> RecycleBatches { get; set; }
            = Array.Empty<ProductionRecycleBatch>();

        public IReadOnlyList<CustomerJobOrder> CustomerJobOrders { get; set; }
            = Array.Empty<CustomerJobOrder>();

        public IReadOnlyList<CustomerMaterialCustodyRecord> CustomerMaterialCustodyRecords { get; set; }
            = Array.Empty<CustomerMaterialCustodyRecord>();

        public IReadOnlyList<ProductionStatusHistory> RecentStatusHistories { get; set; }
            = Array.Empty<ProductionStatusHistory>();

        public IReadOnlyList<SelectListItem> BranchOptions { get; set; }
            = Array.Empty<SelectListItem>();

        public IReadOnlyList<SelectListItem> WorkshopOptions { get; set; }
            = Array.Empty<SelectListItem>();

        public IReadOnlyList<SelectListItem> WarehouseOptions { get; set; }
            = Array.Empty<SelectListItem>();

        public IReadOnlyList<SelectListItem> ProductOptions { get; set; }
            = Array.Empty<SelectListItem>();

        public IReadOnlyList<SelectListItem> BomOptions { get; set; }
            = Array.Empty<SelectListItem>();

        public IReadOnlyList<SelectListItem> RawMaterialLotOptions { get; set; }
            = Array.Empty<SelectListItem>();

        public IReadOnlyList<SelectListItem> InventoryItemOptions { get; set; }
            = Array.Empty<SelectListItem>();

        public IReadOnlyList<SelectListItem> ResponsibleUserOptions { get; set; }
            = Array.Empty<SelectListItem>();

        public IReadOnlyList<SelectListItem> MaterialOptions { get; set; }
            = Array.Empty<SelectListItem>();

        public IReadOnlyList<SelectListItem> WorkOrderStatusOptions { get; set; }
            = Array.Empty<SelectListItem>();

        public IReadOnlyList<SelectListItem> CustomerJobStatusOptions { get; set; }
            = Array.Empty<SelectListItem>();

        public IReadOnlyList<SelectListItem> LossTypeOptions { get; set; }
            = Array.Empty<SelectListItem>();

        public IReadOnlyList<SelectListItem> InspectionResultOptions { get; set; }
            = Array.Empty<SelectListItem>();
    }

    public sealed class ProductionTraceViewModel
    {
        public ProductionWorkOrder WorkOrder { get; set; }
        public CustomerMaterialCustodyRecord CustomerCustody { get; set; }
        public IReadOnlyList<ProductionStatusHistory> StatusHistories { get; set; } = Array.Empty<ProductionStatusHistory>();
        public IReadOnlyList<ProductionMaterialReservation> Reservations { get; set; } = Array.Empty<ProductionMaterialReservation>();
        public IReadOnlyList<ProductionOperationLog> Operations { get; set; } = Array.Empty<ProductionOperationLog>();
        public IReadOnlyList<ProductionLossRecord> Losses { get; set; } = Array.Empty<ProductionLossRecord>();
        public IReadOnlyList<ProductionQualityInspection> Inspections { get; set; } = Array.Empty<ProductionQualityInspection>();
        public ProductionReceipt Receipt { get; set; }
    }

    public sealed class ProductionReportViewModel
    {
        public int BranchId { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int WorkOrderCount { get; set; }
        public int ClosedWorkOrderCount { get; set; }
        public decimal IssuedWeight { get; set; }
        public decimal OutputWeight { get; set; }
        public decimal LossWeight { get; set; }
        public decimal WipWeight { get; set; }
        public decimal MaterialCost { get; set; }
        public decimal LaborCost { get; set; }
        public decimal OverheadCost { get; set; }
        public decimal TotalCost { get; set; }
        public IReadOnlyList<ProductionReportRowViewModel> Rows { get; set; } = Array.Empty<ProductionReportRowViewModel>();
        public IReadOnlyList<ProductionAuditLog> AuditLogs { get; set; } = Array.Empty<ProductionAuditLog>();
    }

    public sealed class ProductionReportRowViewModel
    {
        public int WorkOrderId { get; set; }
        public string WorkOrderCode { get; set; }
        public string ProductName { get; set; }
        public string Status { get; set; }
        public decimal IssuedWeight { get; set; }
        public decimal OutputWeight { get; set; }
        public decimal LossWeight { get; set; }
        public decimal WipWeight { get; set; }
        public decimal TotalCost { get; set; }
    }

    public sealed class AttachProductionEvidenceInput
    {
        [Required, StringLength(30)]
        public string EntityType { get; set; } = string.Empty;
        [Range(1, int.MaxValue)]
        public int EntityId { get; set; }
        [Required, StringLength(1000)]
        public string EvidenceUrl { get; set; } = string.Empty;
        [Required]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public class CreateProductionWorkshopInput
    {
        [Required, StringLength(30)]
        public string Code { get; set; } = string.Empty;

        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int BranchId { get; set; }

        [StringLength(300)]
        public string Address { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsProductionAuthorized { get; set; }

        [StringLength(100)]
        public string LicenseNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime? LicenseValidFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime? LicenseValidTo { get; set; }

        [StringLength(1000)]
        public string Note { get; set; }
    }

    public class CreateProductionLossPolicyInput
    {
        [Required, StringLength(40)]
        public string PolicyCode { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int BranchId { get; set; }

        [Required, StringLength(50)]
        public string MaterialType { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.000001", "1.000000")]
        public decimal MinimumPurityRate { get; set; }

        [Range(typeof(decimal), "0.000001", "1.000000")]
        public decimal MaximumPurityRate { get; set; } = 1m;

        [StringLength(50)]
        public string OperationCode { get; set; }

        [Range(typeof(decimal), "0.0000", "100.0000")]
        public decimal MaximumLossRate { get; set; }

        [Range(typeof(decimal), "0.0000", "99999999999999.9999")]
        public decimal ApprovalWeightLimit { get; set; }

        [Range(typeof(decimal), "0.00", "9999999999999999.99")]
        public decimal ApprovalAmountLimit { get; set; }

        [Required, StringLength(30)]
        public string Version { get; set; } = "1.0";

        [DataType(DataType.Date)]
        public DateTime EffectiveFrom { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        public DateTime? EffectiveTo { get; set; }

        [StringLength(1000)]
        public string Note { get; set; }
    }

    public class CreateRawMaterialLotInput
    {
        [Required, StringLength(40)]
        public string LotCode { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int BranchId { get; set; }

        [Range(1, int.MaxValue)]
        public int WarehouseId { get; set; }

        [Range(1, int.MaxValue)]
        public int InventoryItemId { get; set; }

        public int? SupplierId { get; set; }

        [Required, StringLength(50)]
        public string MaterialType { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.000001", "1.000000")]
        public decimal PurityRate { get; set; }

        [Range(typeof(decimal), "0.0001", "99999999999999.9999")]
        public decimal GrossWeight { get; set; }

        [Range(typeof(decimal), "0.0001", "99999999999999.9999")]
        public decimal FineWeight { get; set; }

        [Required, StringLength(30)]
        public string SourceType { get; set; } = RawMaterialLot.SourceSupplier;

        [StringLength(100)]
        public string SourceReference { get; set; }

        [StringLength(100)]
        public string SourceDocumentNumber { get; set; }

        [Range(typeof(decimal), "0.00", "9999999999999999.99")]
        public decimal UnitCost { get; set; }

        [StringLength(1000)]
        public string Note { get; set; }
    }

    public class ReleaseRawMaterialLotInput
    {
        [Range(1, int.MaxValue)]
        public int RawMaterialLotId { get; set; }

        [Range(typeof(decimal), "0.000001", "1.000000")]
        public decimal MeasuredPurityRate { get; set; }

        [Range(typeof(decimal), "0.0001", "99999999999999.9999")]
        public decimal MeasuredGrossWeight { get; set; }

        [Range(typeof(decimal), "0.0001", "99999999999999.9999")]
        public decimal MeasuredFineWeight { get; set; }

        [Required, StringLength(1000)]
        public string QualityNote { get; set; } = string.Empty;
    }

    public class CreateProductionBomInput
    {
        [Required, StringLength(40)]
        public string BomCode { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int BranchId { get; set; }

        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Required, StringLength(30)]
        public string Version { get; set; } = "1.0";

        [DataType(DataType.Date)]
        public DateTime EffectiveFrom { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        public DateTime? EffectiveTo { get; set; }

        [Range(1, int.MaxValue)]
        public int StandardOutputQuantity { get; set; } = 1;

        [Range(typeof(decimal), "0.0001", "99999999999999.9999")]
        public decimal StandardOutputWeight { get; set; }

        [Range(typeof(decimal), "0.0000", "100.0000")]
        public decimal ExpectedLossRate { get; set; }

        [Range(typeof(decimal), "0.00", "9999999999999999.99")]
        public decimal EstimatedMaterialCost { get; set; }

        [Range(typeof(decimal), "0.00", "9999999999999999.99")]
        public decimal EstimatedLaborCost { get; set; }

        [Range(typeof(decimal), "0.00", "9999999999999999.99")]
        public decimal EstimatedOverheadCost { get; set; }

        [StringLength(1000)]
        public string Note { get; set; }

        [Required, MinLength(1)]
        public List<ProductionBomItemInput> Items { get; set; }
            = new List<ProductionBomItemInput>();

        [Required, MinLength(1)]
        public List<ProductionBomOperationInput> Operations { get; set; }
            = new List<ProductionBomOperationInput>();
    }

    public class ProductionBomItemInput
    {
        [Range(1, int.MaxValue)]
        public int SequenceNumber { get; set; }

        [Required, StringLength(50)]
        public string MaterialType { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.000001", "1.000000")]
        public decimal RequiredPurityRate { get; set; }

        [Range(typeof(decimal), "0.0001", "99999999999999.9999")]
        public decimal RequiredWeight { get; set; }

        [Range(typeof(decimal), "0.0000", "100.0000")]
        public decimal WasteAllowanceRate { get; set; }

        [Range(typeof(decimal), "0.00", "9999999999999999.99")]
        public decimal EstimatedUnitCost { get; set; }

        public bool IsRecoverable { get; set; } = true;

        [StringLength(500)]
        public string Note { get; set; }
    }

    public class ProductionBomOperationInput
    {
        [Range(1, int.MaxValue)]
        public int SequenceNumber { get; set; }

        [Required, StringLength(50)]
        public string OperationCode { get; set; } = string.Empty;

        [Required, StringLength(150)]
        public string OperationName { get; set; } = string.Empty;

        [StringLength(150)]
        public string WorkCenter { get; set; }

        [Range(1, 100000)]
        public int StandardMinutes { get; set; }

        [Range(typeof(decimal), "0.0000", "100.0000")]
        public decimal ExpectedLossRate { get; set; }

        [Range(typeof(decimal), "0.00", "9999999999999999.99")]
        public decimal EstimatedLaborCost { get; set; }

        public bool RequiresQualityCheck { get; set; }

        [StringLength(2000)]
        public string Instruction { get; set; }
    }

    public class ActivateProductionBomInput
    {
        [Range(1, int.MaxValue)]
        public int ProductionBomId { get; set; }

        [DataType(DataType.Date)]
        public DateTime EffectiveFrom { get; set; } = DateTime.Today;

        [Required, StringLength(1000)]
        public string ApprovalNote { get; set; } = string.Empty;
    }

    public class CreateProductionWorkOrderInput
    {
        [Required, StringLength(40)]
        public string WorkOrderCode { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int BranchId { get; set; }

        [Range(1, int.MaxValue)]
        public int WorkshopId { get; set; }

        [Range(1, int.MaxValue)]
        public int ProductionBomId { get; set; }

        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        public int? CustomerJobOrderId { get; set; }

        [Range(1, int.MaxValue)]
        public int MaterialWarehouseId { get; set; }

        [Range(1, int.MaxValue)]
        public int FinishedGoodsWarehouseId { get; set; }

        [Range(1, int.MaxValue)]
        public int PlannedQuantity { get; set; }

        [Range(typeof(decimal), "0.0001", "99999999999999.9999")]
        public decimal PlannedOutputWeight { get; set; }

        public DateTime PlannedStartAt { get; set; } = DateTime.Now;

        public DateTime? PlannedEndAt { get; set; }

        [Required, StringLength(450)]
        public string ResponsibleUserId { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Note { get; set; }
    }

    public class ReserveProductionMaterialInput
    {
        [Range(1, int.MaxValue)]
        public int ProductionWorkOrderId { get; set; }

        [Range(1, int.MaxValue)]
        public int RawMaterialLotId { get; set; }

        [Range(typeof(decimal), "0.0001", "99999999999999.9999")]
        public decimal ReservedWeight { get; set; }

        [StringLength(500)]
        public string Note { get; set; }

        [Required]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public class IssueProductionMaterialInput
    {
        [Range(1, int.MaxValue)]
        public int ProductionMaterialReservationId { get; set; }

        [Range(typeof(decimal), "0.0001", "99999999999999.9999")]
        public decimal IssuedWeight { get; set; }

        [Required, StringLength(450)]
        public string ReceiverUserId { get; set; } = string.Empty;

        [StringLength(500)]
        public string Note { get; set; }

        [Required]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public class ReturnProductionMaterialInput
    {
        [Range(1, int.MaxValue)]
        public int ProductionMaterialReservationId { get; set; }

        [Range(typeof(decimal), "0.0001", "99999999999999.9999")]
        public decimal ReturnedWeight { get; set; }

        [Required, StringLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(500)]
        public string EvidenceUrl { get; set; }

        [Required]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public class ReleaseReservedMaterialInput
    {
        [Range(1, int.MaxValue)]
        public int ProductionMaterialReservationId { get; set; }

        [Required, StringLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public class RecordProductionOperationInput
    {
        [Range(1, int.MaxValue)]
        public int ProductionWorkOrderId { get; set; }

        public int? ProductionBomOperationId { get; set; }

        [Range(1, int.MaxValue)]
        public int SequenceNumber { get; set; }

        [Required, StringLength(50)]
        public string OperationCode { get; set; } = string.Empty;

        [Required, StringLength(150)]
        public string OperationName { get; set; } = string.Empty;

        [Required, StringLength(30)]
        public string Status { get; set; } = ProductionOperationLog.StatusStarted;

        [Range(typeof(decimal), "0.0000", "99999999999999.9999")]
        public decimal InputWeight { get; set; }

        [Range(typeof(decimal), "0.0000", "99999999999999.9999")]
        public decimal OutputWeight { get; set; }

        [Range(typeof(decimal), "0.0000", "99999999999999.9999")]
        public decimal ScrapWeight { get; set; }

        [Required, StringLength(450)]
        public string WorkerUserId { get; set; } = string.Empty;

        public DateTime StartedAt { get; set; } = DateTime.Now;

        public DateTime? CompletedAt { get; set; }

        [StringLength(500)]
        public string EvidenceUrl { get; set; }

        [StringLength(1000)]
        public string Note { get; set; }

        [Required]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public class RecordProductionLossInput
    {
        [Range(1, int.MaxValue)]
        public int ProductionWorkOrderId { get; set; }

        public int? ProductionOperationLogId { get; set; }

        [Required, StringLength(30)]
        public string LossType { get; set; } = ProductionLossRecord.TypeOther;

        [Range(typeof(decimal), "0.0001", "99999999999999.9999")]
        public decimal LossWeight { get; set; }

        [Required, StringLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(500)]
        public string EvidenceUrl { get; set; }

        public bool RequestRecycle { get; set; }

        [Required]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public class ReviewProductionLossInput
    {
        [Range(1, int.MaxValue)]
        public int ProductionLossRecordId { get; set; }

        [Required, StringLength(30)]
        public string Decision { get; set; } = ProductionLossRecord.StatusApproved;

        [Required, StringLength(1000)]
        public string ReviewNote { get; set; } = string.Empty;

        [Required]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public class RecordProductionQualityInspectionInput
    {
        [Required, StringLength(40)]
        public string InspectionCode { get; set; } = string.Empty;

        public int? ProductionWorkOrderId { get; set; }

        public int? ProductionOperationLogId { get; set; }

        public int? ProductionRecycleBatchId { get; set; }

        public int? CustomerJobOrderId { get; set; }

        [Required, StringLength(30)]
        public string InspectionType { get; set; } = ProductionQualityInspection.TypeFinal;

        [Range(typeof(decimal), "0.0000", "99999999999999.9999")]
        public decimal MeasuredGrossWeight { get; set; }

        [Range(typeof(decimal), "0.0000", "99999999999999.9999")]
        public decimal MeasuredFineWeight { get; set; }

        [Range(typeof(decimal), "0.000001", "1.000000")]
        public decimal MeasuredPurityRate { get; set; }

        [Required, StringLength(30)]
        public string AppearanceResult { get; set; } = ProductionQualityInspection.ResultPending;

        [StringLength(100)]
        public string LabelCode { get; set; }

        [Required, StringLength(30)]
        public string Result { get; set; } = ProductionQualityInspection.ResultPending;

        [StringLength(50)]
        public string ReworkOperationCode { get; set; }

        [StringLength(500)]
        public string EvidenceUrl { get; set; }

        [StringLength(1000)]
        public string Note { get; set; }

        [Required]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public class ReleaseProductionWorkOrderInput
    {
        [Range(1, int.MaxValue)]
        public int ProductionWorkOrderId { get; set; }

        [Required, StringLength(40)]
        public string ReceiptCode { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int WarehouseId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(typeof(decimal), "0.0001", "99999999999999.9999")]
        public decimal GrossWeight { get; set; }

        [Range(typeof(decimal), "0.0001", "99999999999999.9999")]
        public decimal FineWeight { get; set; }

        [Range(typeof(decimal), "0.00", "9999999999999999.99")]
        public decimal UnitCost { get; set; }

        [StringLength(1000)]
        public string Note { get; set; }

        [Required]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public class ChangeProductionWorkOrderStatusInput
    {
        [Range(1, int.MaxValue)]
        public int ProductionWorkOrderId { get; set; }

        [Required, StringLength(30)]
        public string TargetStatus { get; set; } = string.Empty;

        [Required, StringLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public class CreateCustomerJobOrderInput
    {
        [Required, StringLength(40)]
        public string JobOrderCode { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int BranchId { get; set; }

        [Range(1, int.MaxValue)]
        public int WorkshopId { get; set; }

        [Required, StringLength(150)]
        public string CustomerName { get; set; } = string.Empty;

        [Required, StringLength(20)]
        [RegularExpression(@"^[0-9+() .-]{8,20}$")]
        public string CustomerPhone { get; set; } = string.Empty;

        [StringLength(100)]
        public string CustomerIdentityReference { get; set; }

        [Required, StringLength(30)]
        public string JobType { get; set; } = CustomerJobOrder.JobTypeNewCraft;

        [Required, StringLength(50)]
        public string MaterialType { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.0001", "99999999999999.9999")]
        public decimal InputGrossWeight { get; set; }

        [Range(typeof(decimal), "0.0001", "99999999999999.9999")]
        public decimal InputFineWeight { get; set; }

        [Range(typeof(decimal), "0.000001", "1.000000")]
        public decimal InputPurityRate { get; set; }

        [Required, StringLength(1000)]
        public string MaterialCondition { get; set; } = string.Empty;

        [Required, StringLength(1000)]
        public string IntakeImageUrl { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string CustomerOwnedStorageLocation { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.0000", "100.0000")]
        public decimal AgreedLossRate { get; set; }

        [Required, StringLength(2000)]
        public string DesignDescription { get; set; } = string.Empty;

        [StringLength(1000)]
        public string DesignImageUrl { get; set; }

        [StringLength(200)]
        public string DesignApprovalReference { get; set; }

        public DateTime? DesignApprovedAt { get; set; }

        [Range(typeof(decimal), "0.00", "9999999999999999.99")]
        public decimal QuotedLaborCost { get; set; }

        [Range(typeof(decimal), "0.00", "9999999999999999.99")]
        public decimal QuotedAdditionalMaterialCost { get; set; }

        [Range(typeof(decimal), "0.00", "9999999999999999.99")]
        public decimal QuotedTotalAmount { get; set; }

        [Range(typeof(decimal), "0.00", "9999999999999999.99")]
        public decimal DepositAmount { get; set; }

        public DateTime PromisedAt { get; set; }

        [StringLength(2000)]
        public string Note { get; set; }
    }

    public class RecordCustomerMaterialIssueInput
    {
        [Range(1, int.MaxValue)]
        public int CustomerJobOrderId { get; set; }

        [Range(typeof(decimal), "0.0001", "99999999999999.9999")]
        public decimal IssuedGrossWeight { get; set; }

        [Range(typeof(decimal), "0.0001", "99999999999999.9999")]
        public decimal IssuedFineWeight { get; set; }

        [Range(typeof(decimal), "0.000001", "1.000000")]
        public decimal IssuedPurityRate { get; set; }

        [Required, StringLength(1000)]
        public string EvidenceUrl { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string StorageLocation { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Note { get; set; }

        [Required]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public class ChangeCustomerJobOrderStatusInput
    {
        [Range(1, int.MaxValue)]
        public int CustomerJobOrderId { get; set; }

        [Required, StringLength(30)]
        public string TargetStatus { get; set; } = string.Empty;

        [Required, StringLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(200)]
        public string DesignApprovalReference { get; set; }

        public DateTime? DesignApprovedAt { get; set; }

        [Required]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public class RecordCustomerJobQualityInput
    {
        [Range(1, int.MaxValue)]
        public int CustomerJobOrderId { get; set; }

        [Required, StringLength(40)]
        public string InspectionCode { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.0001", "99999999999999.9999")]
        public decimal OutputGrossWeight { get; set; }

        [Range(typeof(decimal), "0.0001", "99999999999999.9999")]
        public decimal OutputFineWeight { get; set; }

        [Range(typeof(decimal), "0.000001", "1.000000")]
        public decimal OutputPurityRate { get; set; }

        [Required, StringLength(30)]
        public string AppearanceResult { get; set; } = ProductionQualityInspection.ResultPending;

        [Required, StringLength(30)]
        public string Result { get; set; } = ProductionQualityInspection.ResultPending;

        [StringLength(50)]
        public string ReworkOperationCode { get; set; }

        [StringLength(500)]
        public string EvidenceUrl { get; set; }

        [StringLength(1000)]
        public string Note { get; set; }

        [Required]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public class CompleteCustomerJobHandoverInput
    {
        [Range(1, int.MaxValue)]
        public int CustomerJobOrderId { get; set; }

        [Required, StringLength(150)]
        public string ReceiverName { get; set; } = string.Empty;

        [Required, StringLength(1000)]
        public string EvidenceUrl { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.00", "9999999999999999.99")]
        public decimal FinalAmount { get; set; }

        public DateTime HandoverAt { get; set; } = DateTime.Now;

        [StringLength(1000)]
        public string Note { get; set; }

        [Required]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public class CreateProductionRecycleBatchInput
    {
        [Required, StringLength(40)]
        public string BatchCode { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int BranchId { get; set; }

        [Range(1, int.MaxValue)]
        public int WorkshopId { get; set; }

        [Required, StringLength(50)]
        public string MaterialType { get; set; } = string.Empty;

        [Required, StringLength(30)]
        public string SourceType { get; set; } = ProductionRecycleBatch.SourceScrap;

        [Range(typeof(decimal), "0.0001", "99999999999999.9999")]
        public decimal InputGrossWeight { get; set; }

        [Range(typeof(decimal), "0.0000", "99999999999999.9999")]
        public decimal InputFineWeight { get; set; }

        [Required, MinLength(1)]
        public List<int> ProductionLossRecordIds { get; set; } = new List<int>();

        [StringLength(1000)]
        public string Note { get; set; }
    }

    public class CompleteProductionRecycleBatchInput
    {
        [Range(1, int.MaxValue)]
        public int ProductionRecycleBatchId { get; set; }

        [Required, StringLength(40)]
        public string InspectionCode { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.0001", "99999999999999.9999")]
        public decimal OutputGrossWeight { get; set; }

        [Range(typeof(decimal), "0.0001", "99999999999999.9999")]
        public decimal OutputFineWeight { get; set; }

        [Range(typeof(decimal), "0.000001", "1.000000")]
        public decimal OutputPurityRate { get; set; }

        [Required, StringLength(40)]
        public string OutputLotCode { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int OutputWarehouseId { get; set; }

        [Range(1, int.MaxValue)]
        public int OutputInventoryItemId { get; set; }

        [Range(typeof(decimal), "0.00", "9999999999999999.99")]
        public decimal OutputUnitCost { get; set; }

        [Required, StringLength(30)]
        public string AppearanceResult { get; set; } = ProductionQualityInspection.ResultPass;

        [StringLength(500)]
        public string EvidenceUrl { get; set; }

        [StringLength(1000)]
        public string Note { get; set; }
    }
}
