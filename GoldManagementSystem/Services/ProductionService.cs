using GoldManagementSystem.Data;
using GoldManagementSystem.Models;
using GoldManagementSystem.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Globalization;
using System.Reflection;

namespace GoldManagementSystem.Services
{
    /// <summary>
    /// Lỗi nghiệp vụ có thể hiển thị trực tiếp cho người dùng.
    /// </summary>
    public sealed class ProductionBusinessException : InvalidOperationException
    {
        public ProductionBusinessException(string message)
            : base(message)
        {
        }

        public ProductionBusinessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public interface IProductionService
    {
        Task<ProductionWorkshop> CreateWorkshopAsync(CreateProductionWorkshopInput input, string actorUserId, CancellationToken cancellationToken = default);
        Task<ProductionWorkshop> UpdateWorkshopAsync(int workshopId, CreateProductionWorkshopInput input, string actorUserId, CancellationToken cancellationToken = default);
        Task<ProductionWorkshop> SetWorkshopActiveAsync(int workshopId, bool isActive, string actorUserId, CancellationToken cancellationToken = default);

        Task<ProductionLossPolicy> CreateLossPolicyAsync(CreateProductionLossPolicyInput input, string actorUserId, CancellationToken cancellationToken = default);
        Task<ProductionLossPolicy> UpdateLossPolicyAsync(int policyId, CreateProductionLossPolicyInput input, string actorUserId, CancellationToken cancellationToken = default);
        Task<ProductionLossPolicy> ActivateLossPolicyAsync(int policyId, string actorUserId, CancellationToken cancellationToken = default);

        Task<RawMaterialLot> CreateRawMaterialLotAsync(CreateRawMaterialLotInput input, string actorUserId, CancellationToken cancellationToken = default);
        Task<RawMaterialLot> ReleaseRawMaterialLotAsync(int rawMaterialLotId, ReleaseRawMaterialLotInput input, string actorUserId, CancellationToken cancellationToken = default);

        Task<ProductionBom> CreateBomAsync(CreateProductionBomInput input, string actorUserId, CancellationToken cancellationToken = default);
        Task<ProductionBom> ActivateBomAsync(int bomId, ActivateProductionBomInput input, string actorUserId, CancellationToken cancellationToken = default);

        Task<ProductionWorkOrder> CreateWorkOrderAsync(CreateProductionWorkOrderInput input, string actorUserId, CancellationToken cancellationToken = default);
        Task<ProductionMaterialReservation> ReserveMaterialAsync(ReserveProductionMaterialInput input, string actorUserId, CancellationToken cancellationToken = default);
        Task<ProductionWorkOrder> IssueMaterialAsync(IssueProductionMaterialInput input, string actorUserId, CancellationToken cancellationToken = default);
        Task<ProductionMaterialReservation> ReturnMaterialAsync(ReturnProductionMaterialInput input, string actorUserId, CancellationToken cancellationToken = default);
        Task<ProductionMaterialReservation> ReleaseReservedMaterialAsync(ReleaseReservedMaterialInput input, string actorUserId, CancellationToken cancellationToken = default);
        Task<ProductionOperationLog> RecordOperationAsync(RecordProductionOperationInput input, string actorUserId, CancellationToken cancellationToken = default);
        Task<ProductionLossRecord> RecordLossAsync(RecordProductionLossInput input, string actorUserId, CancellationToken cancellationToken = default);
        Task<ProductionLossRecord> ReviewLossAsync(int lossRecordId, ReviewProductionLossInput input, string reviewerUserId, CancellationToken cancellationToken = default);
        Task<ProductionQualityInspection> RecordQualityInspectionAsync(RecordProductionQualityInspectionInput input, string actorUserId, CancellationToken cancellationToken = default);
        Task<ProductionReceipt> ReleaseWorkOrderAsync(int workOrderId, ReleaseProductionWorkOrderInput input, string actorUserId, CancellationToken cancellationToken = default);
        Task<ProductionWorkOrder> ChangeWorkOrderStatusAsync(int workOrderId, ChangeProductionWorkOrderStatusInput input, string actorUserId, CancellationToken cancellationToken = default);

        Task<CustomerJobOrder> CreateCustomerJobOrderAsync(CreateCustomerJobOrderInput input, string actorUserId, CancellationToken cancellationToken = default);
        Task<CustomerMaterialCustodyRecord> RecordCustomerMaterialIssueAsync(RecordCustomerMaterialIssueInput input, string actorUserId, CancellationToken cancellationToken = default);
        Task<AttachProductionEvidenceInput> AttachEvidenceAsync(AttachProductionEvidenceInput input, string actorUserId, CancellationToken cancellationToken = default);
        Task<ProductionQualityInspection> RecordCustomerJobQualityAsync(int customerJobOrderId, RecordCustomerJobQualityInput input, string actorUserId, CancellationToken cancellationToken = default);
        Task<CustomerJobOrder> CompleteCustomerJobHandoverAsync(int customerJobOrderId, CompleteCustomerJobHandoverInput input, string actorUserId, CancellationToken cancellationToken = default);

        Task<ProductionRecycleBatch> CreateRecycleBatchAsync(CreateProductionRecycleBatchInput input, string actorUserId, CancellationToken cancellationToken = default);
        Task<ProductionRecycleBatch> CompleteRecycleBatchAsync(int recycleBatchId, CompleteProductionRecycleBatchInput input, string actorUserId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Transaction boundary and domain rules for manufacturing, customer jobs and
    /// recycling. Controllers must not update production aggregates directly.
    /// </summary>
    public sealed class ProductionService : IProductionService
    {
        private const string StatusDraft = "Draft";
        private const string StatusActive = "Active";
        private const string StatusInactive = RawMaterialLot.StatusQuarantined;
        private const string StatusSuperseded = ProductionLossPolicy.StatusRetired;
        private const string StatusReleased = "Released";
        private const string StatusDepleted = RawMaterialLot.StatusExhausted;
        private const string StatusPlanned = "Planned";
        private const string StatusMaterialReserved = "MaterialReserved";
        private const string StatusIssued = "Issued";
        private const string StatusInProgress = "InProgress";
        private const string StatusCompleted = "Completed";
        private const string StatusQualityChecked = "QualityChecked";
        private const string StatusClosed = "Closed";
        private const string StatusCancelled = "Cancelled";
        private const string StatusHold = ProductionWorkOrder.StatusOnHold;
        private const string StatusPendingApproval = "PendingApproval";
        private const string StatusApproved = "Approved";
        private const string StatusRejected = "Rejected";
        private const string StatusReserved = "Reserved";
        private const string StatusPosted = "Posted";
        private const string QualityPending = "Pending";
        private const string QualityPass = "Pass";
        private const string QualityRework = "Rework";
        private const string QualityReject = "Reject";
        private const string CustomerReceived = "Received";
        private const string CustomerReadyForHandover = "ReadyForHandover";
        private const string CustomerHandedOver = "HandedOver";

        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ProductionService(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public Task<ProductionWorkshop> CreateWorkshopAsync(
            CreateProductionWorkshopInput input,
            string actorUserId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);

            return ExecuteSerializableAsync(async () =>
            {
                var actor = await RequireActorAsync(actorUserId, cancellationToken);
                var branchId = ReadRequiredInt(input, "BranchId");
                await RequireActiveBranchAsync(branchId, cancellationToken);

                var code = ReadRequiredText(input, 30, "Code", "WorkshopCode");
                if (await _context.Set<ProductionWorkshop>().AnyAsync(
                    item => item.Code == code,
                    cancellationToken))
                {
                    throw Business("Mã xưởng chế tác đã tồn tại.");
                }

                var authorized = Read(input, false, "IsProductionAuthorized");
                var licenseNumber = ReadOptionalText(input, 100, "LicenseNumber");
                var validFrom = ReadNullableUtc(input, "LicenseValidFrom");
                var validTo = ReadNullableUtc(input, "LicenseValidTo");
                ValidateWorkshopLicense(authorized, licenseNumber, validFrom, validTo, DateTime.UtcNow);

                var now = DateTime.UtcNow;
                var workshop = new ProductionWorkshop
                {
                    Code = code,
                    Name = ReadRequiredText(input, 180, "Name", "WorkshopName"),
                    BranchId = branchId,
                    Address = ReadOptionalText(input, 300, "Address"),
                    IsActive = Read(input, true, "IsActive"),
                    IsProductionAuthorized = authorized,
                    LicenseNumber = licenseNumber,
                    LicenseValidFrom = validFrom,
                    LicenseValidTo = validTo,
                    LicenseVerifiedAt = authorized ? now : null,
                    LicenseVerifiedByUserId = authorized ? actor.Id : null,
                    CreatedByUserId = actor.Id,
                    CreatedAt = now,
                    UpdatedByUserId = actor.Id,
                    UpdatedAt = now,
                    Note = ReadOptionalText(input, 1000, "Note")
                };

                _context.Set<ProductionWorkshop>().Add(workshop);
                return workshop;
            }, cancellationToken);
        }

        public Task<ProductionWorkshop> UpdateWorkshopAsync(
            int workshopId,
            CreateProductionWorkshopInput input,
            string actorUserId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);

            return ExecuteSerializableAsync(async () =>
            {
                var actor = await RequireActorAsync(actorUserId, cancellationToken);
                var workshop = await RequireWorkshopAsync(workshopId, cancellationToken);
                await RequireActorBranchAsync(actor, workshop.BranchId);
                var branchId = Read(input, workshop.BranchId, "BranchId");
                await RequireActiveBranchAsync(branchId, cancellationToken);

                var code = ReadRequiredText(input, 30, "Code", "WorkshopCode");
                if (await _context.Set<ProductionWorkshop>().AnyAsync(
                    item => item.Id != workshopId && item.Code == code,
                    cancellationToken))
                {
                    throw Business("Mã xưởng chế tác đã tồn tại.");
                }

                if (branchId != workshop.BranchId
                    && await _context.Set<ProductionWorkOrder>().AnyAsync(
                        item => item.WorkshopId == workshopId,
                        cancellationToken))
                {
                    throw Business("Không thể chuyển chi nhánh cho xưởng đã phát sinh lệnh sản xuất.");
                }

                var authorized = Read(input, workshop.IsProductionAuthorized, "IsProductionAuthorized");
                var licenseNumber = ReadOptionalText(input, 100, "LicenseNumber");
                var validFrom = ReadNullableUtc(input, "LicenseValidFrom");
                var validTo = ReadNullableUtc(input, "LicenseValidTo");
                ValidateWorkshopLicense(authorized, licenseNumber, validFrom, validTo, DateTime.UtcNow);

                workshop.Code = code;
                workshop.Name = ReadRequiredText(input, 180, "Name", "WorkshopName");
                workshop.BranchId = branchId;
                workshop.Address = ReadOptionalText(input, 300, "Address");
                workshop.IsProductionAuthorized = authorized;
                workshop.LicenseNumber = licenseNumber;
                workshop.LicenseValidFrom = validFrom;
                workshop.LicenseValidTo = validTo;
                workshop.LicenseVerifiedAt = authorized ? DateTime.UtcNow : null;
                workshop.LicenseVerifiedByUserId = authorized ? actor.Id : null;
                workshop.Note = ReadOptionalText(input, 1000, "Note");
                workshop.UpdatedByUserId = actor.Id;
                workshop.UpdatedAt = DateTime.UtcNow;
                return workshop;
            }, cancellationToken);
        }

        public Task<ProductionWorkshop> SetWorkshopActiveAsync(
            int workshopId,
            bool isActive,
            string actorUserId,
            CancellationToken cancellationToken = default)
        {
            return ExecuteSerializableAsync(async () =>
            {
                var actor = await RequireActorAsync(actorUserId, cancellationToken);
                var workshop = await RequireWorkshopAsync(workshopId, cancellationToken);
                await RequireActorBranchAsync(actor, workshop.BranchId);

                if (isActive)
                {
                    await RequireActiveBranchAsync(workshop.BranchId, cancellationToken);
                    ValidateWorkshopLicense(
                        workshop.IsProductionAuthorized,
                        workshop.LicenseNumber,
                        workshop.LicenseValidFrom,
                        workshop.LicenseValidTo,
                        DateTime.UtcNow);
                }
                else
                {
                    var hasOpenOrders = await _context.Set<ProductionWorkOrder>().AnyAsync(
                        item => item.WorkshopId == workshopId
                            && item.Status != StatusClosed
                            && item.Status != StatusCancelled,
                        cancellationToken);
                    if (hasOpenOrders)
                    {
                        throw Business("Xưởng còn lệnh sản xuất chưa kết thúc.");
                    }
                }

                workshop.IsActive = isActive;
                workshop.UpdatedByUserId = actor.Id;
                workshop.UpdatedAt = DateTime.UtcNow;
                return workshop;
            }, cancellationToken);
        }

        public Task<ProductionLossPolicy> CreateLossPolicyAsync(
            CreateProductionLossPolicyInput input,
            string actorUserId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);

            return ExecuteSerializableAsync(async () =>
            {
                var actor = await RequireActorAsync(actorUserId, cancellationToken);
                var branchId = ReadRequiredInt(input, "BranchId");
                await RequireActiveBranchAsync(branchId, cancellationToken);
                var values = ReadAndValidateLossPolicy(input);

                if (await _context.Set<ProductionLossPolicy>().AnyAsync(
                    item => item.PolicyCode == values.Code && item.Version == values.Version,
                    cancellationToken))
                {
                    throw Business("Mã và phiên bản chính sách hao hụt đã tồn tại.");
                }

                var now = DateTime.UtcNow;
                var policy = new ProductionLossPolicy
                {
                    PolicyCode = values.Code,
                    BranchId = branchId,
                    MaterialType = values.MaterialType,
                    MinimumPurityRate = values.MinimumPurityRate,
                    MaximumPurityRate = values.MaximumPurityRate,
                    OperationCode = values.OperationCode,
                    MaximumLossRate = values.MaximumLossRate,
                    ApprovalWeightLimit = values.ApprovalWeightLimit,
                    ApprovalAmountLimit = values.ApprovalAmountLimit,
                    Version = values.Version,
                    EffectiveFrom = values.EffectiveFrom,
                    EffectiveTo = values.EffectiveTo,
                    Status = StatusDraft,
                    CreatedByUserId = actor.Id,
                    CreatedAt = now,
                    UpdatedByUserId = actor.Id,
                    UpdatedAt = now,
                    Note = values.Note
                };
                _context.Set<ProductionLossPolicy>().Add(policy);
                return policy;
            }, cancellationToken);
        }

        public Task<ProductionLossPolicy> UpdateLossPolicyAsync(
            int policyId,
            CreateProductionLossPolicyInput input,
            string actorUserId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);

            return ExecuteSerializableAsync(async () =>
            {
                var actor = await RequireActorAsync(actorUserId, cancellationToken);
                var policy = await _context.Set<ProductionLossPolicy>()
                    .FirstOrDefaultAsync(item => item.Id == policyId, cancellationToken)
                    ?? throw Business("Không tìm thấy chính sách hao hụt.");
                await RequireActorBranchAsync(actor, policy.BranchId);
                if (policy.Status == StatusActive)
                {
                    throw Business("Chính sách đang hiệu lực là bản ghi bất biến; hãy tạo phiên bản mới.");
                }

                var branchId = Read(input, policy.BranchId, "BranchId");
                await RequireActiveBranchAsync(branchId, cancellationToken);
                var values = ReadAndValidateLossPolicy(input);
                if (await _context.Set<ProductionLossPolicy>().AnyAsync(
                    item => item.Id != policyId
                        && item.PolicyCode == values.Code
                        && item.Version == values.Version,
                    cancellationToken))
                {
                    throw Business("Mã và phiên bản chính sách hao hụt đã tồn tại.");
                }

                policy.PolicyCode = values.Code;
                policy.BranchId = branchId;
                policy.MaterialType = values.MaterialType;
                policy.MinimumPurityRate = values.MinimumPurityRate;
                policy.MaximumPurityRate = values.MaximumPurityRate;
                policy.OperationCode = values.OperationCode;
                policy.MaximumLossRate = values.MaximumLossRate;
                policy.ApprovalWeightLimit = values.ApprovalWeightLimit;
                policy.ApprovalAmountLimit = values.ApprovalAmountLimit;
                policy.Version = values.Version;
                policy.EffectiveFrom = values.EffectiveFrom;
                policy.EffectiveTo = values.EffectiveTo;
                policy.Note = values.Note;
                policy.UpdatedByUserId = actor.Id;
                policy.UpdatedAt = DateTime.UtcNow;
                return policy;
            }, cancellationToken);
        }

        public Task<ProductionLossPolicy> ActivateLossPolicyAsync(
            int policyId,
            string actorUserId,
            CancellationToken cancellationToken = default)
        {
            return ExecuteSerializableAsync(async () =>
            {
                var actor = await RequireActorAsync(actorUserId, cancellationToken);
                var policy = await _context.Set<ProductionLossPolicy>()
                    .FirstOrDefaultAsync(item => item.Id == policyId, cancellationToken)
                    ?? throw Business("Không tìm thấy chính sách hao hụt.");
                await RequireActorBranchAsync(actor, policy.BranchId);
                await RequireActiveBranchAsync(policy.BranchId, cancellationToken);
                ValidatePolicyValues(policy.MinimumPurityRate, policy.MaximumPurityRate,
                    policy.MaximumLossRate, policy.ApprovalWeightLimit, policy.ApprovalAmountLimit,
                    policy.EffectiveFrom, policy.EffectiveTo);

                var overlapping = await _context.Set<ProductionLossPolicy>()
                    .Where(item => item.Id != policy.Id
                        && item.BranchId == policy.BranchId
                        && item.MaterialType == policy.MaterialType
                        && item.OperationCode == policy.OperationCode
                        && item.Status == StatusActive)
                    .ToListAsync(cancellationToken);
                foreach (var oldPolicy in overlapping)
                {
                    oldPolicy.Status = StatusSuperseded;
                    oldPolicy.UpdatedByUserId = actor.Id;
                    oldPolicy.UpdatedAt = DateTime.UtcNow;
                }

                policy.Status = StatusActive;
                policy.ApprovedByUserId = actor.Id;
                policy.ApprovedAt = DateTime.UtcNow;
                policy.UpdatedByUserId = actor.Id;
                policy.UpdatedAt = DateTime.UtcNow;
                return policy;
            }, cancellationToken);
        }

        public Task<RawMaterialLot> CreateRawMaterialLotAsync(
            CreateRawMaterialLotInput input,
            string actorUserId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);

            return ExecuteSerializableAsync(async () =>
            {
                var actor = await RequireActorAsync(actorUserId, cancellationToken);
                var branchId = ReadRequiredInt(input, "BranchId");
                var warehouseId = ReadRequiredInt(input, "WarehouseId");
                var inventoryItemId = ReadRequiredInt(input, "InventoryItemId");
                await RequireActiveBranchAsync(branchId, cancellationToken);
                var warehouse = await RequireWarehouseAsync(warehouseId, branchId, cancellationToken);
                var inventoryItem = await _context.InventoryItems
                    .FirstOrDefaultAsync(item => item.Id == inventoryItemId, cancellationToken)
                    ?? throw Business("Không tìm thấy mã tồn kho nguyên liệu.");
                if (inventoryItem.WarehouseId != warehouse.Id)
                {
                    throw Business("Mã tồn kho không thuộc kho nguyên liệu đã chọn.");
                }
                if (inventoryItem.Status == InventoryItem.StatusQuarantined
                    || inventoryItem.Status == InventoryItem.StatusOutOfStock
                    || inventoryItem.Status == InventoryItem.StatusDepleted)
                {
                    throw Business("Mã tồn kho không sẵn sàng để tạo lô nguyên liệu.");
                }

                var lotCode = ReadRequiredText(input, 40, "LotCode", "Code");
                if (await _context.Set<RawMaterialLot>().AnyAsync(
                    item => item.LotCode == lotCode || item.InventoryItemId == inventoryItemId,
                    cancellationToken))
                {
                    throw Business("Mã lô hoặc mã tồn kho đã được khai báo lô nguyên liệu.");
                }

                var grossWeight = ReadRequiredPositiveDecimal(input, "GrossWeight", "GrossWeightGrams");
                var purityRate = ReadRequiredPurity(input, "PurityRate");
                var fineWeight = FineWeight(grossWeight, purityRate);
                var suppliedFineWeight = Read(input, fineWeight, "FineWeight", "FineWeightGrams");
                EnsureCloseFineWeight(suppliedFineWeight, fineWeight);
                if (inventoryItem.WeightOnHand + 0.0001m < grossWeight)
                {
                    throw Business("Trọng lượng lô lớn hơn trọng lượng tồn kho hiện có.");
                }

                var materialType = ReadRequiredText(input, 120, "MaterialType");
                if (!string.Equals(inventoryItem.MaterialType?.Trim(), materialType,
                    StringComparison.OrdinalIgnoreCase))
                {
                    throw Business("Loại nguyên liệu không khớp với mã tồn kho.");
                }

                var supplierId = ReadNullableInt(input, "SupplierId");
                if (supplierId.HasValue
                    && !await _context.Suppliers.AnyAsync(item => item.Id == supplierId.Value, cancellationToken))
                {
                    throw Business("Nhà cung cấp không tồn tại.");
                }

                var now = DateTime.UtcNow;
                var lot = new RawMaterialLot
                {
                    LotCode = lotCode,
                    BranchId = branchId,
                    WarehouseId = warehouse.Id,
                    InventoryItemId = inventoryItem.Id,
                    SupplierId = supplierId,
                    MaterialType = materialType,
                    PurityRate = purityRate,
                    GrossWeight = RoundGrams(grossWeight),
                    FineWeight = fineWeight,
                    AvailableWeight = RoundGrams(grossWeight),
                    SourceType = ReadRequiredText(input, 50, "SourceType"),
                    SourceReference = ReadOptionalText(input, 100, "SourceReference"),
                    SourceDocumentNumber = ReadOptionalText(input, 100, "SourceDocumentNumber", "DocumentNumber"),
                    UnitCost = ReadNonNegativeDecimal(input, "UnitCost"),
                    Status = StatusDraft,
                    QualityStatus = QualityPending,
                    QualityNote = ReadOptionalText(input, 1000, "QualityNote"),
                    CreatedByUserId = actor.Id,
                    CreatedAt = now,
                    UpdatedByUserId = actor.Id,
                    UpdatedAt = now,
                    Note = ReadOptionalText(input, 1000, "Note")
                };
                _context.Set<RawMaterialLot>().Add(lot);
                return lot;
            }, cancellationToken);
        }

        public Task<RawMaterialLot> ReleaseRawMaterialLotAsync(
            int rawMaterialLotId,
            ReleaseRawMaterialLotInput input,
            string actorUserId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);

            return ExecuteSerializableAsync(async () =>
            {
                var actor = await RequireActorAsync(actorUserId, cancellationToken);
                var lot = await _context.Set<RawMaterialLot>()
                    .FirstOrDefaultAsync(item => item.Id == rawMaterialLotId, cancellationToken)
                    ?? throw Business("Không tìm thấy lô nguyên liệu.");
                if (lot.Status != StatusDraft && lot.Status != StatusInactive)
                {
                    throw Business("Chỉ lô đang nháp/cách ly mới được phép duyệt cấp phát.");
                }

                await RequireActiveBranchAsync(lot.BranchId, cancellationToken);
                await RequireWarehouseAsync(lot.WarehouseId, lot.BranchId, cancellationToken);
                var inventoryItem = await _context.InventoryItems
                    .FirstOrDefaultAsync(item => item.Id == lot.InventoryItemId, cancellationToken)
                    ?? throw Business("Mã tồn kho của lô không còn tồn tại.");
                if (inventoryItem.WarehouseId != lot.WarehouseId
                    || inventoryItem.WeightOnHand + 0.0001m < lot.AvailableWeight)
                {
                    throw Business("Dữ liệu kho và lô nguyên liệu không nhất quán.");
                }

                var qualityStatus = ReadRequiredText(input, 30, "QualityStatus", "Result");
                if (!IsPass(qualityStatus))
                {
                    lot.QualityStatus = NormalizeQualityResult(qualityStatus);
                    lot.QualityNote = ReadOptionalText(input, 1000, "QualityNote", "Note");
                    lot.InspectedByUserId = actor.Id;
                    lot.InspectedAt = DateTime.UtcNow;
                    lot.UpdatedByUserId = actor.Id;
                    lot.UpdatedAt = DateTime.UtcNow;
                    throw Business("Lô nguyên liệu chưa đạt kiểm tra chất lượng nên không thể cấp phát.");
                }

                var measuredPurity = Read(input, lot.PurityRate, "MeasuredPurityRate", "PurityRate");
                ValidatePurity(measuredPurity, "Hàm lượng kiểm định");
                var measuredGross = Read(input, lot.GrossWeight, "MeasuredGrossWeight", "GrossWeight");
                var measuredFine = Read(input, FineWeight(measuredGross, measuredPurity), "MeasuredFineWeight", "FineWeight");
                if (measuredGross <= 0 || measuredGross > inventoryItem.WeightOnHand + 0.0001m) throw Business("Khối lượng kiểm định không hợp lệ với tồn kho.");
                EnsureCloseFineWeight(measuredFine, FineWeight(measuredGross, measuredPurity));
                lot.GrossWeight = RoundGrams(measuredGross);
                lot.AvailableWeight = RoundGrams(measuredGross);
                lot.PurityRate = measuredPurity;
                lot.FineWeight = FineWeight(measuredGross, measuredPurity);
                lot.Status = StatusReleased;
                lot.QualityStatus = QualityPass;
                lot.QualityNote = ReadOptionalText(input, 1000, "QualityNote", "Note");
                lot.InspectedByUserId = actor.Id;
                lot.InspectedAt = DateTime.UtcNow;
                lot.ReleasedByUserId = actor.Id;
                lot.ReleasedAt = DateTime.UtcNow;
                lot.UpdatedByUserId = actor.Id;
                lot.UpdatedAt = DateTime.UtcNow;
                inventoryItem.Status = InventoryItem.StatusAvailable;
                inventoryItem.UpdatedAt = DateTime.UtcNow;
                return lot;
            }, cancellationToken);
        }

        public Task<ProductionBom> CreateBomAsync(CreateProductionBomInput input, string actorUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            return ExecuteSerializableAsync(async () =>
            {
                var actor = await RequireActorAsync(actorUserId, cancellationToken);
                await RequireActiveBranchAsync(input.BranchId, cancellationToken);
                if (!await _context.Products.AnyAsync(item => item.Id == input.ProductId && item.BranchId == input.BranchId, cancellationToken)) throw Business("Sản phẩm không tồn tại hoặc không thuộc chi nhánh.");
                if (await _context.ProductionBoms.AnyAsync(item => item.BomCode == input.BomCode && item.Version == input.Version, cancellationToken)) throw Business("Mã BOM và phiên bản đã tồn tại.");
                ValidateBom(input);
                var now = DateTime.UtcNow;
                var bom = new ProductionBom
                {
                    BomCode = input.BomCode.Trim(), BranchId = input.BranchId, ProductId = input.ProductId, Version = input.Version.Trim(),
                    EffectiveFrom = input.EffectiveFrom.ToUniversalTime(), EffectiveTo = input.EffectiveTo?.ToUniversalTime(), StandardOutputQuantity = input.StandardOutputQuantity,
                    StandardOutputWeight = input.StandardOutputWeight, ExpectedLossRate = input.ExpectedLossRate, EstimatedMaterialCost = input.EstimatedMaterialCost,
                    EstimatedLaborCost = input.EstimatedLaborCost, EstimatedOverheadCost = input.EstimatedOverheadCost, Status = ProductionBom.StatusDraft,
                    CreatedByUserId = actor.Id, CreatedAt = now, UpdatedByUserId = actor.Id, UpdatedAt = now, Note = input.Note?.Trim()
                };
                foreach (var item in input.Items) bom.Items.Add(new ProductionBomItem { SequenceNumber = item.SequenceNumber, MaterialType = NormalizeMaterialType(item.MaterialType), RequiredPurityRate = item.RequiredPurityRate, RequiredWeight = item.RequiredWeight, WasteAllowanceRate = item.WasteAllowanceRate, EstimatedUnitCost = item.EstimatedUnitCost, IsRecoverable = item.IsRecoverable, Note = item.Note?.Trim(), CreatedAt = now, UpdatedAt = now });
                foreach (var operation in input.Operations) bom.Operations.Add(new ProductionBomOperation { SequenceNumber = operation.SequenceNumber, OperationCode = operation.OperationCode.Trim(), OperationName = operation.OperationName.Trim(), WorkCenter = operation.WorkCenter?.Trim(), StandardMinutes = operation.StandardMinutes, ExpectedLossRate = operation.ExpectedLossRate, EstimatedLaborCost = operation.EstimatedLaborCost, RequiresQualityCheck = operation.RequiresQualityCheck, Instruction = operation.Instruction?.Trim(), CreatedAt = now, UpdatedAt = now });
                _context.ProductionBoms.Add(bom);
                return bom;
            }, cancellationToken);
        }

        public Task<ProductionBom> ActivateBomAsync(int bomId, ActivateProductionBomInput input, string actorUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            return ExecuteSerializableAsync(async () =>
            {
                var actor = await RequireActorAsync(actorUserId, cancellationToken);
                var bom = await _context.ProductionBoms.Include(item => item.Items).Include(item => item.Operations).FirstOrDefaultAsync(item => item.Id == bomId, cancellationToken) ?? throw Business("Không tìm thấy BOM.");
                await RequireActorBranchAsync(actor, bom.BranchId);
                await RequireActiveBranchAsync(bom.BranchId, cancellationToken);
                if (bom.Status == ProductionBom.StatusRetired) throw Business("BOM đã ngừng hiệu lực.");
                if (bom.Items.Count == 0 || bom.Operations.Count == 0) throw Business("BOM phải có ít nhất một vật tư và một công đoạn.");
                var active = await _context.ProductionBoms.Where(item => item.Id != bom.Id && item.BranchId == bom.BranchId && item.ProductId == bom.ProductId && item.Status == ProductionBom.StatusActive).ToListAsync(cancellationToken);
                foreach (var old in active) { old.Status = ProductionBom.StatusRetired; old.EffectiveTo = input.EffectiveFrom.ToUniversalTime(); old.UpdatedByUserId = actor.Id; old.UpdatedAt = DateTime.UtcNow; }
                bom.Status = ProductionBom.StatusActive; bom.EffectiveFrom = input.EffectiveFrom.ToUniversalTime(); bom.ApprovedByUserId = actor.Id; bom.ApprovedAt = DateTime.UtcNow; bom.UpdatedByUserId = actor.Id; bom.UpdatedAt = DateTime.UtcNow; bom.Note = input.ApprovalNote.Trim();
                return bom;
            }, cancellationToken);
        }

        public Task<ProductionWorkOrder> CreateWorkOrderAsync(CreateProductionWorkOrderInput input, string actorUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            return ExecuteSerializableAsync(async () =>
            {
                var actor = await RequireActorAsync(actorUserId, cancellationToken);
                await RequireActiveBranchAsync(input.BranchId, cancellationToken);
                var workshop = await RequireWorkshopAsync(input.WorkshopId, input.BranchId, cancellationToken);
                var bom = await _context.ProductionBoms.FirstOrDefaultAsync(item => item.Id == input.ProductionBomId && item.ProductId == input.ProductId && item.BranchId == input.BranchId && item.Status == ProductionBom.StatusActive, cancellationToken) ?? throw Business("BOM chưa được duyệt hoặc không khớp sản phẩm.");
                await RequireWarehouseAsync(input.MaterialWarehouseId, input.BranchId, cancellationToken); await RequireWarehouseAsync(input.FinishedGoodsWarehouseId, input.BranchId, cancellationToken);
                if (await _context.ProductionWorkOrders.AnyAsync(item => item.WorkOrderCode == input.WorkOrderCode, cancellationToken)) throw Business("Mã lệnh sản xuất đã tồn tại.");
                if (!await _context.Users.AnyAsync(item => item.Id == input.ResponsibleUserId, cancellationToken)) throw Business("Không tìm thấy người phụ trách.");
                CustomerJobOrder customerJob = null;
                if (input.CustomerJobOrderId.HasValue)
                {
                    customerJob = await _context.CustomerJobOrders.FirstOrDefaultAsync(item => item.Id == input.CustomerJobOrderId && item.BranchId == input.BranchId, cancellationToken) ?? throw Business("Đơn gia công khách không hợp lệ.");
                    if (customerJob.Status is CustomerJobOrder.StatusCancelled or CustomerJobOrder.StatusHandedOver) throw Business("Đơn gia công khách đã kết thúc.");
                    var customerCustody = await _context.CustomerMaterialCustodyRecords.FirstOrDefaultAsync(item => item.CustomerJobOrderId == customerJob.Id, cancellationToken) ?? throw Business("Đơn khách chưa có sổ tài sản.");
                    if (customerCustody.IssuedGrossWeight <= 0) throw Business("Chưa lập phiếu giao nhận vật liệu khách vào xưởng.");
                }
                var now = DateTime.UtcNow;
                var order = new ProductionWorkOrder { WorkOrderCode = input.WorkOrderCode.Trim(), BranchId = input.BranchId, WorkshopId = workshop.Id, ProductionBomId = bom.Id, ProductId = input.ProductId, CustomerJobOrderId = input.CustomerJobOrderId, MaterialWarehouseId = input.MaterialWarehouseId, FinishedGoodsWarehouseId = input.FinishedGoodsWarehouseId, PlannedQuantity = input.PlannedQuantity, PlannedOutputWeight = input.PlannedOutputWeight, PlannedStartAt = input.PlannedStartAt.ToUniversalTime(), PlannedEndAt = input.PlannedEndAt?.ToUniversalTime(), ResponsibleUserId = input.ResponsibleUserId, Status = StatusPlanned, CreatedByUserId = actor.Id, CreatedAt = now, UpdatedByUserId = actor.Id, UpdatedAt = now, Note = input.Note?.Trim() };
                _context.ProductionWorkOrders.Add(order); if (customerJob != null) { var custody = await _context.CustomerMaterialCustodyRecords.FirstOrDefaultAsync(item => item.CustomerJobOrderId == customerJob.Id, cancellationToken) ?? throw Business("Đơn khách chưa có sổ bàn giao tài sản."); var oldCustomerStatus = customerJob.Status; customerJob.Status = CustomerJobOrder.StatusInProduction; customerJob.UpdatedByUserId = actor.Id; customerJob.UpdatedAt = now; custody.Status = CustomerMaterialCustodyRecord.StatusInProduction; custody.UpdatedByUserId = actor.Id; custody.UpdatedAt = now; AddStatus(customerJob, oldCustomerStatus, CustomerJobOrder.StatusInProduction, "Bắt đầu thực hiện lệnh chế tác", actor.Id, true); } AddStatus(order, null, StatusPlanned, "Tạo lệnh sản xuất", actor.Id, true); return order;
            }, cancellationToken);
        }

        public Task<ProductionMaterialReservation> ReserveMaterialAsync(ReserveProductionMaterialInput input, string actorUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            return ExecuteSerializableAsync(async () =>
            {
                var actor = await RequireActorAsync(actorUserId, cancellationToken); var order = await RequireWorkOrder(input.ProductionWorkOrderId, actorUserId, cancellationToken); var lot = await _context.RawMaterialLots.FirstOrDefaultAsync(item => item.Id == input.RawMaterialLotId, cancellationToken) ?? throw Business("Không tìm thấy lô nguyên liệu.");
                if (order.BranchId != lot.BranchId || order.MaterialWarehouseId != lot.WarehouseId || lot.Status != RawMaterialLot.StatusReleased) throw Business("Lô nguyên liệu chưa được phép cấp cho lệnh.");
                if (lot.AvailableWeight < input.ReservedWeight) throw Business("Khối lượng lô nguyên liệu không đủ.");
                if (await _context.ProductionMaterialReservations.AnyAsync(item => item.ProductionWorkOrderId == order.Id && item.RawMaterialLotId == lot.Id && item.Status != ProductionMaterialReservation.StatusCancelled, cancellationToken)) throw Business("Lô nguyên liệu đã được giữ cho lệnh này.");
                lot.Status = RawMaterialLot.StatusReserved; lot.AvailableWeight -= input.ReservedWeight;
                var reservation = new ProductionMaterialReservation { ProductionWorkOrderId = order.Id, RawMaterialLotId = lot.Id, ReservedWeight = input.ReservedWeight, Status = ProductionMaterialReservation.StatusReserved, ReservedByUserId = actor.Id, ReservedAt = DateTime.UtcNow, Note = input.Note?.Trim() };
                order.ReservedMaterialWeight += input.ReservedWeight; if (order.Status == StatusPlanned) { order.Status = StatusMaterialReserved; AddStatus(order, StatusPlanned, StatusMaterialReserved, "Đã giữ nguyên liệu", actor.Id, true); } _context.ProductionMaterialReservations.Add(reservation); return reservation;
            }, cancellationToken);
        }

        public Task<ProductionWorkOrder> IssueMaterialAsync(IssueProductionMaterialInput input, string actorUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            return ExecuteSerializableAsync(async () =>
            {
                var actor = await RequireActorAsync(actorUserId, cancellationToken); var reservation = await _context.ProductionMaterialReservations.Include(item => item.ProductionWorkOrder).Include(item => item.RawMaterialLot).FirstOrDefaultAsync(item => item.Id == input.ProductionMaterialReservationId, cancellationToken) ?? throw Business("Không tìm thấy phiếu giữ nguyên liệu.");
                var order = reservation.ProductionWorkOrder; var lot = reservation.RawMaterialLot; if (reservation.Status != ProductionMaterialReservation.StatusReserved && reservation.Status != ProductionMaterialReservation.StatusPartiallyIssued) throw Business("Phiếu giữ nguyên liệu không còn hiệu lực.");
                if (lot.BranchId != order.BranchId || lot.WarehouseId != order.MaterialWarehouseId) throw Business("Lô nguyên liệu không thuộc kho vật tư của lệnh.");
                if (input.IssuedWeight <= 0 || input.IssuedWeight > reservation.ReservedWeight - reservation.IssuedWeight || input.IssuedWeight > lot.GrossWeight) throw Business("Khối lượng xuất cấp không hợp lệ.");
                var receiver = await _context.Users.FirstOrDefaultAsync(item => item.Id == input.ReceiverUserId, cancellationToken) ?? throw Business("Không tìm thấy người nhận vật tư.");
                if (receiver.BranchId.HasValue && receiver.BranchId != order.BranchId && !await _userManager.IsInRoleAsync(receiver, RoleCatalog.Admin)) throw Business("Người nhận không thuộc chi nhánh của lệnh.");
                var inventory = await _context.InventoryItems.FirstOrDefaultAsync(item => item.Id == lot.InventoryItemId, cancellationToken) ?? throw Business("Không tìm thấy tồn kho nguyên liệu.");
                if (inventory.WarehouseId != lot.WarehouseId) throw Business("Tồn kho nguyên liệu không thuộc kho của lô.");
                if (inventory.WeightOnHand < input.IssuedWeight) throw Business("Tồn kho không đủ để xuất cấp.");
                inventory.WeightOnHand -= input.IssuedWeight; inventory.Status = inventory.WeightOnHand <= 0 ? InventoryItem.StatusDepleted : InventoryItem.StatusWorkInProgress; inventory.UpdatedAt = DateTime.UtcNow;
                lot.GrossWeight -= input.IssuedWeight; lot.Status = lot.GrossWeight <= 0 ? RawMaterialLot.StatusExhausted : RawMaterialLot.StatusReserved; reservation.IssuedWeight += input.IssuedWeight; reservation.Status = reservation.IssuedWeight >= reservation.ReservedWeight ? ProductionMaterialReservation.StatusIssued : ProductionMaterialReservation.StatusPartiallyIssued; reservation.IssuedByUserId = actor.Id; reservation.IssuedAt = DateTime.UtcNow;
                var transaction = AddInventoryTransaction(inventory, -input.IssuedWeight, InventoryTransaction.TypeProductionIssue, order.Id, actor.Id, "Xuất cấp cho " + order.WorkOrderCode); reservation.ProductionIssueTransaction = transaction; order.IssuedMaterialWeight += input.IssuedWeight; order.Status = StatusIssued; order.ActualStartAt ??= DateTime.UtcNow; AddStatus(order, StatusMaterialReserved, StatusIssued, "Đã xuất cấp nguyên liệu", actor.Id, true); return order;
            }, cancellationToken);
        }

        public Task<ProductionMaterialReservation> ReturnMaterialAsync(ReturnProductionMaterialInput input, string actorUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            return ExecuteSerializableAsync(async () =>
            {
                var actor = await RequireActorAsync(actorUserId, cancellationToken);
                var reservation = await _context.ProductionMaterialReservations
                    .Include(item => item.ProductionWorkOrder)
                    .Include(item => item.RawMaterialLot)
                    .FirstOrDefaultAsync(item => item.Id == input.ProductionMaterialReservationId, cancellationToken)
                    ?? throw Business("Không tìm thấy phiếu giữ nguyên liệu.");
                var order = reservation.ProductionWorkOrder;
                var lot = reservation.RawMaterialLot;
                if (actor.BranchId.HasValue && actor.BranchId != order.BranchId && !await _userManager.IsInRoleAsync(actor, RoleCatalog.Admin)) throw Business("Không được hoàn nguyên vật tư của chi nhánh khác.");
                if (lot.BranchId != order.BranchId || lot.WarehouseId != order.MaterialWarehouseId) throw Business("Lô nguyên liệu không thuộc kho vật tư của lệnh.");
                var remaining = reservation.IssuedWeight - reservation.ReturnedWeight;
                if (reservation.IssuedWeight <= 0 || input.ReturnedWeight > remaining + 0.0001m) throw Business("Khối lượng hoàn trả vượt số đã xuất.");
                var inventory = await _context.InventoryItems.FirstOrDefaultAsync(item => item.Id == lot.InventoryItemId && item.WarehouseId == lot.WarehouseId, cancellationToken) ?? throw Business("Không tìm thấy tồn kho nguyên liệu.");
                inventory.WeightOnHand += input.ReturnedWeight;
                inventory.Status = InventoryItem.StatusAvailable;
                inventory.UpdatedAt = DateTime.UtcNow;
                lot.GrossWeight += input.ReturnedWeight;
                lot.AvailableWeight += input.ReturnedWeight;
                lot.Status = RawMaterialLot.StatusReleased;
                reservation.ReturnedWeight += input.ReturnedWeight;
                reservation.Status = reservation.ReturnedWeight >= reservation.IssuedWeight - 0.0001m ? ProductionMaterialReservation.StatusReleased : ProductionMaterialReservation.StatusPartiallyIssued;
                reservation.ReleasedByUserId = actor.Id;
                reservation.ReleasedAt = DateTime.UtcNow;
                order.IssuedMaterialWeight = Math.Max(0, order.IssuedMaterialWeight - input.ReturnedWeight);
                var transaction = AddInventoryTransaction(inventory, input.ReturnedWeight, InventoryTransaction.TypeProductionReturn, order.Id, actor.Id, "Hoàn nguyên liệu từ " + order.WorkOrderCode + ": " + input.Reason.Trim());
                transaction.Note = input.EvidenceUrl?.Trim() is { Length: > 0 } evidence ? transaction.Note + " | " + evidence : transaction.Note;
                reservation.ReturnTransaction = transaction;
                _context.InventoryTransactions.Add(transaction);
                return reservation;
            }, cancellationToken);
        }

        public Task<ProductionMaterialReservation> ReleaseReservedMaterialAsync(ReleaseReservedMaterialInput input, string actorUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            return ExecuteSerializableAsync(async () =>
            {
                var actor = await RequireActorAsync(actorUserId, cancellationToken);
                var reservation = await _context.ProductionMaterialReservations.Include(item => item.ProductionWorkOrder).Include(item => item.RawMaterialLot).FirstOrDefaultAsync(item => item.Id == input.ProductionMaterialReservationId, cancellationToken) ?? throw Business("Không tìm thấy phiếu giữ nguyên liệu.");
                await RequireActorBranchAsync(actor, reservation.ProductionWorkOrder.BranchId);
                var unissued = reservation.ReservedWeight - reservation.IssuedWeight;
                if (unissued <= 0) throw Business("Phiếu không còn khối lượng chưa xuất để giải phóng.");
                reservation.RawMaterialLot.AvailableWeight += unissued;
                reservation.RawMaterialLot.Status = RawMaterialLot.StatusReleased;
                reservation.Status = reservation.IssuedWeight > 0 ? ProductionMaterialReservation.StatusPartiallyIssued : ProductionMaterialReservation.StatusReleased;
                reservation.ProductionWorkOrder.ReservedMaterialWeight = Math.Max(0, reservation.ProductionWorkOrder.ReservedMaterialWeight - unissued);
                reservation.ReleasedByUserId = actor.Id;
                reservation.ReleasedAt = DateTime.UtcNow;
                reservation.Note = string.IsNullOrWhiteSpace(reservation.Note) ? input.Reason.Trim() : reservation.Note + " | " + input.Reason.Trim();
                return reservation;
            }, cancellationToken);
        }

        public Task<ProductionOperationLog> RecordOperationAsync(RecordProductionOperationInput input, string actorUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            return ExecuteSerializableAsync(async () =>
            {
                var actor = await RequireActorAsync(actorUserId, cancellationToken); var order = await RequireWorkOrder(input.ProductionWorkOrderId, actorUserId, cancellationToken); if (order.Status is StatusPlanned or StatusMaterialReserved) throw Business("Lệnh chưa được xuất cấp nguyên liệu."); if (input.InputWeight <= 0 || input.OutputWeight < 0 || input.ScrapWeight < 0 || input.OutputWeight + input.ScrapWeight > input.InputWeight + 0.0001m || input.InputWeight > order.IssuedMaterialWeight + 0.0001m) throw Business("Khối lượng công đoạn không hợp lệ hoặc vượt vật tư đã xuất.");
                var log = new ProductionOperationLog { ProductionWorkOrderId = order.Id, ProductionBomOperationId = input.ProductionBomOperationId, SequenceNumber = input.SequenceNumber, OperationCode = input.OperationCode.Trim(), OperationName = input.OperationName.Trim(), Status = input.Status, InputWeight = input.InputWeight, OutputWeight = input.OutputWeight, ScrapWeight = input.ScrapWeight, WorkerUserId = input.WorkerUserId, StartedAt = input.StartedAt.ToUniversalTime(), CompletedAt = input.CompletedAt?.ToUniversalTime(), CreatedByUserId = actor.Id, CreatedAt = DateTime.UtcNow, UpdatedByUserId = actor.Id, UpdatedAt = DateTime.UtcNow, EvidenceUrl = input.EvidenceUrl?.Trim(), Note = input.Note?.Trim() };
                order.CurrentOperationCode = log.OperationCode; order.Status = input.Status == ProductionOperationLog.StatusCompleted ? StatusInProgress : StatusInProgress; order.ActualOutputWeight = input.OutputWeight; order.ActualLossWeight += input.ScrapWeight; _context.ProductionOperationLogs.Add(log); AddStatus(order, StatusIssued, StatusInProgress, "Ghi nhận công đoạn " + log.OperationName, actor.Id, true); return log;
            }, cancellationToken);
        }

        public Task<ProductionLossRecord> RecordLossAsync(RecordProductionLossInput input, string actorUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            return ExecuteSerializableAsync(async () =>
            {
                var actor = await RequireActorAsync(actorUserId, cancellationToken); var order = await RequireWorkOrder(input.ProductionWorkOrderId, actorUserId, cancellationToken); if (input.LossWeight <= 0) throw Business("Khối lượng hao hụt phải lớn hơn 0."); var basis = order.IssuedMaterialWeight <= 0 ? order.PlannedOutputWeight : order.IssuedMaterialWeight; var lossRate = basis <= 0 ? 0 : input.LossWeight / basis * 100m; var policy = await ReadAndValidateLossPolicy(order.BranchId, NormalizeMaterialType(order.ProductionBom?.Product?.ProductLine ?? ProductLineOptions.Gold), input.ProductionOperationLogId, cancellationToken); var allowed = policy?.MaximumLossRate ?? 0m; var loss = new ProductionLossRecord { ProductionWorkOrderId = order.Id, ProductionOperationLogId = input.ProductionOperationLogId, ProductionLossPolicyId = policy?.Id, LossType = input.LossType, LossWeight = input.LossWeight, LossRate = lossRate, AllowedLossRateSnapshot = allowed, IsOverTolerance = lossRate > allowed, Status = lossRate > allowed ? ProductionLossRecord.StatusPendingApproval : ProductionLossRecord.StatusApproved, Reason = input.Reason.Trim(), EvidenceUrl = input.EvidenceUrl?.Trim(), ReportedByUserId = actor.Id, ReportedAt = DateTime.UtcNow }; order.ActualLossWeight += input.LossWeight; if (loss.IsOverTolerance) order.Status = StatusHold; _context.ProductionLossRecords.Add(loss); return loss;
            }, cancellationToken);
        }

        public Task<ProductionLossRecord> ReviewLossAsync(int lossRecordId, ReviewProductionLossInput input, string reviewerUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            return ExecuteSerializableAsync(async () => { var actor = await RequireActorAsync(reviewerUserId, cancellationToken); var loss = await _context.ProductionLossRecords.Include(item => item.ProductionWorkOrder).FirstOrDefaultAsync(item => item.Id == lossRecordId, cancellationToken) ?? throw Business("Không tìm thấy biên bản hao hụt."); if (actor.BranchId.HasValue && actor.BranchId != loss.ProductionWorkOrder.BranchId && !await _userManager.IsInRoleAsync(actor, RoleCatalog.Admin)) throw Business("Không được duyệt hao hụt của chi nhánh khác."); if (loss.Status != ProductionLossRecord.StatusPendingApproval) throw Business("Biên bản hao hụt đã được xử lý."); if (input.Decision != ProductionLossRecord.StatusApproved && input.Decision != ProductionLossRecord.StatusRejected) throw Business("Quyết định duyệt hao hụt không hợp lệ."); loss.Status = input.Decision; loss.ReviewedByUserId = actor.Id; loss.ReviewedAt = DateTime.UtcNow; loss.ReviewNote = input.ReviewNote.Trim(); if (loss.Status == ProductionLossRecord.StatusApproved) loss.ProductionWorkOrder.Status = StatusInProgress; return loss; }, cancellationToken);
        }

        public Task<ProductionQualityInspection> RecordQualityInspectionAsync(RecordProductionQualityInspectionInput input, string actorUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            return ExecuteSerializableAsync(async () => { var actor = await RequireActorAsync(actorUserId, cancellationToken); if (await _context.ProductionQualityInspections.AnyAsync(item => item.InspectionCode == input.InspectionCode, cancellationToken)) throw Business("Mã kiểm phẩm đã tồn tại."); var order = input.ProductionWorkOrderId.HasValue ? await RequireWorkOrder(input.ProductionWorkOrderId.Value, actorUserId, cancellationToken) : null; var result = NormalizeQualityResult(input.Result); var appearance = NormalizeQualityResult(input.AppearanceResult); if (result == QualityPass && appearance != QualityPass) throw Business("QC đạt phải đồng thời đạt ngoại quan."); var inspection = new ProductionQualityInspection { InspectionCode = input.InspectionCode.Trim(), ProductionWorkOrderId = input.ProductionWorkOrderId, ProductionOperationLogId = input.ProductionOperationLogId, ProductionRecycleBatchId = input.ProductionRecycleBatchId, CustomerJobOrderId = input.CustomerJobOrderId, InspectionType = input.InspectionType, MeasuredGrossWeight = input.MeasuredGrossWeight, MeasuredFineWeight = input.MeasuredFineWeight, MeasuredPurityRate = input.MeasuredPurityRate, AppearanceResult = appearance, LabelCode = input.LabelCode?.Trim(), Result = result, ReworkOperationCode = input.ReworkOperationCode?.Trim(), EvidenceUrl = input.EvidenceUrl?.Trim(), InspectedByUserId = actor.Id, InspectedAt = DateTime.UtcNow, ApprovedByUserId = actor.Id, ApprovedAt = DateTime.UtcNow, Note = input.Note?.Trim() }; _context.ProductionQualityInspections.Add(inspection); if (order != null) { order.Status = result == QualityPass ? StatusQualityChecked : result == QualityRework ? ProductionWorkOrder.StatusRework : result == QualityReject ? StatusHold : order.Status; order.ActualOutputWeight = input.MeasuredGrossWeight; } return inspection; }, cancellationToken);
        }

        public Task<ProductionReceipt> ReleaseWorkOrderAsync(int workOrderId, ReleaseProductionWorkOrderInput input, string actorUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            return ExecuteSerializableAsync(async () =>
            {
                var actor = await RequireActorAsync(actorUserId, cancellationToken);
                var order = await RequireWorkOrder(workOrderId, actorUserId, cancellationToken);
                if (order.CustomerJobOrderId.HasValue) throw Business("Lệnh gia công khách không được nhập vào tồn kho công ty; hãy dùng quy trình QC và bàn giao khách.");
                var inspection = await _context.ProductionQualityInspections.Where(item => item.ProductionWorkOrderId == order.Id).OrderByDescending(item => item.InspectedAt).FirstOrDefaultAsync(cancellationToken) ?? throw Business("Lệnh chưa có biên bản QC.");
                if (inspection.Result != QualityPass || inspection.AppearanceResult != QualityPass) throw Business("Chỉ được nhập kho khi QC và ngoại quan đều đạt.");
                if (input.FineWeight > input.GrossWeight || input.GrossWeight <= 0 || input.Quantity <= 0 || input.GrossWeight > order.IssuedMaterialWeight + 0.0001m) throw Business("Khối lượng nhập kho không hợp lệ.");
                if (await _context.ProductionReceipts.AnyAsync(item => item.ProductionWorkOrderId == order.Id, cancellationToken)) throw Business("Lệnh đã có phiếu nhập thành phẩm.");
                await RequireWarehouseAsync(input.WarehouseId, order.BranchId, cancellationToken);
                var material = order.Product?.ProductLine ?? "Gold";
                var inventory = new InventoryItem { StockCode = input.ReceiptCode.Trim(), WarehouseId = input.WarehouseId, ProductLine = material, Category = "Thành phẩm chế tác", ProductName = order.Product?.Name ?? ("Thành phẩm " + order.WorkOrderCode), MaterialType = material, QuantityOnHand = input.Quantity, WeightOnHand = input.GrossWeight, UnitCost = input.UnitCost, Status = InventoryItem.StatusAvailable, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
                _context.InventoryItems.Add(inventory);
                var receipt = new ProductionReceipt { ReceiptCode = input.ReceiptCode.Trim(), ProductionWorkOrderId = order.Id, ProductionQualityInspectionId = inspection.Id, WarehouseId = input.WarehouseId, InventoryItem = inventory, Quantity = input.Quantity, GrossWeight = input.GrossWeight, FineWeight = input.FineWeight, UnitCost = input.UnitCost, TotalCost = input.UnitCost * input.Quantity, Status = ProductionReceipt.StatusPosted, CreatedByUserId = actor.Id, CreatedAt = DateTime.UtcNow, PostedByUserId = actor.Id, PostedAt = DateTime.UtcNow, Note = input.Note?.Trim() };
                _context.ProductionReceipts.Add(receipt);
                _context.InventoryTransactions.Add(AddInventoryTransaction(inventory, input.GrossWeight, InventoryTransaction.TypeProductionReceipt, order.Id, actor.Id, "Nhập thành phẩm " + order.WorkOrderCode));
                order.CompletedQuantity = input.Quantity; order.ActualOutputWeight = input.GrossWeight; order.TotalCost = receipt.TotalCost; order.Status = ProductionWorkOrder.StatusClosed; order.ClosedByUserId = actor.Id; order.ClosedAt = DateTime.UtcNow;
                AddStatus(order, StatusQualityChecked, StatusClosed, "Nhập kho thành phẩm", actor.Id, true);
                return receipt;
            }, cancellationToken);
        }

        public Task<ProductionWorkOrder> ChangeWorkOrderStatusAsync(int workOrderId, ChangeProductionWorkOrderStatusInput input, string actorUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            return ExecuteSerializableAsync(async () => { var actor = await RequireActorAsync(actorUserId, cancellationToken); var order = await RequireWorkOrder(workOrderId, actorUserId, cancellationToken); var allowed = new[] { StatusInProgress, StatusHold, StatusCancelled, ProductionWorkOrder.StatusRework }; if (!allowed.Contains(input.TargetStatus)) throw Business("Trạng thái chuyển không được phép qua thao tác này."); var old = order.Status; order.Status = input.TargetStatus; order.HoldReason = input.TargetStatus == StatusHold ? input.Reason.Trim() : null; if (input.TargetStatus == StatusCancelled) { order.ClosedByUserId = actor.Id; order.ClosedAt = DateTime.UtcNow; } AddStatus(order, old, input.TargetStatus, input.Reason.Trim(), actor.Id, false); return order; }, cancellationToken);
        }

        public Task<CustomerJobOrder> CreateCustomerJobOrderAsync(CreateCustomerJobOrderInput input, string actorUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            return ExecuteSerializableAsync(async () => { var actor = await RequireActorAsync(actorUserId, cancellationToken); await RequireActiveBranchAsync(input.BranchId, cancellationToken); await RequireWorkshopAsync(input.WorkshopId, input.BranchId, cancellationToken); if (await _context.CustomerJobOrders.AnyAsync(item => item.JobOrderCode == input.JobOrderCode, cancellationToken)) throw Business("Mã đơn gia công khách đã tồn tại."); if (input.InputFineWeight > input.InputGrossWeight) throw Business("Khối lượng tinh không hợp lệ."); var materialType = NormalizeMaterialType(input.MaterialType); var job = new CustomerJobOrder { JobOrderCode = input.JobOrderCode.Trim(), BranchId = input.BranchId, WorkshopId = input.WorkshopId, CustomerName = input.CustomerName.Trim(), CustomerPhone = input.CustomerPhone.Trim(), CustomerIdentityReference = input.CustomerIdentityReference?.Trim(), JobType = input.JobType, MaterialType = materialType, InputGrossWeight = input.InputGrossWeight, InputFineWeight = input.InputFineWeight, InputPurityRate = input.InputPurityRate, MaterialCondition = input.MaterialCondition.Trim(), IntakeImageUrl = input.IntakeImageUrl.Trim(), CustomerOwnedStorageLocation = input.CustomerOwnedStorageLocation.Trim(), AgreedLossRate = input.AgreedLossRate, DesignDescription = input.DesignDescription.Trim(), DesignImageUrl = input.DesignImageUrl?.Trim(), DesignApprovalReference = input.DesignApprovalReference?.Trim(), DesignApprovedAt = input.DesignApprovedAt?.ToUniversalTime(), QuotedLaborCost = input.QuotedLaborCost, QuotedAdditionalMaterialCost = input.QuotedAdditionalMaterialCost, QuotedTotalAmount = input.QuotedTotalAmount, DepositAmount = input.DepositAmount, PromisedAt = input.PromisedAt.ToUniversalTime(), Status = CustomerJobOrder.StatusReceived, CreatedByUserId = actor.Id, CreatedAt = DateTime.UtcNow, UpdatedByUserId = actor.Id, UpdatedAt = DateTime.UtcNow, Note = input.Note?.Trim() }; _context.CustomerJobOrders.Add(job); _context.CustomerMaterialCustodyRecords.Add(new CustomerMaterialCustodyRecord { CustomerJobOrder = job, BranchId = input.BranchId, MaterialType = materialType, InputGrossWeight = input.InputGrossWeight, InputFineWeight = input.InputFineWeight, InputPurityRate = input.InputPurityRate, StorageLocation = input.CustomerOwnedStorageLocation.Trim(), IntakeEvidenceUrl = input.IntakeImageUrl.Trim(), Status = CustomerMaterialCustodyRecord.StatusHeld, CreatedByUserId = actor.Id, CreatedAt = DateTime.UtcNow, UpdatedByUserId = actor.Id, UpdatedAt = DateTime.UtcNow, Note = input.Note?.Trim() }); AddStatus(job, null, CustomerJobOrder.StatusReceived, "Tiếp nhận và niêm phong tài sản khách", actor.Id, true); return job; }, cancellationToken);
        }

        public Task<CustomerMaterialCustodyRecord> RecordCustomerMaterialIssueAsync(RecordCustomerMaterialIssueInput input, string actorUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            return ExecuteSerializableAsync(async () =>
            {
                var actor = await RequireActorAsync(actorUserId, cancellationToken);
                var job = await _context.CustomerJobOrders.FirstOrDefaultAsync(item => item.Id == input.CustomerJobOrderId, cancellationToken) ?? throw Business("Không tìm thấy đơn gia công khách.");
                await RequireActorBranchAsync(actor, job.BranchId);
                var custody = await _context.CustomerMaterialCustodyRecords.FirstOrDefaultAsync(item => item.CustomerJobOrderId == job.Id, cancellationToken) ?? throw Business("Đơn khách chưa có sổ tài sản.");
                if (custody.Status != CustomerMaterialCustodyRecord.StatusHeld) throw Business("Tài sản khách đã được giao vào xưởng.");
                if (input.IssuedFineWeight > input.IssuedGrossWeight || input.IssuedGrossWeight > custody.InputGrossWeight + 0.0001m) throw Business("Khối lượng giao vật liệu khách không hợp lệ.");
                EnsureCloseFineWeight(input.IssuedFineWeight, FineWeight(input.IssuedGrossWeight, input.IssuedPurityRate));
                custody.IssuedGrossWeight = input.IssuedGrossWeight;
                custody.IssuedFineWeight = input.IssuedFineWeight;
                custody.IssuedPurityRate = input.IssuedPurityRate;
                custody.StorageLocation = input.StorageLocation.Trim();
                custody.IntakeEvidenceUrl = input.EvidenceUrl.Trim();
                custody.Status = CustomerMaterialCustodyRecord.StatusInProduction;
                custody.UpdatedByUserId = actor.Id;
                custody.UpdatedAt = DateTime.UtcNow;
                custody.Note = input.Note?.Trim();
                return custody;
            }, cancellationToken);
        }

        public Task<AttachProductionEvidenceInput> AttachEvidenceAsync(AttachProductionEvidenceInput input, string actorUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            return ExecuteSerializableAsync(async () =>
            {
                var actor = await RequireActorAsync(actorUserId, cancellationToken);
                if (string.Equals(input.EntityType, "QualityInspection", StringComparison.OrdinalIgnoreCase))
                {
                    var inspection = await _context.ProductionQualityInspections.Include(item => item.ProductionWorkOrder).Include(item => item.CustomerJobOrder).FirstOrDefaultAsync(item => item.Id == input.EntityId, cancellationToken) ?? throw Business("Không tìm thấy biên bản QC.");
                    var branchId = inspection.ProductionWorkOrder?.BranchId ?? inspection.CustomerJobOrder?.BranchId ?? 0;
                    await RequireActorBranchAsync(actor, branchId);
                    inspection.EvidenceUrl = input.EvidenceUrl.Trim();
                }
                else if (string.Equals(input.EntityType, "CustomerIntake", StringComparison.OrdinalIgnoreCase) || string.Equals(input.EntityType, "CustomerHandover", StringComparison.OrdinalIgnoreCase))
                {
                    var job = await _context.CustomerJobOrders.FirstOrDefaultAsync(item => item.Id == input.EntityId, cancellationToken) ?? throw Business("Không tìm thấy đơn gia công khách.");
                    await RequireActorBranchAsync(actor, job.BranchId);
                    if (string.Equals(input.EntityType, "CustomerIntake", StringComparison.OrdinalIgnoreCase))
                    {
                        job.IntakeImageUrl = input.EvidenceUrl.Trim();
                        var custody = await _context.CustomerMaterialCustodyRecords.FirstOrDefaultAsync(item => item.CustomerJobOrderId == job.Id, cancellationToken);
                        if (custody != null) custody.IntakeEvidenceUrl = input.EvidenceUrl.Trim();
                    }
                    else
                    {
                        job.HandoverEvidenceUrl = input.EvidenceUrl.Trim();
                        var custody = await _context.CustomerMaterialCustodyRecords.FirstOrDefaultAsync(item => item.CustomerJobOrderId == job.Id, cancellationToken);
                        if (custody != null) custody.ReturnEvidenceUrl = input.EvidenceUrl.Trim();
                    }
                }
                else throw Business("Loại đối tượng upload không hợp lệ.");
                return input;
            }, cancellationToken);
        }

        public Task<ProductionQualityInspection> RecordCustomerJobQualityAsync(int customerJobOrderId, RecordCustomerJobQualityInput input, string actorUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            return ExecuteSerializableAsync(async () =>
            {
                var actor = await RequireActorAsync(actorUserId, cancellationToken);
                var job = await _context.CustomerJobOrders.FirstOrDefaultAsync(item => item.Id == customerJobOrderId, cancellationToken) ?? throw Business("Không tìm thấy đơn gia công khách.");
                var custody = await _context.CustomerMaterialCustodyRecords.FirstOrDefaultAsync(item => item.CustomerJobOrderId == job.Id, cancellationToken) ?? throw Business("Đơn khách chưa có sổ bàn giao tài sản.");
                await RequireActorBranchAsync(actor, job.BranchId);
                var previousStatus = job.Status;
                if (job.Status != CustomerJobOrder.StatusInProduction && job.Status != CustomerJobOrder.StatusRework) throw Business("Đơn gia công khách chưa ở trạng thái chờ QC.");
                if (await _context.ProductionQualityInspections.AnyAsync(item => item.InspectionCode == input.InspectionCode, cancellationToken)) throw Business("Mã kiểm phẩm đã tồn tại.");
                var allowedOutputLoss = custody.IssuedGrossWeight * job.AgreedLossRate / 100m;
                if (input.OutputFineWeight > input.OutputGrossWeight || input.OutputGrossWeight > custody.IssuedGrossWeight + 0.0001m || custody.IssuedGrossWeight - input.OutputGrossWeight > allowedOutputLoss + 0.0001m) throw Business("Khối lượng thành phẩm khách vượt mức hao hụt đã thỏa thuận.");
                EnsureCloseFineWeight(input.OutputFineWeight, FineWeight(input.OutputGrossWeight, input.OutputPurityRate));
                var result = NormalizeQualityResult(input.Result);
                var appearance = NormalizeQualityResult(input.AppearanceResult);
                if (result == QualityPass && appearance != QualityPass) throw Business("QC đạt phải đồng thời đạt ngoại quan.");
                if (result == QualityRework && string.IsNullOrWhiteSpace(input.ReworkOperationCode)) throw Business("QC cần làm lại phải chỉ rõ công đoạn.");
                job.Status = result == QualityPass ? CustomerJobOrder.StatusReadyForHandover : result == QualityRework ? CustomerJobOrder.StatusRework : CustomerJobOrder.StatusQualityChecked;
                job.OutputGrossWeight = input.OutputGrossWeight;
                job.OutputFineWeight = input.OutputFineWeight;
                job.OutputPurityRate = input.OutputPurityRate;
                job.QualityResult = result;
                job.UpdatedByUserId = actor.Id;
                job.UpdatedAt = DateTime.UtcNow;
                custody.OutputGrossWeight = input.OutputGrossWeight;
                custody.OutputFineWeight = input.OutputFineWeight;
                custody.OutputPurityRate = input.OutputPurityRate;
                custody.Status = result == QualityPass ? CustomerMaterialCustodyRecord.StatusReadyForReturn : CustomerMaterialCustodyRecord.StatusInProduction;
                custody.UpdatedByUserId = actor.Id;
                custody.UpdatedAt = DateTime.UtcNow;
                var inspection = new ProductionQualityInspection { InspectionCode = input.InspectionCode.Trim(), CustomerJobOrderId = job.Id, InspectionType = ProductionQualityInspection.TypeCustomerJob, MeasuredGrossWeight = input.OutputGrossWeight, MeasuredFineWeight = input.OutputFineWeight, MeasuredPurityRate = input.OutputPurityRate, AppearanceResult = appearance, Result = result, ReworkOperationCode = input.ReworkOperationCode?.Trim(), EvidenceUrl = input.EvidenceUrl?.Trim(), InspectedByUserId = actor.Id, InspectedAt = DateTime.UtcNow, ApprovedByUserId = result == QualityPass ? actor.Id : null, ApprovedAt = result == QualityPass ? DateTime.UtcNow : null, Note = input.Note?.Trim() };
                _context.ProductionQualityInspections.Add(inspection);
                AddStatus(job, previousStatus, job.Status, "Kiểm tra và cập nhật sổ tài sản khách", actor.Id, true);
                return inspection;
            }, cancellationToken);
        }

        public Task<CustomerJobOrder> CompleteCustomerJobHandoverAsync(int customerJobOrderId, CompleteCustomerJobHandoverInput input, string actorUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            return ExecuteSerializableAsync(async () =>
            {
                var actor = await RequireActorAsync(actorUserId, cancellationToken);
                var job = await _context.CustomerJobOrders.FirstOrDefaultAsync(item => item.Id == customerJobOrderId, cancellationToken) ?? throw Business("Không tìm thấy đơn gia công khách.");
                var custody = await _context.CustomerMaterialCustodyRecords.FirstOrDefaultAsync(item => item.CustomerJobOrderId == job.Id, cancellationToken) ?? throw Business("Đơn khách chưa có sổ bàn giao tài sản.");
                await RequireActorBranchAsync(actor, job.BranchId);
                if (job.Status != CustomerJobOrder.StatusReadyForHandover || custody.Status != CustomerMaterialCustodyRecord.StatusReadyForReturn) throw Business("Đơn chưa đạt QC để bàn giao.");
                if (input.FinalAmount < job.DepositAmount || input.HandoverAt > DateTime.Now.AddMinutes(5)) throw Business("Số tiền quyết toán hoặc thời điểm bàn giao không hợp lệ.");
                job.Status = CustomerJobOrder.StatusHandedOver;
                job.FinalAmount = input.FinalAmount;
                job.HandoverReceiverName = input.ReceiverName.Trim();
                job.HandoverEvidenceUrl = input.EvidenceUrl.Trim();
                job.HandoverAt = input.HandoverAt.ToUniversalTime();
                job.HandedOverByUserId = actor.Id;
                job.UpdatedByUserId = actor.Id;
                job.UpdatedAt = DateTime.UtcNow;
                job.Note = input.Note?.Trim();
                custody.ReturnedGrossWeight = custody.OutputGrossWeight;
                custody.ReturnEvidenceUrl = input.EvidenceUrl.Trim();
                custody.ReturnedByUserId = actor.Id;
                custody.ReturnedAt = input.HandoverAt.ToUniversalTime();
                custody.Status = CustomerMaterialCustodyRecord.StatusReturned;
                custody.UpdatedByUserId = actor.Id;
                custody.UpdatedAt = DateTime.UtcNow;
                AddStatus(job, CustomerJobOrder.StatusReadyForHandover, CustomerJobOrder.StatusHandedOver, "Bàn giao và đóng sổ tài sản khách", actor.Id, true);
                return job;
            }, cancellationToken);
        }

        public Task<ProductionRecycleBatch> CreateRecycleBatchAsync(CreateProductionRecycleBatchInput input, string actorUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            return ExecuteSerializableAsync(async () => { var actor = await RequireActorAsync(actorUserId, cancellationToken); await RequireActiveBranchAsync(input.BranchId, cancellationToken); await RequireWorkshopAsync(input.WorkshopId, input.BranchId, cancellationToken); if (await _context.ProductionRecycleBatches.AnyAsync(item => item.BatchCode == input.BatchCode, cancellationToken)) throw Business("Mã mẻ tái chế đã tồn tại."); var losses = await _context.ProductionLossRecords.Include(item => item.ProductionWorkOrder).Where(item => input.ProductionLossRecordIds.Contains(item.Id) && item.Status == ProductionLossRecord.StatusApproved && item.ProductionRecycleBatchId == null).ToListAsync(cancellationToken); if (losses.Count != input.ProductionLossRecordIds.Count || losses.Any(item => item.ProductionWorkOrder.BranchId != input.BranchId || !string.Equals(item.ProductionWorkOrder.ProductionBom.Product.ProductLine, input.MaterialType, StringComparison.OrdinalIgnoreCase))) throw Business("Có hao hụt không đủ điều kiện đưa vào tái chế."); var calculatedGross = losses.Sum(item => item.LossWeight); if (Math.Abs(calculatedGross - input.InputGrossWeight) > 0.0001m || input.InputFineWeight > input.InputGrossWeight) throw Business("Khối lượng mẻ tái chế phải khớp tổng hao hụt đã duyệt."); var batch = new ProductionRecycleBatch { BatchCode = input.BatchCode.Trim(), BranchId = input.BranchId, WorkshopId = input.WorkshopId, MaterialType = input.MaterialType.Trim(), SourceType = input.SourceType, InputGrossWeight = input.InputGrossWeight, InputFineWeight = input.InputFineWeight, Status = ProductionRecycleBatch.StatusCollected, CreatedByUserId = actor.Id, CreatedAt = DateTime.UtcNow, UpdatedByUserId = actor.Id, UpdatedAt = DateTime.UtcNow, Note = input.Note?.Trim() }; foreach (var loss in losses) loss.ProductionRecycleBatch = batch; _context.ProductionRecycleBatches.Add(batch); AddStatus(batch, null, ProductionRecycleBatch.StatusCollected, "Thu gom phế liệu tái chế", actor.Id, true); return batch; }, cancellationToken);
        }

        public Task<ProductionRecycleBatch> CompleteRecycleBatchAsync(int recycleBatchId, CompleteProductionRecycleBatchInput input, string actorUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            return ExecuteSerializableAsync(async () => { var actor = await RequireActorAsync(actorUserId, cancellationToken); var batch = await _context.ProductionRecycleBatches.FirstOrDefaultAsync(item => item.Id == recycleBatchId, cancellationToken) ?? throw Business("Không tìm thấy mẻ tái chế."); if (batch.Status != ProductionRecycleBatch.StatusCollected && batch.Status != ProductionRecycleBatch.StatusInRefining) throw Business("Mẻ tái chế không ở trạng thái xử lý."); if (await _context.ProductionQualityInspections.AnyAsync(item => item.InspectionCode == input.InspectionCode, cancellationToken)) throw Business("Mã kiểm phẩm đã tồn tại."); if (input.OutputFineWeight > input.OutputGrossWeight || input.OutputGrossWeight > batch.InputGrossWeight + 0.0001m) throw Business("Khối lượng đầu ra tái chế không hợp lệ."); var warehouse = await RequireWarehouseAsync(input.OutputWarehouseId, batch.BranchId, cancellationToken); var inventory = await _context.InventoryItems.FirstOrDefaultAsync(item => item.Id == input.OutputInventoryItemId && item.WarehouseId == warehouse.Id, cancellationToken) ?? throw Business("Mã tồn đầu ra không thuộc kho đã chọn."); if (!string.Equals(inventory.MaterialType, batch.MaterialType, StringComparison.OrdinalIgnoreCase) && !string.Equals(inventory.ProductLine, batch.MaterialType, StringComparison.OrdinalIgnoreCase)) throw Business("Vật liệu tồn đầu ra không khớp mẻ tái chế."); var appearance = NormalizeQualityResult(input.AppearanceResult); if (appearance != QualityPass) throw Business("Lô tái chế chỉ được phát hành khi ngoại quan đạt."); if (await _context.RawMaterialLots.AnyAsync(item => item.LotCode == input.OutputLotCode, cancellationToken)) throw Business("Mã lô nguyên liệu đầu ra đã tồn tại."); var inspection = new ProductionQualityInspection { InspectionCode = input.InspectionCode.Trim(), ProductionRecycleBatchId = batch.Id, InspectionType = ProductionQualityInspection.TypeRecycle, MeasuredGrossWeight = input.OutputGrossWeight, MeasuredFineWeight = input.OutputFineWeight, MeasuredPurityRate = input.OutputPurityRate, AppearanceResult = appearance, Result = QualityPass, EvidenceUrl = input.EvidenceUrl?.Trim(), InspectedByUserId = actor.Id, InspectedAt = DateTime.UtcNow, ApprovedByUserId = actor.Id, ApprovedAt = DateTime.UtcNow, Note = input.Note?.Trim() }; var lot = new RawMaterialLot { LotCode = input.OutputLotCode.Trim(), BranchId = batch.BranchId, WarehouseId = input.OutputWarehouseId, InventoryItemId = input.OutputInventoryItemId, MaterialType = batch.MaterialType, PurityRate = input.OutputPurityRate, GrossWeight = input.OutputGrossWeight, FineWeight = input.OutputFineWeight, AvailableWeight = input.OutputGrossWeight, SourceType = RawMaterialLot.SourceRecycle, SourceReference = batch.BatchCode, UnitCost = input.OutputUnitCost, Status = RawMaterialLot.StatusReleased, QualityStatus = RawMaterialLot.QualityPass, CreatedByUserId = actor.Id, CreatedAt = DateTime.UtcNow, ReleasedByUserId = actor.Id, ReleasedAt = DateTime.UtcNow, UpdatedByUserId = actor.Id, UpdatedAt = DateTime.UtcNow }; inventory.WeightOnHand += input.OutputGrossWeight; inventory.UpdatedAt = DateTime.UtcNow; batch.OutputGrossWeight = input.OutputGrossWeight; batch.OutputFineWeight = input.OutputFineWeight; batch.OutputPurityRate = input.OutputPurityRate; batch.OutputRawMaterialLot = lot; batch.Status = ProductionRecycleBatch.StatusReleased; batch.ReleasedByUserId = actor.Id; batch.ReleasedAt = DateTime.UtcNow; _context.ProductionQualityInspections.Add(inspection); _context.RawMaterialLots.Add(lot); _context.InventoryTransactions.Add(AddInventoryTransaction(inventory, input.OutputGrossWeight, InventoryTransaction.TypeProductionReceipt, batch.Id, actor.Id, "Nhập nguyên liệu tái chế " + batch.BatchCode)); AddStatus(batch, ProductionRecycleBatch.StatusCollected, ProductionRecycleBatch.StatusReleased, "QC đạt và tạo lô nguyên liệu tái chế", actor.Id, true); return batch; }, cancellationToken);
        }

        private async Task<AppUser> RequireActorAsync(string userId, CancellationToken cancellationToken) => await _context.Users.FirstOrDefaultAsync(item => item.Id == userId, cancellationToken) ?? throw Business("Tài khoản thực hiện không tồn tại.");
        private async Task RequireActiveBranchAsync(int branchId, CancellationToken cancellationToken) { if (!await _context.Branches.AnyAsync(item => item.Id == branchId, cancellationToken)) throw Business("Chi nhánh không tồn tại."); }
        private async Task RequireActorBranchAsync(AppUser actor, int branchId) { if (actor.BranchId.HasValue && actor.BranchId != branchId && !await _userManager.IsInRoleAsync(actor, RoleCatalog.Admin)) throw Business("Không được thao tác dữ liệu của chi nhánh khác."); }
        private async Task<ProductionWorkshop> RequireWorkshopAsync(int id, int branchId, CancellationToken cancellationToken) => await _context.ProductionWorkshops.FirstOrDefaultAsync(item => item.Id == id && item.BranchId == branchId && item.IsActive && item.IsProductionAuthorized, cancellationToken) ?? throw Business("Xưởng chưa được xác minh hoặc không thuộc chi nhánh.");
        private async Task<Warehouse> RequireWarehouseAsync(int id, int branchId, CancellationToken cancellationToken) => await _context.Warehouses.FirstOrDefaultAsync(item => item.Id == id && item.BranchId == branchId, cancellationToken) ?? throw Business("Kho không thuộc chi nhánh.");
        private async Task<ProductionWorkshop> RequireWorkshopAsync(int id, CancellationToken cancellationToken) => await _context.ProductionWorkshops.FirstOrDefaultAsync(item => item.Id == id, cancellationToken) ?? throw Business("Không tìm thấy xưởng chế tác.");
        private async Task<ProductionWorkOrder> RequireWorkOrder(int id, string actorUserId, CancellationToken cancellationToken)
        {
            var order = await _context.ProductionWorkOrders.Include(item => item.ProductionBom).ThenInclude(item => item.Product).Include(item => item.Product).FirstOrDefaultAsync(item => item.Id == id, cancellationToken) ?? throw Business("Không tìm thấy lệnh sản xuất.");
            var actor = await RequireActorAsync(actorUserId, cancellationToken);
            if (actor.BranchId.HasValue && actor.BranchId.Value != order.BranchId && !await _userManager.IsInRoleAsync(actor, RoleCatalog.Admin)) throw Business("Không được thao tác lệnh của chi nhánh khác.");
            return order;
        }
        private static ProductionBusinessException Business(string message) => new(message);
        private static decimal FineWeight(decimal gross, decimal purity) => Math.Round(gross * purity, 4);
        private static decimal RoundGrams(decimal value) => Math.Round(value, 4, MidpointRounding.AwayFromZero);
        private static bool IsPass(string value) => string.Equals(value, QualityPass, StringComparison.OrdinalIgnoreCase);
        private static string NormalizeQualityResult(string value) => value?.Trim() switch { "Pass" => QualityPass, "Rework" => QualityRework, "Reject" => QualityReject, _ => QualityPending };
        private static void ValidatePurity(decimal value, string label) { if (value <= 0 || value > 1) throw Business(label + " phải trong khoảng 0 đến 1."); }
        private static void ValidateWorkshopLicense(bool authorized, string number, DateTime? from, DateTime? to, DateTime now) { if (!authorized) return; if (string.IsNullOrWhiteSpace(number) || !from.HasValue || !to.HasValue || from > to || from > now || to < now) throw Business("Xưởng được cấp phép phải có thông tin giấy phép hợp lệ."); }
        private static void ValidatePolicyValues(CreateProductionLossPolicyInput input) { ValidatePolicyValues(input.MinimumPurityRate, input.MaximumPurityRate, input.MaximumLossRate, input.ApprovalWeightLimit, input.ApprovalAmountLimit, input.EffectiveFrom, input.EffectiveTo); }
        private static void ValidatePolicyValues(decimal minimumPurity, decimal maximumPurity, decimal maximumLoss, decimal approvalWeight, decimal approvalAmount, DateTime effectiveFrom, DateTime? effectiveTo) { if (minimumPurity <= 0 || maximumPurity < minimumPurity || maximumPurity > 1 || maximumLoss < 0 || approvalWeight < 0 || approvalAmount < 0 || (effectiveTo.HasValue && effectiveTo <= effectiveFrom)) throw Business("Thông số chính sách hao hụt không hợp lệ."); }
        private sealed record LossPolicyValues(string Code, string MaterialType, decimal MinimumPurityRate, decimal MaximumPurityRate, string OperationCode, decimal MaximumLossRate, decimal ApprovalWeightLimit, decimal ApprovalAmountLimit, string Version, DateTime EffectiveFrom, DateTime? EffectiveTo, string Note);
        private static LossPolicyValues ReadAndValidateLossPolicy(CreateProductionLossPolicyInput input) { ValidatePolicyValues(input); if (string.IsNullOrWhiteSpace(input.PolicyCode) || string.IsNullOrWhiteSpace(input.MaterialType) || string.IsNullOrWhiteSpace(input.Version)) throw Business("Thông tin chính sách hao hụt là bắt buộc."); return new(input.PolicyCode.Trim(), NormalizeMaterialType(input.MaterialType), input.MinimumPurityRate, input.MaximumPurityRate, input.OperationCode?.Trim(), input.MaximumLossRate, input.ApprovalWeightLimit, input.ApprovalAmountLimit, input.Version.Trim(), input.EffectiveFrom.ToUniversalTime(), input.EffectiveTo?.ToUniversalTime(), input.Note?.Trim()); }
        private static void ValidateBom(CreateProductionBomInput input) { if (input.Items == null || input.Items.Count == 0 || input.Operations == null || input.Operations.Count == 0) throw Business("BOM phải có ít nhất một vật tư và một công đoạn."); if (input.EffectiveTo.HasValue && input.EffectiveTo.Value < input.EffectiveFrom) throw Business("Thời hạn BOM không hợp lệ."); if (input.Items.GroupBy(item => item.SequenceNumber).Any(group => group.Count() > 1) || input.Operations.GroupBy(item => item.SequenceNumber).Any(group => group.Count() > 1)) throw Business("Thứ tự BOM không được trùng."); if (input.Items.Any(item => string.IsNullOrWhiteSpace(item.MaterialType) || item.RequiredWeight <= 0) || input.Operations.Any(item => string.IsNullOrWhiteSpace(item.OperationCode) || string.IsNullOrWhiteSpace(item.OperationName) || item.StandardMinutes <= 0)) throw Business("Vật tư và công đoạn BOM phải có đủ thông tin hợp lệ."); if (input.Operations.GroupBy(item => item.OperationCode.Trim(), StringComparer.OrdinalIgnoreCase).Any(group => group.Count() > 1)) throw Business("Mã công đoạn BOM không được trùng."); }
        private static string NormalizeMaterialType(string material) => material?.Trim().ToLowerInvariant() switch { "vàng" or "gold" => ProductLineOptions.Gold, "bạc" or "silver" => ProductLineOptions.Silver, _ => material?.Trim() ?? string.Empty };
        private static T Read<T>(object input, T fallback, params string[] names) { foreach (var name in names) { var property = input.GetType().GetProperty(name); if (property?.GetValue(input) is T value) return value; } return fallback; }
        private static int Read(object input, int fallback, params string[] names) => Read<int>(input, fallback, names);
        private static bool Read(object input, bool fallback, params string[] names) => Read<bool>(input, fallback, names);
        private static decimal Read(object input, decimal fallback, params string[] names) => Read<decimal>(input, fallback, names);
        private static int? Read(object input, int? fallback, params string[] names) => Read<int?>(input, fallback, names);
        private static DateTime? Read(object input, DateTime? fallback, params string[] names) => Read<DateTime?>(input, fallback, names);
        private static string ReadRequiredText(object input, int maxLength, params string[] names) { var value = ReadOptionalText(input, maxLength, names); if (string.IsNullOrWhiteSpace(value)) throw Business(names[0] + " là bắt buộc."); return value; }
        private static string ReadOptionalText(object input, int maxLength, params string[] names) { foreach (var name in names) { var value = input.GetType().GetProperty(name)?.GetValue(input)?.ToString()?.Trim(); if (!string.IsNullOrWhiteSpace(value)) { if (value.Length > maxLength) throw Business(name + " vượt quá độ dài cho phép."); return value; } } return null; }
        private static int ReadRequiredInt(object input, params string[] names) { var value = Read(input, 0, names); if (value <= 0) throw Business(names[0] + " là bắt buộc."); return value; }
        private static decimal ReadRequiredPositiveDecimal(object input, params string[] names) { var value = Read(input, 0m, names); if (value <= 0) throw Business(names[0] + " phải lớn hơn 0."); return value; }
        private static decimal ReadNonNegativeDecimal(object input, params string[] names) { var value = Read(input, 0m, names); if (value < 0) throw Business(names[0] + " không được âm."); return value; }
        private static decimal ReadRequiredPurity(object input, params string[] names) { var value = ReadRequiredPositiveDecimal(input, names); ValidatePurity(value, names[0]); return value; }
        private static void EnsureCloseFineWeight(decimal supplied, decimal calculated) { if (Math.Abs(supplied - calculated) > 0.01m) throw Business("Khối lượng tinh không khớp hàm lượng và khối lượng tổng."); }
        private static DateTime? ReadNullableUtc(object input, params string[] names) { var value = Read<DateTime?>(input, null, names); return value?.ToUniversalTime(); }
        private static int? ReadNullableInt(object input, params string[] names) => Read<int?>(input, null, names);
        private async Task<ProductionLossPolicy> ReadAndValidateLossPolicy(int branchId, string material, int? operationId, CancellationToken cancellationToken)
        {
            var operationCode = operationId.HasValue
                ? await _context.ProductionOperationLogs.Where(item => item.Id == operationId.Value).Select(item => item.OperationCode).FirstOrDefaultAsync(cancellationToken)
                : null;
            return await _context.ProductionLossPolicies.Where(item => item.BranchId == branchId && item.MaterialType == material && (string.IsNullOrEmpty(operationCode) || item.OperationCode == operationCode) && item.Status == ProductionLossPolicy.StatusActive && item.EffectiveFrom <= DateTime.UtcNow && (item.EffectiveTo == null || item.EffectiveTo > DateTime.UtcNow)).OrderByDescending(item => item.EffectiveFrom).FirstOrDefaultAsync(cancellationToken);
        }
        private InventoryTransaction AddInventoryTransaction(InventoryItem item, decimal weightChange, string type, int referenceId, string userId, string note) => new() { TransactionCode = $"PRD-{Guid.NewGuid():N}"[..40], WarehouseId = item.WarehouseId, InventoryItem = item, TransactionType = note.Contains("tái chế", StringComparison.OrdinalIgnoreCase) ? InventoryTransaction.TypeRecycleReceipt : type, QuantityChange = 0, WeightChange = weightChange, QuantityAfter = item.QuantityOnHand, WeightAfter = item.WeightOnHand, ReferenceType = "ProductionWorkOrder", ReferenceId = referenceId, CreatedByUserId = userId, CreatedAt = DateTime.UtcNow, Note = note };
        private void AddStatus(ProductionWorkOrder entity, string from, string to, string reason, string userId, bool system) => _context.ProductionStatusHistories.Add(new ProductionStatusHistory { EntityType = ProductionStatusHistory.EntityWorkOrder, EntityId = entity.Id, ProductionWorkOrder = entity, FromStatus = from, ToStatus = to, Reason = reason, ChangedByUserId = userId, ChangedAt = DateTime.UtcNow, IsSystemGenerated = system });
        private void AddStatus(CustomerJobOrder entity, string from, string to, string reason, string userId, bool system) => _context.ProductionStatusHistories.Add(new ProductionStatusHistory { EntityType = ProductionStatusHistory.EntityCustomerJob, EntityId = entity.Id, CustomerJobOrder = entity, FromStatus = from, ToStatus = to, Reason = reason, ChangedByUserId = userId, ChangedAt = DateTime.UtcNow, IsSystemGenerated = system });
        private void AddStatus(ProductionRecycleBatch entity, string from, string to, string reason, string userId, bool system) => _context.ProductionStatusHistories.Add(new ProductionStatusHistory { EntityType = ProductionStatusHistory.EntityRecycleBatch, EntityId = entity.Id, ProductionRecycleBatch = entity, FromStatus = from, ToStatus = to, Reason = reason, ChangedByUserId = userId, ChangedAt = DateTime.UtcNow, IsSystemGenerated = system });
        private async Task<T> ExecuteSerializableAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            var result = await action();
            await SyncWipInventoriesAsync(cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }

        private async Task SyncWipInventoriesAsync(CancellationToken cancellationToken)
        {
            var orders = _context.ChangeTracker.Entries<ProductionWorkOrder>().Select(item => item.Entity).Where(item => item.IssuedMaterialWeight > 0).ToList();
            foreach (var order in orders)
            {
                var weight = order.Status is ProductionWorkOrder.StatusClosed or ProductionWorkOrder.StatusCancelled ? 0 : order.ActualOutputWeight > 0 ? order.ActualOutputWeight : order.IssuedMaterialWeight;
                var wip = order.WipInventoryItemId.HasValue
                    ? await _context.InventoryItems.FirstOrDefaultAsync(item => item.Id == order.WipInventoryItemId.Value, cancellationToken)
                    : null;
                if (wip == null && weight > 0)
                {
                    wip = new InventoryItem { StockCode = "WIP-" + order.WorkOrderCode, WarehouseId = order.MaterialWarehouseId, ProductLine = order.Product?.ProductLine ?? "Gold", Category = "Sản phẩm dở dang", ProductName = "WIP " + order.WorkOrderCode, MaterialType = order.Product?.ProductLine ?? "Gold", QuantityOnHand = 1, WeightOnHand = weight, UnitCost = order.MaterialCost, Status = InventoryItem.StatusWorkInProgress, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
                    order.WipInventoryItem = wip;
                    _context.InventoryItems.Add(wip);
                }
                else if (wip != null)
                {
                    wip.WeightOnHand = weight;
                    wip.Status = weight > 0 ? InventoryItem.StatusWorkInProgress : InventoryItem.StatusDepleted;
                    wip.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}
