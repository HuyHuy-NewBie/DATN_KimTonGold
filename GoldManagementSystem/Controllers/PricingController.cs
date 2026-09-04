using GoldManagementSystem.Data;
using GoldManagementSystem.Models;
using GoldManagementSystem.Models.ViewModels;
using GoldManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GoldManagementSystem.Controllers
{
    [Authorize(Roles = RoleCatalog.ManagementRoles)]
    public sealed class PricingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IManagementPermissionService _permissions;

        public PricingController(ApplicationDbContext context, UserManager<AppUser> userManager, IManagementPermissionService permissions)
        {
            _context = context;
            _userManager = userManager;
            _permissions = permissions;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (!User.IsInRole(RoleCatalog.Admin)
                && !await _permissions.CanAsync(User, ManagementFeatureCatalog.PriceManage, currentUser?.BranchId)
                && !await _permissions.CanAsync(User, ManagementFeatureCatalog.PriceApprove, currentUser?.BranchId)) return Forbid();
            var query = _context.PriceBooks
                .AsNoTracking()
                .Include(book => book.Branch)
                .Include(book => book.Versions)
                .AsQueryable();
            if (!User.IsInRole(RoleCatalog.Admin)) query = query.Where(book => book.BranchId == null || book.BranchId == currentUser.BranchId);
            var books = await query.OrderByDescending(book => book.CreatedAt).ToListAsync();
            return View(books);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!await _permissions.CanAsync(User, ManagementFeatureCatalog.PriceManage)) return Forbid();
            var products = await _context.Products.AsNoTracking().Where(product => product.Status != "Đã bán").OrderBy(product => product.Name).ToListAsync();
            var model = new PriceBookFormViewModel
            {
                Code = $"PB-{DateTime.UtcNow:yyyyMMddHHmmss}",
                Name = "Bảng giá mới",
                Scope = PriceBookScopeOptions.General,
                Lines = products.Select(product => new PriceLineFormViewModel
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    SellUnitPrice = product.SellPrice,
                    BuyUnitPrice = product.BuyPrice,
                    ProcessingFee = product.ProcessingFee
                }).ToList()
            };
            await PopulateBranchesAsync(model.BranchId);
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PriceBookFormViewModel model)
        {
            if (!await _permissions.CanAsync(User, ManagementFeatureCatalog.PriceManage, model.BranchId)) return Forbid();
            Normalize(model);
            var productIds = model.Lines.Select(line => line.ProductId).Distinct().ToList();
            var products = await _context.Products.Where(product => productIds.Contains(product.Id)).ToDictionaryAsync(product => product.Id);
            if (model.Lines.Count == 0) ModelState.AddModelError(nameof(model.Lines), "Bảng giá phải có ít nhất một sản phẩm.");
            if (products.Count != productIds.Count) ModelState.AddModelError(nameof(model.Lines), "Có sản phẩm không tồn tại.");
            if (model.EffectiveTo.HasValue && model.EffectiveTo <= model.EffectiveFrom) ModelState.AddModelError(nameof(model.EffectiveTo), "Thời điểm kết thúc phải sau thời điểm bắt đầu.");
            if (model.Lines.Any(line => line.SellUnitPrice < 0 || line.BuyUnitPrice < 0 || line.ProcessingFee < 0)) ModelState.AddModelError(nameof(model.Lines), "Giá và tiền công không được âm.");
            if (!ModelState.IsValid)
            {
                await PopulateLineNamesAsync(model, products);
                await PopulateBranchesAsync(model.BranchId);
                return View(model);
            }

            var userId = _userManager.GetUserId(User);
            var book = new PriceBook
            {
                Code = model.Code,
                Name = model.Name,
                Scope = model.Scope,
                BranchId = model.BranchId,
                Status = PriceBookStatusOptions.Draft,
                EffectiveFrom = model.EffectiveFrom,
                EffectiveTo = model.EffectiveTo,
                Notes = model.Notes,
                CreatedByUserId = userId
            };
            var version = new PriceVersion
            {
                Version = model.Version,
                EffectiveFrom = model.EffectiveFrom,
                EffectiveTo = model.EffectiveTo,
                ChangeReason = model.ChangeReason,
                CreatedByUserId = userId,
                Lines = model.Lines.Select(line => new PriceLine
                {
                    ProductId = line.ProductId,
                    SellUnitPrice = line.SellUnitPrice,
                    BuyUnitPrice = line.BuyUnitPrice,
                    ProcessingFee = line.ProcessingFee,
                    MaxDiscountRate = line.MaxDiscountRate
                }).ToList()
            };
            book.Versions.Add(version);
            _context.PriceBooks.Add(book);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã tạo bảng giá ở trạng thái nháp.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id)
        {
            var book = await _context.PriceBooks.FirstOrDefaultAsync(item => item.Id == id);
            if (book == null) return NotFound();
            if (!await _permissions.CanAsync(User, ManagementFeatureCatalog.PriceManage, book.BranchId)) return Forbid();
            if (book.Status != PriceBookStatusOptions.Draft) return BadRequest("Chỉ bảng giá nháp mới được gửi duyệt.");
            book.Status = PriceBookStatusOptions.PendingApproval;
            book.SubmittedByUserId = _userManager.GetUserId(User);
            book.SubmittedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var book = await _context.PriceBooks.FirstOrDefaultAsync(item => item.Id == id);
            if (book == null) return NotFound();
            if (!await _permissions.CanAsync(User, ManagementFeatureCatalog.PriceApprove, book.BranchId)) return Forbid();
            if (book.Status != PriceBookStatusOptions.PendingApproval) return BadRequest("Bảng giá chưa ở trạng thái chờ duyệt.");
            var userId = _userManager.GetUserId(User);
            if (string.Equals(book.CreatedByUserId, userId, StringComparison.OrdinalIgnoreCase)) return BadRequest("Người tạo không được tự duyệt bảng giá.");
            book.Status = PriceBookStatusOptions.Published;
            book.ApprovedByUserId = userId;
            book.ApprovedAt = DateTime.UtcNow;
            book.PublishedAt = DateTime.UtcNow;
            await using var transaction = await _context.Database.BeginTransactionAsync();
            await _context.PriceBooks.Where(item => item.Id != id && item.Status == PriceBookStatusOptions.Published && item.BranchId == book.BranchId && item.EffectiveTo == null).ExecuteUpdateAsync(update => update.SetProperty(item => item.Status, PriceBookStatusOptions.Expired).SetProperty(item => item.EffectiveTo, book.EffectiveFrom));
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["SuccessMessage"] = "Đã duyệt và công bố bảng giá.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Expire(int id)
        {
            var book = await _context.PriceBooks.FirstOrDefaultAsync(item => item.Id == id);
            if (book == null) return NotFound();
            if (!await _permissions.CanAsync(User, ManagementFeatureCatalog.PriceApprove, book.BranchId)) return Forbid();
            if (book.Status != PriceBookStatusOptions.Published) return BadRequest("Chỉ bảng giá đã công bố mới được hết hạn.");
            book.Status = PriceBookStatusOptions.Expired;
            book.EffectiveTo = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateBranchesAsync(int? selected)
        {
            ViewBag.Branches = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _permissions.GetAccessibleBranchesAsync(User), "Id", "BranchName", selected);
        }

        private async Task PopulateLineNamesAsync(PriceBookFormViewModel model, IReadOnlyDictionary<int, Product> products)
        {
            foreach (var line in model.Lines) if (products.TryGetValue(line.ProductId, out var product)) line.ProductName = product.Name;
            await PopulateBranchesAsync(model.BranchId);
        }

        private static void Normalize(PriceBookFormViewModel model)
        {
            model.Code = model.Code?.Trim() ?? string.Empty;
            model.Name = model.Name?.Trim() ?? string.Empty;
            model.Version = model.Version?.Trim() ?? string.Empty;
            model.Notes = model.Notes?.Trim();
            model.ChangeReason = model.ChangeReason?.Trim();
            model.Lines = (model.Lines ?? new List<PriceLineFormViewModel>()).Where(line => line.ProductId > 0).GroupBy(line => line.ProductId).Select(group => group.First()).ToList();
        }
    }
}
