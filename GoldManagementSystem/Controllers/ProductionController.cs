using GoldManagementSystem.Data;
using GoldManagementSystem.Models;
using GoldManagementSystem.Models.ViewModels;
using GoldManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace GoldManagementSystem.Controllers
{
    [Authorize]
    public sealed class ProductionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IProductionService _production;
        private readonly IManagementPermissionService _permissions;
        private readonly IWebHostEnvironment _environment;

        public ProductionController(ApplicationDbContext context, UserManager<AppUser> userManager, IProductionService production, IManagementPermissionService permissions, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _production = production;
            _permissions = permissions;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm, int? branchId, int? workshopId, string statusFilter, string activeTab = "work-orders", CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            var branches = await _permissions.GetAccessibleBranchesAsync(User);
            var selectedBranch = branches.FirstOrDefault(item => item.Id == branchId) ?? branches.FirstOrDefault(item => item.Id == user?.BranchId) ?? branches.FirstOrDefault();
            if (selectedBranch == null) return Forbid();
            var branch = selectedBranch.Id;
            var grants = await _permissions.GetGrantedKeysAsync(User, branch);
            var isAdmin = User.IsInRole(RoleCatalog.Admin);
            var model = new ProductionManagementViewModel { SearchTerm = searchTerm, BranchId = branch, WorkshopId = workshopId, StatusFilter = statusFilter, ActiveTab = activeTab, CanView = isAdmin || grants.Contains(ManagementFeatureCatalog.ProductionView), CanOperate = isAdmin || grants.Contains(ManagementFeatureCatalog.ProductionOperate), CanApprove = isAdmin || grants.Contains(ManagementFeatureCatalog.ProductionApprove), CanManageCustomerJobs = isAdmin || grants.Contains(ManagementFeatureCatalog.ProductionCustomerJobs) };
            if (!model.CanView && !model.CanOperate && !model.CanApprove && !model.CanManageCustomerJobs) return Forbid();
            var workshops = _context.ProductionWorkshops.Where(item => item.BranchId == branch);
            if (workshopId.HasValue) workshops = workshops.Where(item => item.Id == workshopId.Value);
            model.Workshops = await workshops.OrderBy(item => item.Name).ToListAsync(cancellationToken);
            model.LossPolicies = await _context.ProductionLossPolicies.Where(item => item.BranchId == branch).OrderByDescending(item => item.EffectiveFrom).ToListAsync(cancellationToken);
            model.RawMaterialLots = await _context.RawMaterialLots.Where(item => item.BranchId == branch).Include(item => item.InventoryItem).OrderByDescending(item => item.CreatedAt).ToListAsync(cancellationToken);
            model.Boms = await _context.ProductionBoms.Where(item => item.BranchId == branch).Include(item => item.Product).Include(item => item.Items).Include(item => item.Operations).OrderByDescending(item => item.CreatedAt).ToListAsync(cancellationToken);
            var orders = _context.ProductionWorkOrders.Where(item => item.BranchId == branch).Include(item => item.Product).Include(item => item.ProductionBom).ThenInclude(item => item.Product).Include(item => item.MaterialReservations).Include(item => item.OperationLogs).Include(item => item.LossRecords).Include(item => item.QualityInspections).OrderByDescending(item => item.CreatedAt);
            if (!string.IsNullOrWhiteSpace(searchTerm)) orders = orders.Where(item => item.WorkOrderCode.Contains(searchTerm) || item.Product.Name.Contains(searchTerm)).OrderByDescending(item => item.CreatedAt);
            if (!string.IsNullOrWhiteSpace(statusFilter)) orders = orders.Where(item => item.Status == statusFilter).OrderByDescending(item => item.CreatedAt);
            model.WorkOrders = await orders.Take(100).ToListAsync(cancellationToken);
            model.LateWorkOrders = model.WorkOrders.Where(item => ProductionMetrics.IsLate(item, DateTime.UtcNow)).ToList();
            model.LossRecords = await _context.ProductionLossRecords.Where(item => item.ProductionWorkOrder.BranchId == branch).Include(item => item.ProductionWorkOrder).OrderByDescending(item => item.ReportedAt).Take(100).ToListAsync(cancellationToken);
            model.OverToleranceLosses = model.LossRecords.Where(ProductionMetrics.IsOverTolerance).ToList();
            model.QualityInspections = await _context.ProductionQualityInspections.Where(item => item.ProductionWorkOrder.BranchId == branch || item.CustomerJobOrder.BranchId == branch || item.ProductionRecycleBatch.BranchId == branch).OrderByDescending(item => item.InspectedAt).Take(100).ToListAsync(cancellationToken);
            model.Receipts = await _context.ProductionReceipts.Where(item => item.ProductionWorkOrder.BranchId == branch).OrderByDescending(item => item.CreatedAt).Take(100).ToListAsync(cancellationToken);
            model.RecycleBatches = await _context.ProductionRecycleBatches.Where(item => item.BranchId == branch).OrderByDescending(item => item.CreatedAt).ToListAsync(cancellationToken);
            model.CustomerJobOrders = await _context.CustomerJobOrders.Where(item => item.BranchId == branch).OrderByDescending(item => item.CreatedAt).ToListAsync(cancellationToken);
            model.CustomerMaterialCustodyRecords = await _context.CustomerMaterialCustodyRecords.Where(item => item.BranchId == branch).Include(item => item.CustomerJobOrder).OrderByDescending(item => item.CreatedAt).ToListAsync(cancellationToken);
            model.RecentStatusHistories = await _context.ProductionStatusHistories.Where(item => item.ProductionWorkOrder.BranchId == branch || item.CustomerJobOrder.BranchId == branch || item.ProductionRecycleBatch.BranchId == branch).OrderByDescending(item => item.ChangedAt).Take(100).ToListAsync(cancellationToken);
            model.TotalRawMaterialLots = model.RawMaterialLots.Count; model.ReleasedRawMaterialLots = model.RawMaterialLots.Count(item => item.Status == RawMaterialLot.StatusReleased); model.AvailableRawMaterialWeight = model.RawMaterialLots.Sum(item => item.AvailableWeight); model.ActiveWorkOrders = model.WorkOrders.Count(item => item.Status != ProductionWorkOrder.StatusClosed && item.Status != ProductionWorkOrder.StatusCancelled); model.OnHoldWorkOrders = model.WorkOrders.Count(item => item.Status == ProductionWorkOrder.StatusOnHold); model.PendingLossApprovals = model.LossRecords.Count(item => item.Status == ProductionLossRecord.StatusPendingApproval); model.PendingQualityInspections = model.QualityInspections.Count(item => item.Result == ProductionQualityInspection.ResultPending); model.OpenCustomerJobs = model.CustomerJobOrders.Count(item => item.Status != CustomerJobOrder.StatusHandedOver && item.Status != CustomerJobOrder.StatusCancelled); model.RecycleWeight = model.RecycleBatches.Where(item => item.Status != ProductionRecycleBatch.StatusClosed).Sum(item => item.InputGrossWeight); model.WipWeight = model.WorkOrders.Sum(ProductionMetrics.WipWeight);
            model.BranchOptions = branches.Select(item => new SelectListItem(item.BranchName, item.Id.ToString(), item.Id == branch)).ToList(); model.WorkshopOptions = model.Workshops.Select(item => new SelectListItem(item.Name, item.Id.ToString())).ToList(); model.WarehouseOptions = await _context.Warehouses.Where(item => item.BranchId == branch).Select(item => new SelectListItem(item.Name, item.Id.ToString())).ToListAsync(cancellationToken); model.ProductOptions = await _context.Products.Where(item => item.BranchId == branch).Select(item => new SelectListItem(item.Name, item.Id.ToString())).ToListAsync(cancellationToken); model.InventoryItemOptions = await _context.InventoryItems.Where(item => item.Warehouse.BranchId == branch && item.Status != InventoryItem.StatusDepleted).Select(item => new SelectListItem($"{item.StockCode} · {item.ProductName}", item.Id.ToString())).ToListAsync(cancellationToken); model.BomOptions = model.Boms.Where(item => item.Status == ProductionBom.StatusActive).Select(item => new SelectListItem($"{item.BomCode} · {item.Version}", item.Id.ToString())).ToList(); model.RawMaterialLotOptions = model.RawMaterialLots.Where(item => item.Status == RawMaterialLot.StatusReleased).Select(item => new SelectListItem($"{item.LotCode} · {item.AvailableWeight:N4} g", item.Id.ToString())).ToList(); model.ResponsibleUserOptions = (await _userManager.GetUsersInRoleAsync(RoleCatalog.Artisan)).Where(item => item.BranchId == branch).Select(item => new SelectListItem(item.FullName ?? item.UserName, item.Id)).ToList(); model.MaterialOptions = new[] { new SelectListItem("Vàng", ProductLineOptions.Gold), new SelectListItem("Bạc", ProductLineOptions.Silver) }; model.WorkOrderStatusOptions = new[] { ProductionWorkOrder.StatusInProgress, ProductionWorkOrder.StatusOnHold, ProductionWorkOrder.StatusRework, ProductionWorkOrder.StatusCancelled }.Select(item => new SelectListItem(item, item)).ToList(); model.LossTypeOptions = new[] { ProductionLossRecord.TypeEvaporation, ProductionLossRecord.TypeScrap, ProductionLossRecord.TypeDefect, ProductionLossRecord.TypeScaleVariance, ProductionLossRecord.TypeOther }.Select(item => new SelectListItem(item, item)).ToList(); model.InspectionResultOptions = new[] { ProductionQualityInspection.ResultPass, ProductionQualityInspection.ResultRework, ProductionQualityInspection.ResultReject }.Select(item => new SelectListItem(item, item)).ToList();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Trace(int id, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            var branches = await _permissions.GetAccessibleBranchesAsync(User);
            var order = await _context.ProductionWorkOrders.AsNoTracking()
                .Include(item => item.Product)
                .Include(item => item.Workshop)
                .Include(item => item.ProductionBom).ThenInclude(item => item.Items)
                .Include(item => item.ProductionBom).ThenInclude(item => item.Operations)
                .Include(item => item.MaterialReservations).ThenInclude(item => item.RawMaterialLot)
                .Include(item => item.OperationLogs).ThenInclude(item => item.WorkerUser)
                .Include(item => item.LossRecords).ThenInclude(item => item.ProductionLossPolicy)
                .Include(item => item.QualityInspections).ThenInclude(item => item.InspectedByUser)
                .Include(item => item.Receipts).ThenInclude(item => item.Warehouse)
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
            if (order == null || (!branches.Any(item => item.Id == order.BranchId) && !User.IsInRole(RoleCatalog.Admin))) return NotFound();
            var histories = await _context.ProductionStatusHistories.AsNoTracking()
                .Include(item => item.ChangedByUser)
                .Where(item => item.ProductionWorkOrderId == order.Id)
                .OrderBy(item => item.ChangedAt).ToListAsync(cancellationToken);
            var custody = order.CustomerJobOrderId.HasValue
                ? await _context.CustomerMaterialCustodyRecords.AsNoTracking().FirstOrDefaultAsync(item => item.CustomerJobOrderId == order.CustomerJobOrderId.Value, cancellationToken)
                : null;
            return View(new ProductionTraceViewModel
            {
                WorkOrder = order,
                CustomerCustody = custody,
                StatusHistories = histories,
                Reservations = order.MaterialReservations?.OrderBy(item => item.Id).ToList() ?? new List<ProductionMaterialReservation>(),
                Operations = order.OperationLogs?.OrderBy(item => item.SequenceNumber).ThenBy(item => item.StartedAt).ToList() ?? new List<ProductionOperationLog>(),
                Losses = order.LossRecords?.OrderBy(item => item.ReportedAt).ToList() ?? new List<ProductionLossRecord>(),
                Inspections = order.QualityInspections?.OrderBy(item => item.InspectedAt).ToList() ?? new List<ProductionQualityInspection>(),
                Receipt = order.Receipts?.OrderByDescending(item => item.CreatedAt).FirstOrDefault()
            });
        }

        [HttpGet]
        public async Task<IActionResult> Reports(int? branchId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            var branches = await _permissions.GetAccessibleBranchesAsync(User);
            var selectedBranch = branches.FirstOrDefault(item => item.Id == branchId) ?? branches.FirstOrDefault(item => item.Id == user?.BranchId) ?? branches.FirstOrDefault();
            if (selectedBranch == null) return Forbid();
            var grants = await _permissions.GetGrantedKeysAsync(User, selectedBranch.Id);
            if (!User.IsInRole(RoleCatalog.Admin) && !grants.Contains(ManagementFeatureCatalog.ProductionView)) return Forbid();
            var start = (from ?? DateTime.Today.AddDays(-29)).Date.ToUniversalTime();
            var end = (to ?? DateTime.Today).Date.AddDays(1).ToUniversalTime();
            var orders = await _context.ProductionWorkOrders.AsNoTracking().Include(item => item.Product)
                .Where(item => item.BranchId == selectedBranch.Id && item.CreatedAt >= start && item.CreatedAt < end).ToListAsync(cancellationToken);
            var auditLogs = await _context.ProductionAuditLogs.AsNoTracking().Include(item => item.ActorUser)
                .Where(item => item.BranchId == selectedBranch.Id && item.CreatedAt >= start && item.CreatedAt < end)
                .OrderByDescending(item => item.CreatedAt).Take(200).ToListAsync(cancellationToken);
            var rows = orders.Select(item => new ProductionReportRowViewModel
            {
                WorkOrderId = item.Id,
                WorkOrderCode = item.WorkOrderCode,
                ProductName = item.Product?.Name ?? $"Sản phẩm #{item.ProductId}",
                Status = item.Status,
                IssuedWeight = item.IssuedMaterialWeight,
                OutputWeight = item.ActualOutputWeight,
                LossWeight = item.ActualLossWeight,
                WipWeight = item.Status is ProductionWorkOrder.StatusClosed or ProductionWorkOrder.StatusCancelled ? 0 : item.ActualOutputWeight > 0 ? item.ActualOutputWeight : item.IssuedMaterialWeight,
                TotalCost = item.TotalCost
            }).OrderBy(item => item.WorkOrderCode).ToList();
            return View(new ProductionReportViewModel
            {
                BranchId = selectedBranch.Id, From = start.ToLocalTime(), To = end.AddDays(-1).ToLocalTime(), WorkOrderCount = orders.Count,
                ClosedWorkOrderCount = orders.Count(item => item.Status == ProductionWorkOrder.StatusClosed), IssuedWeight = orders.Sum(item => item.IssuedMaterialWeight),
                OutputWeight = orders.Sum(item => item.ActualOutputWeight), LossWeight = orders.Sum(item => item.ActualLossWeight), WipWeight = rows.Sum(item => item.WipWeight),
                MaterialCost = orders.Sum(item => item.MaterialCost), LaborCost = orders.Sum(item => item.LaborCost), OverheadCost = orders.Sum(item => item.OverheadCost), TotalCost = orders.Sum(item => item.TotalCost), Rows = rows, AuditLogs = auditLogs
            });
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(int? branchId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
        {
            var report = await BuildExportRows(branchId, from, to, cancellationToken);
            var html = new StringBuilder("<html><head><meta charset='utf-8'></head><body><table border='1'><tr><th colspan='8'>Báo cáo sản xuất</th></tr><tr><th>Lệnh</th><th>Sản phẩm</th><th>Trạng thái</th><th>Đã xuất (g)</th><th>Đầu ra (g)</th><th>Hao hụt (g)</th><th>WIP (g)</th><th>Giá vốn</th></tr>");
            foreach (var row in report.Rows) html.Append("<tr>").Append($"<td>{Html(row.WorkOrderCode)}</td><td>{Html(row.ProductName)}</td><td>{Html(row.Status)}</td><td>{row.IssuedWeight:N4}</td><td>{row.OutputWeight:N4}</td><td>{row.LossWeight:N4}</td><td>{row.WipWeight:N4}</td><td>{row.TotalCost:N0}</td></tr>");
            html.Append("</table></body></html>"); return File(Encoding.UTF8.GetBytes(html.ToString()), "application/vnd.ms-excel", $"production-{DateTime.UtcNow:yyyyMMddHHmmss}.xls");
        }

        [HttpGet]
        public async Task<IActionResult> ExportPdf(int? branchId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
        {
            var report = await BuildExportRows(branchId, from, to, cancellationToken);
            var lines = new List<string> { "PRODUCTION REPORT", $"Period: {report.From:yyyy-MM-dd} - {report.To:yyyy-MM-dd}", $"Orders: {report.WorkOrderCount} | Closed: {report.ClosedWorkOrderCount}", $"Issued: {report.IssuedWeight:N4} g | Output: {report.OutputWeight:N4} g | Loss: {report.LossWeight:N4} g", $"WIP: {report.WipWeight:N4} g | Total cost: {report.TotalCost:N0}" };
            lines.AddRange(report.Rows.Select(row => $"{row.WorkOrderCode} | {row.Status} | issued {row.IssuedWeight:N4} | output {row.OutputWeight:N4} | loss {row.LossWeight:N4} | cost {row.TotalCost:N0}"));
            return File(BuildSimplePdf(lines), "application/pdf", $"production-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
        }

        private async Task<ProductionReportViewModel> BuildExportRows(int? branchId, DateTime? from, DateTime? to, CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User); var branches = await _permissions.GetAccessibleBranchesAsync(User);
            var branch = branches.FirstOrDefault(item => item.Id == branchId) ?? branches.FirstOrDefault(item => item.Id == user?.BranchId) ?? branches.FirstOrDefault();
            if (branch == null) throw new ProductionBusinessException("Không tìm thấy chi nhánh được phép truy cập.");
            var grants = await _permissions.GetGrantedKeysAsync(User, branch.Id);
            if (!User.IsInRole(RoleCatalog.Admin) && !grants.Contains(ManagementFeatureCatalog.ProductionView)) throw new ProductionBusinessException("Bạn không có quyền xem báo cáo sản xuất.");
            var start = (from ?? DateTime.Today.AddDays(-29)).Date.ToUniversalTime(); var end = (to ?? DateTime.Today).Date.AddDays(1).ToUniversalTime();
            var orders = await _context.ProductionWorkOrders.AsNoTracking().Include(item => item.Product).Where(item => item.BranchId == branch.Id && item.CreatedAt >= start && item.CreatedAt < end).ToListAsync(cancellationToken);
            var rows = orders.Select(item => new ProductionReportRowViewModel { WorkOrderId = item.Id, WorkOrderCode = item.WorkOrderCode, ProductName = item.Product?.Name ?? $"Sản phẩm #{item.ProductId}", Status = item.Status, IssuedWeight = item.IssuedMaterialWeight, OutputWeight = item.ActualOutputWeight, LossWeight = item.ActualLossWeight, WipWeight = ProductionMetrics.WipWeight(item), TotalCost = item.TotalCost }).ToList();
            return new ProductionReportViewModel { BranchId = branch.Id, From = start.ToLocalTime(), To = end.AddDays(-1).ToLocalTime(), WorkOrderCount = orders.Count, ClosedWorkOrderCount = orders.Count(item => item.Status == ProductionWorkOrder.StatusClosed), IssuedWeight = orders.Sum(item => item.IssuedMaterialWeight), OutputWeight = orders.Sum(item => item.ActualOutputWeight), LossWeight = orders.Sum(item => item.ActualLossWeight), WipWeight = rows.Sum(item => item.WipWeight), MaterialCost = orders.Sum(item => item.MaterialCost), LaborCost = orders.Sum(item => item.LaborCost), OverheadCost = orders.Sum(item => item.OverheadCost), TotalCost = orders.Sum(item => item.TotalCost), Rows = rows };
        }

        private static byte[] BuildSimplePdf(IReadOnlyList<string> lines)
        {
            var content = new StringBuilder("BT /F1 9 Tf 36 806 Td");
            foreach (var line in lines.Take(45)) content.Append(" (").Append(PdfEscape(line)).Append(") Tj 0 -16 Td");
            content.Append(" ET"); var objects = new[] { "<< /Type /Catalog /Pages 2 0 R >>", "<< /Type /Pages /Kids [3 0 R] /Count 1 >>", "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>", "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>", $"<< /Length {Encoding.ASCII.GetByteCount(content.ToString())} >>\nstream\n{content}\nendstream" };
            using var stream = new MemoryStream(); using var writer = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true); writer.Write("%PDF-1.4\n"); writer.Flush(); var offsets = new List<long> { 0 };
            for (var index = 0; index < objects.Length; index++) { offsets.Add(stream.Position); writer.Write($"{index + 1} 0 obj\n{objects[index]}\nendobj\n"); writer.Flush(); }
            var xref = stream.Position; writer.Write($"xref\n0 {objects.Length + 1}\n0000000000 65535 f \n"); for (var index = 1; index < offsets.Count; index++) writer.Write($"{offsets[index]:D10} 00000 n \n"); writer.Write($"trailer\n<< /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF"); writer.Flush(); return stream.ToArray();
        }

        private static string PdfEscape(string value) => value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)").Replace("\r", string.Empty).Replace("\n", " ");
        private static string Html(string value) => System.Net.WebUtility.HtmlEncode(value ?? string.Empty);

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadEvidence(IFormFile file, string entityType, int entityId, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0) return RedirectToAction(nameof(Trace), new { id = entityId });
            try
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                if (!allowed.Contains(extension) || file.Length > 5 * 1024 * 1024) throw new InvalidOperationException("Ảnh phải là JPG, PNG hoặc WEBP và không vượt quá 5 MB.");
                var folder = Path.Combine(_environment.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot"), "uploads", "production");
                Directory.CreateDirectory(folder);
                var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
                await using (var stream = System.IO.File.Create(Path.Combine(folder, fileName))) await file.CopyToAsync(stream, cancellationToken);
                await _production.AttachEvidenceAsync(new AttachProductionEvidenceInput { EntityType = entityType, EntityId = entityId, EvidenceUrl = $"/uploads/production/{fileName}" }, CurrentUserId(), cancellationToken);
                TempData["SuccessMessage"] = "Đã tải và gắn ảnh bằng chứng.";
                if (string.Equals(entityType, "QualityInspection", StringComparison.OrdinalIgnoreCase))
                {
                    var inspection = await _context.ProductionQualityInspections.AsNoTracking().FirstOrDefaultAsync(item => item.Id == entityId, cancellationToken);
                    if (inspection?.ProductionWorkOrderId is int workOrderId) entityId = workOrderId;
                }
                else if (string.Equals(entityType, "CustomerIntake", StringComparison.OrdinalIgnoreCase) || string.Equals(entityType, "CustomerHandover", StringComparison.OrdinalIgnoreCase))
                {
                    var workOrderId = await _context.ProductionWorkOrders.AsNoTracking().Where(item => item.CustomerJobOrderId == entityId).Select(item => (int?)item.Id).FirstOrDefaultAsync(cancellationToken);
                    if (workOrderId.HasValue) entityId = workOrderId.Value;
                }
            }
            catch (Exception exception) when (exception is InvalidOperationException or ProductionBusinessException)
            {
                TempData["ErrorMessage"] = exception.Message;
            }
            return RedirectToAction(nameof(Trace), new { id = entityId });
        }

        [HttpPost, ValidateAntiForgeryToken] public Task<IActionResult> CreateWorkOrder(CreateProductionWorkOrderInput input, CancellationToken cancellationToken) => Execute(() => _production.CreateWorkOrderAsync(input, CurrentUserId(), cancellationToken), input.BranchId, cancellationToken);
        [HttpPost, ValidateAntiForgeryToken] public Task<IActionResult> CreateWorkshop(CreateProductionWorkshopInput input, CancellationToken cancellationToken) => Execute(() => _production.CreateWorkshopAsync(input, CurrentUserId(), cancellationToken), input.BranchId, cancellationToken);
        [HttpPost, ValidateAntiForgeryToken] public Task<IActionResult> UpdateWorkshop(int id, CreateProductionWorkshopInput input, CancellationToken cancellationToken) => Execute(() => _production.UpdateWorkshopAsync(id, input, CurrentUserId(), cancellationToken), input.BranchId, cancellationToken);
        [HttpPost, ValidateAntiForgeryToken] public Task<IActionResult> SetWorkshopActive(int id, bool isActive, CancellationToken cancellationToken) => Execute(() => _production.SetWorkshopActiveAsync(id, isActive, CurrentUserId(), cancellationToken), null, cancellationToken);
        [HttpPost, ValidateAntiForgeryToken] public Task<IActionResult> CreateLossPolicy(CreateProductionLossPolicyInput input, CancellationToken cancellationToken) => Execute(() => _production.CreateLossPolicyAsync(input, CurrentUserId(), cancellationToken), input.BranchId, cancellationToken);
        [HttpPost, ValidateAntiForgeryToken] public Task<IActionResult> UpdateLossPolicy(int id, CreateProductionLossPolicyInput input, CancellationToken cancellationToken) => Execute(() => _production.UpdateLossPolicyAsync(id, input, CurrentUserId(), cancellationToken), input.BranchId, cancellationToken);
        [HttpPost, ValidateAntiForgeryToken] public Task<IActionResult> ActivateLossPolicy(int id, CancellationToken cancellationToken) => Execute(() => _production.ActivateLossPolicyAsync(id, CurrentUserId(), cancellationToken), null, cancellationToken);
        [HttpPost, ValidateAntiForgeryToken] public Task<IActionResult> CreateRawMaterialLot(CreateRawMaterialLotInput input, CancellationToken cancellationToken)
        {
            if (input.FineWeight <= 0) { input.FineWeight = input.GrossWeight * input.PurityRate; ModelState.Remove(nameof(input.FineWeight)); }
            return Execute(() => _production.CreateRawMaterialLotAsync(input, CurrentUserId(), cancellationToken), input.BranchId, cancellationToken);
        }
        [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> ReleaseRawMaterialLot(int id, ReleaseRawMaterialLotInput input, CancellationToken cancellationToken)
        {
            var lot = await _context.RawMaterialLots.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
            if (input.MeasuredGrossWeight <= 0 && lot != null) { input.MeasuredGrossWeight = lot.GrossWeight; ModelState.Remove(nameof(input.MeasuredGrossWeight)); }
            if (input.MeasuredFineWeight <= 0) { input.MeasuredFineWeight = input.MeasuredGrossWeight * input.MeasuredPurityRate; ModelState.Remove(nameof(input.MeasuredFineWeight)); }
            return await Execute(() => _production.ReleaseRawMaterialLotAsync(id, input, CurrentUserId(), cancellationToken), null, cancellationToken);
        }
        [HttpPost, ValidateAntiForgeryToken] public Task<IActionResult> CreateBom(CreateProductionBomInput input, CancellationToken cancellationToken) => Execute(() => _production.CreateBomAsync(input, CurrentUserId(), cancellationToken), input.BranchId, cancellationToken);
        [HttpPost, ValidateAntiForgeryToken] public Task<IActionResult> ActivateBom(int id, ActivateProductionBomInput input, CancellationToken cancellationToken) => Execute(() => _production.ActivateBomAsync(id, input, CurrentUserId(), cancellationToken), null, cancellationToken);
        [HttpPost, ValidateAntiForgeryToken] public Task<IActionResult> ReserveMaterial(ReserveProductionMaterialInput input, CancellationToken cancellationToken) => Execute(() => _production.ReserveMaterialAsync(input, CurrentUserId(), cancellationToken), null, cancellationToken);
        [HttpPost, ValidateAntiForgeryToken] public Task<IActionResult> IssueMaterial(IssueProductionMaterialInput input, CancellationToken cancellationToken) => Execute(() => _production.IssueMaterialAsync(input, CurrentUserId(), cancellationToken), null, cancellationToken);
        [HttpPost, ValidateAntiForgeryToken] public Task<IActionResult> ReturnMaterial(ReturnProductionMaterialInput input, CancellationToken cancellationToken) => Execute(() => _production.ReturnMaterialAsync(input, CurrentUserId(), cancellationToken), null, cancellationToken);
        [HttpPost, ValidateAntiForgeryToken] public Task<IActionResult> ReleaseReservedMaterial(ReleaseReservedMaterialInput input, CancellationToken cancellationToken) => Execute(() => _production.ReleaseReservedMaterialAsync(input, CurrentUserId(), cancellationToken), null, cancellationToken);
        [HttpPost, ValidateAntiForgeryToken] public Task<IActionResult> RecordOperation(RecordProductionOperationInput input, CancellationToken cancellationToken) => Execute(() => _production.RecordOperationAsync(input, CurrentUserId(), cancellationToken), null, cancellationToken);
        [HttpPost, ValidateAntiForgeryToken] public Task<IActionResult> RecordLoss(RecordProductionLossInput input, CancellationToken cancellationToken) => Execute(() => _production.RecordLossAsync(input, CurrentUserId(), cancellationToken), null, cancellationToken);
        [HttpPost, ValidateAntiForgeryToken] public Task<IActionResult> ReviewLoss(int id, ReviewProductionLossInput input, CancellationToken cancellationToken) => Execute(() => _production.ReviewLossAsync(id, input, CurrentUserId(), cancellationToken), null, cancellationToken);
        [HttpPost, ValidateAntiForgeryToken] public Task<IActionResult> RecordQualityInspection(RecordProductionQualityInspectionInput input, CancellationToken cancellationToken) => Execute(() => _production.RecordQualityInspectionAsync(input, CurrentUserId(), cancellationToken), null, cancellationToken);
        [HttpPost, ValidateAntiForgeryToken] public Task<IActionResult> ReleaseWorkOrder(ReleaseProductionWorkOrderInput input, CancellationToken cancellationToken) => Execute(() => _production.ReleaseWorkOrderAsync(input.ProductionWorkOrderId, input, CurrentUserId(), cancellationToken), null, cancellationToken);
        [HttpPost, ValidateAntiForgeryToken] public Task<IActionResult> ChangeWorkOrderStatus(ChangeProductionWorkOrderStatusInput input, CancellationToken cancellationToken) => Execute(() => _production.ChangeWorkOrderStatusAsync(input.ProductionWorkOrderId, input, CurrentUserId(), cancellationToken), null, cancellationToken);
        [HttpPost, ValidateAntiForgeryToken] public Task<IActionResult> CreateCustomerJob(CreateCustomerJobOrderInput input, CancellationToken cancellationToken) => Execute(() => _production.CreateCustomerJobOrderAsync(input, CurrentUserId(), cancellationToken), input.BranchId, cancellationToken);
        [HttpPost, ValidateAntiForgeryToken] public Task<IActionResult> RecordCustomerMaterialIssue(RecordCustomerMaterialIssueInput input, CancellationToken cancellationToken) => Execute(() => _production.RecordCustomerMaterialIssueAsync(input, CurrentUserId(), cancellationToken), null, cancellationToken);
        [HttpPost, ValidateAntiForgeryToken] public Task<IActionResult> RecordCustomerJobQuality(RecordCustomerJobQualityInput input, CancellationToken cancellationToken) => Execute(() => _production.RecordCustomerJobQualityAsync(input.CustomerJobOrderId, input, CurrentUserId(), cancellationToken), null, cancellationToken);
        [HttpPost, ValidateAntiForgeryToken] public Task<IActionResult> CompleteCustomerJobHandover(CompleteCustomerJobHandoverInput input, CancellationToken cancellationToken) => Execute(() => _production.CompleteCustomerJobHandoverAsync(input.CustomerJobOrderId, input, CurrentUserId(), cancellationToken), null, cancellationToken);
        [HttpPost, ValidateAntiForgeryToken] public Task<IActionResult> CreateRecycleBatch(CreateProductionRecycleBatchInput input, CancellationToken cancellationToken) => Execute(() => _production.CreateRecycleBatchAsync(input, CurrentUserId(), cancellationToken), input.BranchId, cancellationToken);
        [HttpPost, ValidateAntiForgeryToken] public Task<IActionResult> CompleteRecycleBatch(int id, CompleteProductionRecycleBatchInput input, CancellationToken cancellationToken) => Execute(() => _production.CompleteRecycleBatchAsync(id, input, CurrentUserId(), cancellationToken), null, cancellationToken);

        private string CurrentUserId() => _userManager.GetUserId(User);
        private async Task<IActionResult> Execute<T>(Func<Task<T>> operation, int? branchId, CancellationToken cancellationToken)
        {
            try { ModelState.Remove("RowVersion"); if (!ModelState.IsValid) { TempData["ErrorMessage"] = string.Join(" ", ModelState.Values.SelectMany(item => item.Errors).Select(item => item.ErrorMessage)); } else { await operation(); TempData["SuccessMessage"] = "Đã ghi nhận nghiệp vụ sản xuất."; } }
            catch (ProductionBusinessException exception) { TempData["ErrorMessage"] = exception.Message; }
            return RedirectToAction(nameof(Index), new { branchId });
        }
    }
}
