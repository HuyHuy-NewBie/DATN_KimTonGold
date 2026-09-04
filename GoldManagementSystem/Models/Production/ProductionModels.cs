using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoldManagementSystem.Models
{
    /// <summary>
    /// Xưởng/địa điểm được phép thực hiện chế tác.
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    public class ProductionWorkshop
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        public int BranchId { get; set; }

        public virtual Branch Branch { get; set; }

        [StringLength(300)]
        public string Address { get; set; }

        public bool IsActive { get; set; } = true;

        // Chỉ xưởng đã xác minh mới được nhận lệnh sản xuất.
        public bool IsProductionAuthorized { get; set; }

        [StringLength(100)]
        public string LicenseNumber { get; set; }

        public DateTime? LicenseValidFrom { get; set; }

        public DateTime? LicenseValidTo { get; set; }

        public DateTime? LicenseVerifiedAt { get; set; }

        [StringLength(450)]
        public string LicenseVerifiedByUserId { get; set; }

        public virtual AppUser LicenseVerifiedByUser { get; set; }

        [Required]
        [StringLength(450)]
        public string CreatedByUserId { get; set; } = string.Empty;

        public virtual AppUser CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string UpdatedByUserId { get; set; }

        public virtual AppUser UpdatedByUser { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(1000)]
        public string Note { get; set; }

        public virtual ICollection<ProductionWorkOrder> WorkOrders { get; set; }
            = new List<ProductionWorkOrder>();

        public virtual ICollection<ProductionRecycleBatch> RecycleBatches { get; set; }
            = new List<ProductionRecycleBatch>();

        public virtual ICollection<CustomerJobOrder> CustomerJobOrders { get; set; }
            = new List<CustomerJobOrder>();
    }

    /// <summary>
    /// Chính sách giới hạn hao hụt theo vật liệu/công đoạn.
    /// </summary>
    [Index(nameof(PolicyCode), IsUnique = true)]
    public class ProductionLossPolicy
    {
        public const string StatusDraft = "Draft";
        public const string StatusActive = "Active";
        public const string StatusRetired = "Retired";

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        public string PolicyCode { get; set; } = string.Empty;

        public int BranchId { get; set; }

        public virtual Branch Branch { get; set; }

        [Required]
        [StringLength(50)]
        public string MaterialType { get; set; } = string.Empty;

        // Hàm lượng lưu theo tỷ lệ 0..1.
        [Column(TypeName = "decimal(9,6)")]
        public decimal MinimumPurityRate { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal MaximumPurityRate { get; set; } = 1m;

        [StringLength(50)]
        public string OperationCode { get; set; }

        [Column(TypeName = "decimal(9,4)")]
        public decimal MaximumLossRate { get; set; }

        // Vượt hạn mức phải chuyển cấp duyệt cao hơn.
        [Column(TypeName = "decimal(18,4)")]
        public decimal ApprovalWeightLimit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ApprovalAmountLimit { get; set; }

        [Required]
        [StringLength(30)]
        public string Version { get; set; } = "1.0";

        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;

        public DateTime? EffectiveTo { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = StatusDraft;

        [Required]
        [StringLength(450)]
        public string CreatedByUserId { get; set; } = string.Empty;

        public virtual AppUser CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string ApprovedByUserId { get; set; }

        public virtual AppUser ApprovedByUser { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [StringLength(450)]
        public string UpdatedByUserId { get; set; }

        public virtual AppUser UpdatedByUser { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(1000)]
        public string Note { get; set; }

        public virtual ICollection<ProductionLossRecord> LossRecords { get; set; }
            = new List<ProductionLossRecord>();
    }

    /// <summary>
    /// Lô vàng/bạc nguyên liệu gắn một-một với mã tồn kho.
    /// </summary>
    [Index(nameof(LotCode), IsUnique = true)]
    [Index(nameof(InventoryItemId), IsUnique = true)]
    public class RawMaterialLot
    {
        public const string StatusReceiving = "Receiving";
        public const string StatusQuarantined = "Quarantined";
        public const string StatusReleased = "Released";
        public const string StatusReserved = "Reserved";
        public const string StatusExhausted = "Exhausted";
        public const string StatusRejected = "Rejected";

        public const string QualityPending = "Pending";
        public const string QualityPass = "Pass";
        public const string QualityFail = "Fail";

        public const string SourceSupplier = "Supplier";
        public const string SourceBuyback = "Buyback";
        public const string SourceRefining = "Refining";
        public const string SourceRecycle = "Recycle";
        public const string SourceAdjustment = "Adjustment";

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        public string LotCode { get; set; } = string.Empty;

        public int BranchId { get; set; }

        public virtual Branch Branch { get; set; }

        public int WarehouseId { get; set; }

        public virtual Warehouse Warehouse { get; set; }

        // Unique index bảo đảm một InventoryItem chỉ có một lô nguyên liệu.
        public int InventoryItemId { get; set; }

        public virtual InventoryItem InventoryItem { get; set; }

        public int? SupplierId { get; set; }

        public virtual Supplier Supplier { get; set; }

        [Required]
        [StringLength(50)]
        public string MaterialType { get; set; } = string.Empty;

        [Column(TypeName = "decimal(9,6)")]
        public decimal PurityRate { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal GrossWeight { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal FineWeight { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal AvailableWeight { get; set; }

        [Required]
        [StringLength(30)]
        public string SourceType { get; set; } = SourceSupplier;

        [StringLength(100)]
        public string SourceReference { get; set; }

        [StringLength(100)]
        public string SourceDocumentNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = StatusQuarantined;

        [Required]
        [StringLength(30)]
        public string QualityStatus { get; set; } = QualityPending;

        [StringLength(1000)]
        public string QualityNote { get; set; }

        [StringLength(450)]
        public string InspectedByUserId { get; set; }

        public virtual AppUser InspectedByUser { get; set; }

        public DateTime? InspectedAt { get; set; }

        [StringLength(450)]
        public string ReleasedByUserId { get; set; }

        public virtual AppUser ReleasedByUser { get; set; }

        public DateTime? ReleasedAt { get; set; }

        [Required]
        [StringLength(450)]
        public string CreatedByUserId { get; set; } = string.Empty;

        public virtual AppUser CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string UpdatedByUserId { get; set; }

        public virtual AppUser UpdatedByUser { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(1000)]
        public string Note { get; set; }

        public virtual ICollection<ProductionMaterialReservation> Reservations { get; set; }
            = new List<ProductionMaterialReservation>();
    }

    /// <summary>
    /// Định mức sản xuất được khóa theo phiên bản/ngày hiệu lực.
    /// </summary>
    [Index(nameof(BomCode), nameof(Version), IsUnique = true)]
    public class ProductionBom
    {
        public const string StatusDraft = "Draft";
        public const string StatusActive = "Active";
        public const string StatusRetired = "Retired";

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        public string BomCode { get; set; } = string.Empty;

        public int BranchId { get; set; }

        public virtual Branch Branch { get; set; }

        public int ProductId { get; set; }

        public virtual Product Product { get; set; }

        [Required]
        [StringLength(30)]
        public string Version { get; set; } = "1.0";

        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;

        public DateTime? EffectiveTo { get; set; }

        public int StandardOutputQuantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,4)")]
        public decimal StandardOutputWeight { get; set; }

        [Column(TypeName = "decimal(9,4)")]
        public decimal ExpectedLossRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedMaterialCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedLaborCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedOverheadCost { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = StatusDraft;

        [Required]
        [StringLength(450)]
        public string CreatedByUserId { get; set; } = string.Empty;

        public virtual AppUser CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string ApprovedByUserId { get; set; }

        public virtual AppUser ApprovedByUser { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [StringLength(450)]
        public string UpdatedByUserId { get; set; }

        public virtual AppUser UpdatedByUser { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(1000)]
        public string Note { get; set; }

        public virtual ICollection<ProductionBomItem> Items { get; set; }
            = new List<ProductionBomItem>();

        public virtual ICollection<ProductionBomOperation> Operations { get; set; }
            = new List<ProductionBomOperation>();

        public virtual ICollection<ProductionWorkOrder> WorkOrders { get; set; }
            = new List<ProductionWorkOrder>();
    }

    [Index(nameof(ProductionBomId), nameof(SequenceNumber), IsUnique = true)]
    public class ProductionBomItem
    {
        [Key]
        public int Id { get; set; }

        public int ProductionBomId { get; set; }

        public virtual ProductionBom ProductionBom { get; set; }

        public int SequenceNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string MaterialType { get; set; } = string.Empty;

        [Column(TypeName = "decimal(9,6)")]
        public decimal RequiredPurityRate { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal RequiredWeight { get; set; }

        [Column(TypeName = "decimal(9,4)")]
        public decimal WasteAllowanceRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedUnitCost { get; set; }

        public bool IsRecoverable { get; set; } = true;

        [StringLength(500)]
        public string Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    [Index(nameof(ProductionBomId), nameof(SequenceNumber), IsUnique = true)]
    public class ProductionBomOperation
    {
        [Key]
        public int Id { get; set; }

        public int ProductionBomId { get; set; }

        public virtual ProductionBom ProductionBom { get; set; }

        public int SequenceNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string OperationCode { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string OperationName { get; set; } = string.Empty;

        [StringLength(150)]
        public string WorkCenter { get; set; }

        public int StandardMinutes { get; set; }

        [Column(TypeName = "decimal(9,4)")]
        public decimal ExpectedLossRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedLaborCost { get; set; }

        public bool RequiresQualityCheck { get; set; }

        [StringLength(2000)]
        public string Instruction { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<ProductionOperationLog> OperationLogs { get; set; }
            = new List<ProductionOperationLog>();
    }

    /// <summary>
    /// Lệnh sản xuất và trạng thái xuyên suốt từ kế hoạch đến đóng lệnh.
    /// </summary>
    [Index(nameof(WorkOrderCode), IsUnique = true)]
    public class ProductionWorkOrder
    {
        public const string StatusPlanned = "Planned";
        public const string StatusMaterialReserved = "MaterialReserved";
        public const string StatusIssued = "Issued";
        public const string StatusInProgress = "InProgress";
        public const string StatusOnHold = "OnHold";
        public const string StatusQualityChecked = "QualityChecked";
        public const string StatusRework = "Rework";
        public const string StatusReleased = "Released";
        public const string StatusClosed = "Closed";
        public const string StatusCancelled = "Cancelled";

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        public string WorkOrderCode { get; set; } = string.Empty;

        public int BranchId { get; set; }

        public virtual Branch Branch { get; set; }

        public int WorkshopId { get; set; }

        public virtual ProductionWorkshop Workshop { get; set; }

        public int ProductionBomId { get; set; }

        public virtual ProductionBom ProductionBom { get; set; }

        public int ProductId { get; set; }

        public virtual Product Product { get; set; }

        // Có giá trị khi chế tác từ phiếu gia công của khách.
        public int? CustomerJobOrderId { get; set; }

        public virtual CustomerJobOrder CustomerJobOrder { get; set; }

        public int MaterialWarehouseId { get; set; }

        public virtual Warehouse MaterialWarehouse { get; set; }

        public int FinishedGoodsWarehouseId { get; set; }

        public virtual Warehouse FinishedGoodsWarehouse { get; set; }

        public int? WipInventoryItemId { get; set; }

        public virtual InventoryItem WipInventoryItem { get; set; }

        public int PlannedQuantity { get; set; }

        public int CompletedQuantity { get; set; }

        public int RejectedQuantity { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal PlannedOutputWeight { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal ActualOutputWeight { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal ReservedMaterialWeight { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal IssuedMaterialWeight { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal ActualLossWeight { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MaterialCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LaborCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OverheadCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; }

        public DateTime PlannedStartAt { get; set; }

        public DateTime? PlannedEndAt { get; set; }

        public DateTime? ActualStartAt { get; set; }

        public DateTime? ActualEndAt { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = StatusPlanned;

        [StringLength(50)]
        public string CurrentOperationCode { get; set; }

        [Required]
        [StringLength(450)]
        public string ResponsibleUserId { get; set; } = string.Empty;

        public virtual AppUser ResponsibleUser { get; set; }

        [Required]
        [StringLength(450)]
        public string CreatedByUserId { get; set; } = string.Empty;

        public virtual AppUser CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string ApprovedByUserId { get; set; }

        public virtual AppUser ApprovedByUser { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [StringLength(450)]
        public string ClosedByUserId { get; set; }

        public virtual AppUser ClosedByUser { get; set; }

        public DateTime? ClosedAt { get; set; }

        [StringLength(450)]
        public string UpdatedByUserId { get; set; }

        public virtual AppUser UpdatedByUser { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(1000)]
        public string HoldReason { get; set; }

        [StringLength(2000)]
        public string Note { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public virtual ICollection<ProductionMaterialReservation> MaterialReservations { get; set; }
            = new List<ProductionMaterialReservation>();

        public virtual ICollection<ProductionOperationLog> OperationLogs { get; set; }
            = new List<ProductionOperationLog>();

        public virtual ICollection<ProductionLossRecord> LossRecords { get; set; }
            = new List<ProductionLossRecord>();

        public virtual ICollection<ProductionQualityInspection> QualityInspections { get; set; }
            = new List<ProductionQualityInspection>();

        public virtual ICollection<ProductionReceipt> Receipts { get; set; }
            = new List<ProductionReceipt>();

        public virtual ICollection<ProductionStatusHistory> StatusHistories { get; set; }
            = new List<ProductionStatusHistory>();
    }

    [Index(nameof(ProductionWorkOrderId), nameof(RawMaterialLotId), IsUnique = true)]
    public class ProductionMaterialReservation
    {
        public const string StatusReserved = "Reserved";
        public const string StatusIssued = "Issued";
        public const string StatusPartiallyIssued = "PartiallyIssued";
        public const string StatusReleased = "Released";
        public const string StatusCancelled = "Cancelled";

        [Key]
        public int Id { get; set; }

        public int ProductionWorkOrderId { get; set; }

        public virtual ProductionWorkOrder ProductionWorkOrder { get; set; }

        public int RawMaterialLotId { get; set; }

        public virtual RawMaterialLot RawMaterialLot { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal ReservedWeight { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal IssuedWeight { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal ReturnedWeight { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = StatusReserved;

        public int? ProductionIssueTransactionId { get; set; }

        public virtual InventoryTransaction ProductionIssueTransaction { get; set; }

        public int? ReturnTransactionId { get; set; }

        public virtual InventoryTransaction ReturnTransaction { get; set; }

        [Required]
        [StringLength(450)]
        public string ReservedByUserId { get; set; } = string.Empty;

        public virtual AppUser ReservedByUser { get; set; }

        public DateTime ReservedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string IssuedByUserId { get; set; }

        public virtual AppUser IssuedByUser { get; set; }

        public DateTime? IssuedAt { get; set; }

        [StringLength(450)]
        public string ReleasedByUserId { get; set; }

        public virtual AppUser ReleasedByUser { get; set; }

        public DateTime? ReleasedAt { get; set; }

        [StringLength(500)]
        public string Note { get; set; }
    }

    /// <summary>
    /// Nhật ký thực hiện từng bước routing.
    /// </summary>
    public class ProductionOperationLog
    {
        public const string StatusStarted = "Started";
        public const string StatusPaused = "Paused";
        public const string StatusCompleted = "Completed";
        public const string StatusRework = "Rework";
        public const string StatusCancelled = "Cancelled";

        [Key]
        public int Id { get; set; }

        public int ProductionWorkOrderId { get; set; }

        public virtual ProductionWorkOrder ProductionWorkOrder { get; set; }

        public int? ProductionBomOperationId { get; set; }

        public virtual ProductionBomOperation ProductionBomOperation { get; set; }

        public int SequenceNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string OperationCode { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string OperationName { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = StatusStarted;

        [Column(TypeName = "decimal(18,4)")]
        public decimal InputWeight { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal OutputWeight { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal ScrapWeight { get; set; }

        [Required]
        [StringLength(450)]
        public string WorkerUserId { get; set; } = string.Empty;

        public virtual AppUser WorkerUser { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        [Required]
        [StringLength(450)]
        public string CreatedByUserId { get; set; } = string.Empty;

        public virtual AppUser CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string UpdatedByUserId { get; set; }

        public virtual AppUser UpdatedByUser { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string EvidenceUrl { get; set; }

        [StringLength(1000)]
        public string Note { get; set; }

        public virtual ICollection<ProductionLossRecord> LossRecords { get; set; }
            = new List<ProductionLossRecord>();

        public virtual ICollection<ProductionQualityInspection> QualityInspections { get; set; }
            = new List<ProductionQualityInspection>();
    }

    /// <summary>
    /// Hao hụt phải có nguyên nhân, chứng cứ và quyết định duyệt.
    /// </summary>
    public class ProductionLossRecord
    {
        public const string TypeEvaporation = "Evaporation";
        public const string TypeScrap = "Scrap";
        public const string TypeDefect = "Defect";
        public const string TypeScaleVariance = "ScaleVariance";
        public const string TypeOther = "Other";

        public const string StatusPendingApproval = "PendingApproval";
        public const string StatusApproved = "Approved";
        public const string StatusRejected = "Rejected";
        public const string StatusInvestigation = "Investigation";

        [Key]
        public int Id { get; set; }

        public int ProductionWorkOrderId { get; set; }

        public virtual ProductionWorkOrder ProductionWorkOrder { get; set; }

        public int? ProductionOperationLogId { get; set; }

        public virtual ProductionOperationLog ProductionOperationLog { get; set; }

        public int? ProductionLossPolicyId { get; set; }

        public virtual ProductionLossPolicy ProductionLossPolicy { get; set; }

        public int? ProductionRecycleBatchId { get; set; }

        public virtual ProductionRecycleBatch ProductionRecycleBatch { get; set; }

        [Required]
        [StringLength(30)]
        public string LossType { get; set; } = TypeOther;

        [Column(TypeName = "decimal(18,4)")]
        public decimal LossWeight { get; set; }

        [Column(TypeName = "decimal(9,4)")]
        public decimal LossRate { get; set; }

        // Chụp ngưỡng tại thời điểm ghi nhận, không phụ thuộc policy sửa sau này.
        [Column(TypeName = "decimal(9,4)")]
        public decimal AllowedLossRateSnapshot { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedLossAmount { get; set; }

        public bool IsOverTolerance { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = StatusPendingApproval;

        [Required]
        [StringLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(500)]
        public string EvidenceUrl { get; set; }

        [Required]
        [StringLength(450)]
        public string ReportedByUserId { get; set; } = string.Empty;

        public virtual AppUser ReportedByUser { get; set; }

        public DateTime ReportedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string ReviewedByUserId { get; set; }

        public virtual AppUser ReviewedByUser { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [StringLength(1000)]
        public string ReviewNote { get; set; }
    }

    /// <summary>
    /// Phiếu QC dùng cho công đoạn, thành phẩm, tái chế hoặc đơn khách.
    /// </summary>
    [Index(nameof(InspectionCode), IsUnique = true)]
    public class ProductionQualityInspection
    {
        public const string TypeInProcess = "InProcess";
        public const string TypeFinal = "Final";
        public const string TypeRecycle = "Recycle";
        public const string TypeCustomerJob = "CustomerJob";

        public const string ResultPending = "Pending";
        public const string ResultPass = "Pass";
        public const string ResultRework = "Rework";
        public const string ResultReject = "Reject";

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        public string InspectionCode { get; set; } = string.Empty;

        public int? ProductionWorkOrderId { get; set; }

        public virtual ProductionWorkOrder ProductionWorkOrder { get; set; }

        public int? ProductionOperationLogId { get; set; }

        public virtual ProductionOperationLog ProductionOperationLog { get; set; }

        public int? ProductionRecycleBatchId { get; set; }

        public virtual ProductionRecycleBatch ProductionRecycleBatch { get; set; }

        public int? CustomerJobOrderId { get; set; }

        public virtual CustomerJobOrder CustomerJobOrder { get; set; }

        [Required]
        [StringLength(30)]
        public string InspectionType { get; set; } = TypeFinal;

        [Column(TypeName = "decimal(18,4)")]
        public decimal MeasuredGrossWeight { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal MeasuredFineWeight { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal MeasuredPurityRate { get; set; }

        [Required]
        [StringLength(30)]
        public string AppearanceResult { get; set; } = ResultPending;

        [StringLength(100)]
        public string LabelCode { get; set; }

        [Required]
        [StringLength(30)]
        public string Result { get; set; } = ResultPending;

        [StringLength(50)]
        public string ReworkOperationCode { get; set; }

        [StringLength(500)]
        public string EvidenceUrl { get; set; }

        [Required]
        [StringLength(450)]
        public string InspectedByUserId { get; set; } = string.Empty;

        public virtual AppUser InspectedByUser { get; set; }

        public DateTime InspectedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string ApprovedByUserId { get; set; }

        public virtual AppUser ApprovedByUser { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [StringLength(1000)]
        public string Note { get; set; }

        public virtual ProductionReceipt Receipt { get; set; }
    }

    /// <summary>
    /// Phiếu nhập thành phẩm sau QC Pass.
    /// </summary>
    [Index(nameof(ReceiptCode), IsUnique = true)]
    public class ProductionReceipt
    {
        public const string StatusDraft = "Draft";
        public const string StatusPosted = "Posted";
        public const string StatusCancelled = "Cancelled";

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        public string ReceiptCode { get; set; } = string.Empty;

        public int ProductionWorkOrderId { get; set; }

        public virtual ProductionWorkOrder ProductionWorkOrder { get; set; }

        public int ProductionQualityInspectionId { get; set; }

        public virtual ProductionQualityInspection ProductionQualityInspection { get; set; }

        public int WarehouseId { get; set; }

        public virtual Warehouse Warehouse { get; set; }

        public int? InventoryItemId { get; set; }

        public virtual InventoryItem InventoryItem { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal GrossWeight { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal FineWeight { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = StatusDraft;

        [Required]
        [StringLength(450)]
        public string CreatedByUserId { get; set; } = string.Empty;

        public virtual AppUser CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string PostedByUserId { get; set; }

        public virtual AppUser PostedByUser { get; set; }

        public DateTime? PostedAt { get; set; }

        [StringLength(450)]
        public string CancelledByUserId { get; set; }

        public virtual AppUser CancelledByUser { get; set; }

        public DateTime? CancelledAt { get; set; }

        [StringLength(1000)]
        public string Note { get; set; }
    }

    /// <summary>
    /// Gom phế/hàng lỗi để tinh luyện rồi QC thành lô mới.
    /// </summary>
    [Index(nameof(BatchCode), IsUnique = true)]
    public class ProductionRecycleBatch
    {
        public const string StatusDraft = "Draft";
        public const string StatusCollected = "Collected";
        public const string StatusInRefining = "InRefining";
        public const string StatusQualityChecked = "QualityChecked";
        public const string StatusReleased = "Released";
        public const string StatusClosed = "Closed";
        public const string StatusCancelled = "Cancelled";

        public const string SourceScrap = "Scrap";
        public const string SourceDefect = "Defect";
        public const string SourceBuyback = "Buyback";
        public const string SourceMixed = "Mixed";

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        public string BatchCode { get; set; } = string.Empty;

        public int BranchId { get; set; }

        public virtual Branch Branch { get; set; }

        public int WorkshopId { get; set; }

        public virtual ProductionWorkshop Workshop { get; set; }

        [Required]
        [StringLength(50)]
        public string MaterialType { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string SourceType { get; set; } = SourceScrap;

        [Column(TypeName = "decimal(18,4)")]
        public decimal InputGrossWeight { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal InputFineWeight { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal OutputGrossWeight { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal OutputFineWeight { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal OutputPurityRate { get; set; }

        public int? OutputRawMaterialLotId { get; set; }

        public virtual RawMaterialLot OutputRawMaterialLot { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = StatusDraft;

        [Required]
        [StringLength(450)]
        public string CreatedByUserId { get; set; } = string.Empty;

        public virtual AppUser CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string StartedByUserId { get; set; }

        public virtual AppUser StartedByUser { get; set; }

        public DateTime? StartedAt { get; set; }

        [StringLength(450)]
        public string CompletedByUserId { get; set; }

        public virtual AppUser CompletedByUser { get; set; }

        public DateTime? CompletedAt { get; set; }

        [StringLength(450)]
        public string ReleasedByUserId { get; set; }

        public virtual AppUser ReleasedByUser { get; set; }

        public DateTime? ReleasedAt { get; set; }

        [StringLength(450)]
        public string UpdatedByUserId { get; set; }

        public virtual AppUser UpdatedByUser { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(1000)]
        public string Note { get; set; }

        public virtual ICollection<ProductionLossRecord> SourceLossRecords { get; set; }
            = new List<ProductionLossRecord>();

        public virtual ICollection<ProductionQualityInspection> QualityInspections { get; set; }
            = new List<ProductionQualityInspection>();

        public virtual ICollection<ProductionStatusHistory> StatusHistories { get; set; }
            = new List<ProductionStatusHistory>();
    }

    /// <summary>
    /// Gia công bằng vật liệu thuộc sở hữu khách, tách khỏi tồn công ty.
    /// </summary>
    [Index(nameof(JobOrderCode), IsUnique = true)]
    public class CustomerJobOrder
    {
        public const string JobTypeNewCraft = "NewCraft";
        public const string JobTypeRemodel = "Remodel";
        public const string JobTypeRepair = "Repair";

        public const string StatusReceived = "Received";
        public const string StatusAssessed = "Assessed";
        public const string StatusAwaitingApproval = "AwaitingApproval";
        public const string StatusApproved = "Approved";
        public const string StatusInProduction = "InProduction";
        public const string StatusQualityChecked = "QualityChecked";
        public const string StatusRework = "Rework";
        public const string StatusReadyForHandover = "ReadyForHandover";
        public const string StatusHandedOver = "HandedOver";
        public const string StatusCancelled = "Cancelled";

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        public string JobOrderCode { get; set; } = string.Empty;

        public int BranchId { get; set; }

        public virtual Branch Branch { get; set; }

        public int WorkshopId { get; set; }

        public virtual ProductionWorkshop Workshop { get; set; }

        [Required]
        [StringLength(150)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string CustomerPhone { get; set; } = string.Empty;

        // Chỉ lưu tham chiếu đã che/mã hóa, không hiển thị định danh đầy đủ.
        [StringLength(100)]
        public string CustomerIdentityReference { get; set; }

        [Required]
        [StringLength(30)]
        public string JobType { get; set; } = JobTypeNewCraft;

        [Required]
        [StringLength(50)]
        public string MaterialType { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,4)")]
        public decimal InputGrossWeight { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal InputFineWeight { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal InputPurityRate { get; set; }

        [Required]
        [StringLength(1000)]
        public string MaterialCondition { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string IntakeImageUrl { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string CustomerOwnedStorageLocation { get; set; } = string.Empty;

        [Column(TypeName = "decimal(9,4)")]
        public decimal AgreedLossRate { get; set; }

        [Required]
        [StringLength(2000)]
        public string DesignDescription { get; set; } = string.Empty;

        [StringLength(1000)]
        public string DesignImageUrl { get; set; }

        [StringLength(200)]
        public string DesignApprovalReference { get; set; }

        public DateTime? DesignApprovedAt { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal QuotedLaborCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal QuotedAdditionalMaterialCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal QuotedTotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DepositAmount { get; set; }

        public DateTime PromisedAt { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal OutputGrossWeight { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal OutputFineWeight { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal OutputPurityRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal FinalAmount { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = StatusReceived;

        [StringLength(30)]
        public string QualityResult { get; set; }

        [StringLength(150)]
        public string HandoverReceiverName { get; set; }

        [StringLength(1000)]
        public string HandoverEvidenceUrl { get; set; }

        public DateTime? HandoverAt { get; set; }

        [StringLength(450)]
        public string HandedOverByUserId { get; set; }

        public virtual AppUser HandedOverByUser { get; set; }

        [Required]
        [StringLength(450)]
        public string CreatedByUserId { get; set; } = string.Empty;

        public virtual AppUser CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string UpdatedByUserId { get; set; }

        public virtual AppUser UpdatedByUser { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(2000)]
        public string Note { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public virtual ICollection<ProductionWorkOrder> WorkOrders { get; set; }
            = new List<ProductionWorkOrder>();

        public virtual ICollection<ProductionQualityInspection> QualityInspections { get; set; }
            = new List<ProductionQualityInspection>();

        public virtual ICollection<ProductionStatusHistory> StatusHistories { get; set; }
            = new List<ProductionStatusHistory>();
    }

    public class CustomerMaterialCustodyRecord
    {
        public const string StatusHeld = "Held";
        public const string StatusInProduction = "InProduction";
        public const string StatusReadyForReturn = "ReadyForReturn";
        public const string StatusReturned = "Returned";

        [Key]
        public int Id { get; set; }
        public int CustomerJobOrderId { get; set; }
        public virtual CustomerJobOrder CustomerJobOrder { get; set; }
        public int BranchId { get; set; }

        [Required, StringLength(50)]
        public string MaterialType { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18,4)")]
        public decimal InputGrossWeight { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal InputFineWeight { get; set; }
        [Column(TypeName = "decimal(9,6)")]
        public decimal InputPurityRate { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal IssuedGrossWeight { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal IssuedFineWeight { get; set; }
        [Column(TypeName = "decimal(9,6)")]
        public decimal IssuedPurityRate { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal OutputGrossWeight { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal OutputFineWeight { get; set; }
        [Column(TypeName = "decimal(9,6)")]
        public decimal OutputPurityRate { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal ReturnedGrossWeight { get; set; }

        [Required, StringLength(30)]
        public string Status { get; set; } = StatusHeld;
        [Required, StringLength(200)]
        public string StorageLocation { get; set; } = string.Empty;
        [Required, StringLength(1000)]
        public string IntakeEvidenceUrl { get; set; } = string.Empty;
        [StringLength(1000)]
        public string ReturnEvidenceUrl { get; set; }
        [StringLength(450)]
        public string ReturnedByUserId { get; set; }
        public virtual AppUser ReturnedByUser { get; set; }
        public DateTime? ReturnedAt { get; set; }
        [Required, StringLength(450)]
        public string CreatedByUserId { get; set; } = string.Empty;
        public virtual AppUser CreatedByUser { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [StringLength(450)]
        public string UpdatedByUserId { get; set; }
        public virtual AppUser UpdatedByUser { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        [StringLength(1000)]
        public string Note { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    /// <summary>
    /// Lịch sử chuyển trạng thái, không sửa/xóa chứng từ cũ.
    /// </summary>
    public class ProductionStatusHistory
    {
        public const string EntityWorkOrder = "ProductionWorkOrder";
        public const string EntityCustomerJob = "CustomerJobOrder";
        public const string EntityRecycleBatch = "ProductionRecycleBatch";

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string EntityType { get; set; } = EntityWorkOrder;

        public int EntityId { get; set; }

        public int? ProductionWorkOrderId { get; set; }

        public virtual ProductionWorkOrder ProductionWorkOrder { get; set; }

        public int? CustomerJobOrderId { get; set; }

        public virtual CustomerJobOrder CustomerJobOrder { get; set; }

        public int? ProductionRecycleBatchId { get; set; }

        public virtual ProductionRecycleBatch ProductionRecycleBatch { get; set; }

        [StringLength(30)]
        public string FromStatus { get; set; }

        [Required]
        [StringLength(30)]
        public string ToStatus { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string ChangedByUserId { get; set; } = string.Empty;

        public virtual AppUser ChangedByUser { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        public bool IsSystemGenerated { get; set; }
    }

    public class ProductionAuditLog
    {
        [Key]
        public long Id { get; set; }
        [Required, StringLength(120)]
        public string Action { get; set; } = string.Empty;
        [Required, StringLength(80)]
        public string EntityType { get; set; } = string.Empty;
        public int? EntityId { get; set; }
        public int? BranchId { get; set; }
        [Required, StringLength(450)]
        public string ActorUserId { get; set; } = string.Empty;
        public virtual AppUser ActorUser { get; set; }
        [Required, StringLength(2000)]
        public string Snapshot { get; set; } = string.Empty;
        public bool Succeeded { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
