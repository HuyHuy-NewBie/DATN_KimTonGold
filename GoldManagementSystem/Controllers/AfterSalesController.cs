using GoldManagementSystem.Data;
using GoldManagementSystem.Models;
using GoldManagementSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GoldManagementSystem.Controllers;

public sealed class AfterSalesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IManagementPermissionService _permissions;
    private readonly UserManager<AppUser> _userManager;

    public AfterSalesController(ApplicationDbContext context, IManagementPermissionService permissions, UserManager<AppUser> userManager)
    {
        _context = context;
        _permissions = permissions;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? branchId)
    {
        var user = await _userManager.GetUserAsync(User);
        var selectedBranch = branchId ?? user?.BranchId;
        if (!await _permissions.CanAsync(User, ManagementFeatureCatalog.AfterSalesManage, selectedBranch)) return Forbid();
        var buybacks = await _context.BuybackCases.AsNoTracking().Where(item => !selectedBranch.HasValue || item.BranchId == selectedBranch).OrderByDescending(item => item.CreatedAt).Take(100).ToListAsync();
        var returns = await _context.ReturnCases.AsNoTracking().Where(item => !selectedBranch.HasValue || item.BranchId == selectedBranch).OrderByDescending(item => item.RequestedAt).Take(100).ToListAsync();
        var warranties = await _context.WarrantyCases.AsNoTracking().Where(item => !selectedBranch.HasValue || item.BranchId == selectedBranch).OrderByDescending(item => item.ReceivedAt).Take(100).ToListAsync();
        var repairs = await _context.RepairCases.AsNoTracking().Where(item => !selectedBranch.HasValue || item.BranchId == selectedBranch).OrderByDescending(item => item.Id).Take(100).ToListAsync();
        return Ok(new { buybacks, returns, warranties, repairs });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBuyback(CreateBuybackInput input)
    {
        if (!await CanManageAsync(input.BranchId) || !ModelState.IsValid || input.GrossWeight <= 0 || input.PurityRate <= 0 || input.ProposedAmount < 0) return BadRequest("Thông tin thu mua không hợp lệ.");
        var item = new BuybackCase { BranchId = input.BranchId, ProductId = input.ProductId, OrderDetailId = input.OrderDetailId, CustomerName = input.CustomerName.Trim(), CustomerPhone = input.CustomerPhone.Trim(), IdentityNumber = input.IdentityNumber?.Trim(), GrossWeight = input.GrossWeight, FineWeight = input.GrossWeight * input.PurityRate, PurityRate = input.PurityRate, ProposedAmount = input.ProposedAmount, CreatedByUserId = CurrentUserId() };
        _context.BuybackCases.Add(item);
        await _context.SaveChangesAsync();
        return Ok(new { item.Id, item.CaseNumber, item.Status });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordAssay(RecordAssayInput input)
    {
        var item = await _context.BuybackCases.FirstOrDefaultAsync(x => x.Id == input.BuybackCaseId);
        if (item == null) return NotFound();
        if (!await CanManageAsync(item.BranchId) || item.Status is AfterSalesStatus.Paid or AfterSalesStatus.Rejected) return BadRequest("Hồ sơ không còn ở trạng thái được kiểm định.");
        if (input.GrossWeight <= 0 || input.PurityRate <= 0 || input.PurityRate > 1 || input.FineWeight < 0 || input.FineWeight > input.GrossWeight || input.Result is not (AssayResultOptions.Passed or AssayResultOptions.Failed)) return BadRequest("Kết quả assay không hợp lệ.");
        item.GrossWeight = input.GrossWeight; item.FineWeight = input.FineWeight; item.PurityRate = input.PurityRate; item.AssayStatus = input.Result; item.Status = input.Result == AssayResultOptions.Passed ? AfterSalesStatus.Quarantine : AfterSalesStatus.Rejected;
        _context.BuybackAssays.Add(new BuybackAssay { BuybackCaseId = item.Id, MeasuredGrossWeight = input.GrossWeight, MeasuredFineWeight = input.FineWeight, MeasuredPurityRate = input.PurityRate, Result = input.Result, AssayedByUserId = CurrentUserId(), Note = input.Note?.Trim() });
        await _context.SaveChangesAsync(); return Ok(new { item.Id, item.Status, item.AssayStatus });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitBuyback(int id)
    {
        var item = await _context.BuybackCases.FindAsync(id); if (item == null) return NotFound();
        if (!await CanManageAsync(item.BranchId) || item.AssayStatus != AssayResultOptions.Passed || item.Status != AfterSalesStatus.Quarantine) return BadRequest("Chỉ được trình duyệt hồ sơ đã assay đạt.");
        item.Status = AfterSalesStatus.PendingApproval; await _context.SaveChangesAsync(); return Ok(new { item.Id, item.Status });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveBuyback(ApproveBuybackInput input)
    {
        var item = await _context.BuybackCases.FindAsync(input.Id); if (item == null) return NotFound();
        if (!await CanApproveAsync(item.BranchId) || item.Status != AfterSalesStatus.PendingApproval) return BadRequest("Hồ sơ thu mua không chờ duyệt.");
        if (item.CreatedByUserId == CurrentUserId()) return BadRequest("Người tạo không được tự duyệt hồ sơ.");
        if (input.Amount < 0) return BadRequest("Giá duyệt không hợp lệ.");
        item.ApprovedAmount = input.Amount; item.Status = input.Approve ? AfterSalesStatus.Approved : AfterSalesStatus.Rejected; item.ApprovedByUserId = CurrentUserId(); item.ApprovedAt = DateTime.UtcNow; await _context.SaveChangesAsync(); return Ok(new { item.Id, item.Status });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PayBuyback(int id)
    {
        var item = await _context.BuybackCases.FindAsync(id); if (item == null) return NotFound();
        if (!await CanApproveAsync(item.BranchId) || item.Status != AfterSalesStatus.Approved || item.ApprovedAmount <= 0) return BadRequest("Hồ sơ chưa đủ điều kiện chi trả.");
        item.Status = AfterSalesStatus.Paid; item.PaidAt = DateTime.UtcNow; await _context.SaveChangesAsync(); return Ok(new { item.Id, item.Status, item.PaidAt });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestReturn(RequestReturnInput input)
    {
        var detail = await _context.OrderDetails.Include(x => x.Order).FirstOrDefaultAsync(x => x.Id == input.OrderDetailId);
        if (detail == null || !await CanManageAsync(detail.Order.BranchId) || string.IsNullOrWhiteSpace(input.Reason)) return BadRequest("Thông tin đổi trả không hợp lệ.");
        if (await _context.ReturnCases.AnyAsync(x => x.OrderDetailId == input.OrderDetailId && x.Status != AfterSalesStatus.Rejected && x.Status != AfterSalesStatus.Cancelled)) return BadRequest("Dòng hàng đã có hồ sơ đổi trả đang xử lý.");
        var item = new ReturnCase { BranchId = detail.Order.BranchId, OrderId = detail.OrderId, OrderDetailId = detail.Id, Reason = input.Reason.Trim(), RequestedByUserId = CurrentUserId() };
        _context.ReturnCases.Add(item); await _context.SaveChangesAsync(); return Ok(new { item.Id, item.CaseNumber, item.Status });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveReturn(int id, bool approve = true)
    {
        var item = await _context.ReturnCases.FindAsync(id); if (item == null) return NotFound();
        if (!await CanApproveAsync(item.BranchId) || item.Status != AfterSalesStatus.Requested) return BadRequest("Hồ sơ đổi trả không chờ duyệt.");
        if (item.RequestedByUserId == CurrentUserId()) return BadRequest("Người tạo không được tự duyệt hồ sơ.");
        item.Status = approve ? AfterSalesStatus.Approved : AfterSalesStatus.Rejected; item.ApprovedByUserId = CurrentUserId(); item.ApprovedAt = DateTime.UtcNow; await _context.SaveChangesAsync(); return Ok(new { item.Id, item.Status });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestRefund(RequestRefundInput input)
    {
        var item = await _context.ReturnCases.Include(x => x.Refund).Include(x => x.OrderDetail).ThenInclude(x => x.Order).FirstOrDefaultAsync(x => x.Id == input.ReturnCaseId);
        if (item == null) return NotFound();
        if (!await CanManageAsync(item.BranchId) || item.Status != AfterSalesStatus.Approved || item.Refund != null || input.Amount <= 0) return BadRequest("Hồ sơ chưa đủ điều kiện hoàn tiền.");
        var lineAmount = item.OrderDetail.UnitPrice * item.OrderDetail.Quantity + item.OrderDetail.ProcessingFee - item.OrderDetail.DiscountAmount;
        if (input.Amount > lineAmount || input.Channel is not (PaymentChannelOptions.Cash or PaymentChannelOptions.BankTransfer or PaymentChannelOptions.QR)) return BadRequest("Số tiền hoặc kênh hoàn tiền không hợp lệ.");
        item.Status = AfterSalesStatus.Received;
        _context.Refunds.Add(new Refund { ReturnCaseId = item.Id, Amount = input.Amount, Channel = input.Channel, RequestedByUserId = CurrentUserId() });
        await _context.SaveChangesAsync(); return Ok(new { item.Id, RefundStatus = RefundStatusOptions.Pending });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessRefund(int id, bool succeed = true, string error = null)
    {
        var refund = await _context.Refunds.Include(x => x.ReturnCase).FirstOrDefaultAsync(x => x.Id == id); if (refund == null) return NotFound();
        if (!await CanApproveAsync(refund.ReturnCase.BranchId) || refund.Status is RefundStatusOptions.Completed) return BadRequest("Hoàn tiền không thể xử lý lại.");
        refund.RetryCount++; refund.Status = succeed ? RefundStatusOptions.Completed : RefundStatusOptions.Failed; refund.LastError = succeed ? null : (error?.Trim() ?? "Giao dịch hoàn tiền thất bại."); refund.ProcessedByUserId = CurrentUserId(); refund.ProcessedAt = DateTime.UtcNow; refund.TransactionReference = succeed ? $"REF-{refund.Id}-{refund.RetryCount}-{DateTime.UtcNow:yyyyMMddHHmmss}" : null; if (succeed) refund.ReturnCase.Status = AfterSalesStatus.Completed; await _context.SaveChangesAsync(); return Ok(new { refund.Id, refund.Status, refund.RetryCount });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateWarranty(CreateWarrantyInput input)
    {
        var detail = await _context.OrderDetails.Include(x => x.Order).FirstOrDefaultAsync(x => x.Id == input.OrderDetailId); if (detail == null || !await CanManageAsync(detail.Order.BranchId) || string.IsNullOrWhiteSpace(input.IssueDescription)) return BadRequest("Thông tin bảo hành không hợp lệ.");
        var item = new WarrantyCase { BranchId = detail.Order.BranchId, OrderDetailId = detail.Id, CustomerName = input.CustomerName.Trim(), CustomerPhone = input.CustomerPhone.Trim(), IssueDescription = input.IssueDescription.Trim(), DueAt = input.DueAt, CreatedByUserId = CurrentUserId() }; _context.WarrantyCases.Add(item); await _context.SaveChangesAsync(); return Ok(new { item.Id, item.CaseNumber, item.Status });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRepair(CreateRepairInput input)
    {
        var detail = await _context.OrderDetails.Include(x => x.Order).FirstOrDefaultAsync(x => x.Id == input.OrderDetailId); if (detail == null || !await CanManageAsync(detail.Order.BranchId)) return BadRequest("Thông tin sửa chữa không hợp lệ.");
        var item = new RepairCase { BranchId = detail.Order.BranchId, WarrantyCaseId = input.WarrantyCaseId, OrderDetailId = detail.Id, Diagnosis = input.Diagnosis?.Trim(), QuotedLaborCost = input.QuotedLaborCost, DueAt = input.DueAt, CreatedByUserId = CurrentUserId() }; _context.RepairCases.Add(item); await _context.SaveChangesAsync(); return Ok(new { item.Id, item.CaseNumber, item.Status });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveRepair(ApproveRepairInput input)
    {
        var item = await _context.RepairCases.FindAsync(input.Id); if (item == null) return NotFound();
        if (!await CanApproveAsync(item.BranchId) || item.Status != AfterSalesStatus.Received) return BadRequest("Hồ sơ sửa chữa không chờ duyệt.");
        if (item.CreatedByUserId == CurrentUserId()) return BadRequest("Người tạo không được tự duyệt báo giá sửa chữa.");
        if (input.Approve && input.Amount < 0) return BadRequest("Chi phí duyệt không hợp lệ.");
        item.ApprovedAmount = input.Amount; item.ApprovedByUserId = CurrentUserId(); item.ApprovedAt = DateTime.UtcNow; item.Status = input.Approve ? AfterSalesStatus.Repairing : AfterSalesStatus.Rejected; await _context.SaveChangesAsync(); return Ok(new { item.Id, item.Status });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteRepair(int id)
    {
        var item = await _context.RepairCases.Include(x => x.WarrantyCase).FirstOrDefaultAsync(x => x.Id == id); if (item == null) return NotFound();
        if (!await CanManageAsync(item.BranchId) || item.Status is AfterSalesStatus.Completed or AfterSalesStatus.Cancelled) return BadRequest("Hồ sơ sửa chữa không thể hoàn tất.");
        item.Status = AfterSalesStatus.Completed; item.CompletedAt = DateTime.UtcNow; if (item.WarrantyCase != null) { item.WarrantyCase.Status = AfterSalesStatus.Completed; item.WarrantyCase.CompletedAt = item.CompletedAt; } await _context.SaveChangesAsync(); return Ok(new { item.Id, item.Status });
    }

    private string CurrentUserId() => _userManager.GetUserId(User) ?? string.Empty;
    private async Task<bool> CanManageAsync(int branchId) => await _permissions.CanAsync(User, ManagementFeatureCatalog.AfterSalesManage, branchId);
    private async Task<bool> CanApproveAsync(int branchId) => await _permissions.CanAsync(User, ManagementFeatureCatalog.AfterSalesApprove, branchId);
}

public sealed class CreateBuybackInput { public int BranchId { get; set; } public int? ProductId { get; set; } public int? OrderDetailId { get; set; } public string CustomerName { get; set; } public string CustomerPhone { get; set; } public string IdentityNumber { get; set; } public decimal GrossWeight { get; set; } public decimal PurityRate { get; set; } public decimal ProposedAmount { get; set; } }
public sealed class RecordAssayInput { public int BuybackCaseId { get; set; } public decimal GrossWeight { get; set; } public decimal FineWeight { get; set; } public decimal PurityRate { get; set; } public string Result { get; set; } = AssayResultOptions.Passed; public string Note { get; set; } }
public sealed class ApproveBuybackInput { public int Id { get; set; } public decimal Amount { get; set; } public bool Approve { get; set; } = true; }
public sealed class RequestReturnInput { public int OrderDetailId { get; set; } public string Reason { get; set; } }
public sealed class RequestRefundInput { public int ReturnCaseId { get; set; } public decimal Amount { get; set; } public string Channel { get; set; } = PaymentChannelOptions.Cash; }
public sealed class CreateWarrantyInput { public int OrderDetailId { get; set; } public string CustomerName { get; set; } public string CustomerPhone { get; set; } public string IssueDescription { get; set; } public DateTime? DueAt { get; set; } }
public sealed class CreateRepairInput { public int OrderDetailId { get; set; } public int? WarrantyCaseId { get; set; } public string Diagnosis { get; set; } public decimal QuotedLaborCost { get; set; } public DateTime? DueAt { get; set; } }
public sealed class ApproveRepairInput { public int Id { get; set; } public decimal Amount { get; set; } public bool Approve { get; set; } = true; }
