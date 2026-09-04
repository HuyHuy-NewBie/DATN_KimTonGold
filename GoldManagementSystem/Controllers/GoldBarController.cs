using System.Globalization;
using System.Text;
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
    public sealed class GoldBarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IManagementPermissionService _permissions;

        public GoldBarController(ApplicationDbContext context, UserManager<AppUser> userManager, IManagementPermissionService permissions)
        {
            _context = context;
            _userManager = userManager;
            _permissions = permissions;
        }

        public async Task<IActionResult> Index()
        {
            if (!await CanAccessAsync()) return Forbid();
            var locations = await _context.BusinessLocations.Include(item => item.Branch).Include(item => item.Licenses).AsNoTracking().ToListAsync();
            var serials = await _context.GoldBarSerials.Include(item => item.Product).Include(item => item.BusinessLocation).AsNoTracking().OrderByDescending(item => item.ReceivedAt).Take(100).ToListAsync();
            ViewBag.Locations = locations;
            ViewBag.Branches = await _permissions.GetAccessibleBranchesAsync(User);
            ViewBag.GoldBarProducts = await _context.Products.AsNoTracking().Where(item => item.ProductLegalClass == ProductLegalClassOptions.GoldBarRegulated).OrderBy(item => item.Name).ToListAsync();
            return View(serials);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLocation(GoldBarLocationInput input)
        {
            if (!await _permissions.CanAsync(User, ManagementFeatureCatalog.GoldBarCompliance, input.BranchId)) return Forbid();
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _context.BusinessLocations.AnyAsync(item => item.Code == input.Code)) return Conflict("Mã địa điểm đã tồn tại.");
            _context.BusinessLocations.Add(new BusinessLocation { BranchId = input.BranchId, Code = input.Code.Trim(), Name = input.Name.Trim(), Address = input.Address.Trim() });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLicense(GoldBarLicenseInput input)
        {
            var location = await _context.BusinessLocations.FindAsync(input.BusinessLocationId);
            if (location == null) return NotFound();
            if (!await _permissions.CanAsync(User, ManagementFeatureCatalog.GoldBarCompliance, location.BranchId)) return Forbid();
            if (!ModelState.IsValid || input.ValidTo <= input.ValidFrom) return BadRequest("Thông tin giấy phép không hợp lệ.");
            _context.BusinessLicenses.Add(new BusinessLicense { BusinessLocationId = location.Id, Number = input.Number.Trim(), ValidFrom = input.ValidFrom, ValidTo = input.ValidTo, IsVerified = false });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyLicense(int id)
        {
            var license = await _context.BusinessLicenses.Include(item => item.BusinessLocation).FirstOrDefaultAsync(item => item.Id == id);
            if (license == null) return NotFound();
            if (!await _permissions.CanAsync(User, ManagementFeatureCatalog.GoldBarCompliance, license.BusinessLocation.BranchId)) return Forbid();
            if (license.ValidTo.HasValue && license.ValidTo <= license.ValidFrom) return BadRequest("Thời hạn giấy phép không hợp lệ.");
            license.IsVerified = true;
            license.VerifiedAt = DateTime.UtcNow;
            license.VerifiedByUserId = _userManager.GetUserId(User);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSerial(GoldBarSerialInput input)
        {
            var location = await _context.BusinessLocations.FindAsync(input.BusinessLocationId);
            var product = await _context.Products.FindAsync(input.ProductId);
            if (location == null || product == null) return NotFound();
            if (!await _permissions.CanAsync(User, ManagementFeatureCatalog.GoldBarCompliance, location.BranchId)) return Forbid();
            if (product.ProductLegalClass != ProductLegalClassOptions.GoldBarRegulated) return BadRequest("Sản phẩm không thuộc nhóm vàng miếng.");
            if (!ModelState.IsValid || input.FineWeight > input.GrossWeight) return BadRequest("Trọng lượng serial không hợp lệ.");
            if (!await HasValidLicenseAsync(location.Id)) return BadRequest("Địa điểm chưa có giấy phép vàng miếng còn hiệu lực.");
            if (await _context.GoldBarSerials.AnyAsync(item => item.SerialNumber == input.SerialNumber)) return Conflict("Serial đã tồn tại.");
            _context.GoldBarSerials.Add(new GoldBarSerial { ProductId = product.Id, BusinessLocationId = location.Id, SerialNumber = input.SerialNumber.Trim(), PurityCode = input.PurityCode.Trim(), GrossWeight = input.GrossWeight, FineWeight = input.FineWeight, CertificateNumber = input.CertificateNumber?.Trim(), RetainUntil = DateTime.UtcNow.AddYears(10) });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterSale(RegisterGoldBarSaleInput input)
        {
            var actorId = _userManager.GetUserId(User);
            var detail = await _context.OrderDetails.Include(item => item.Order).Include(item => item.Product).Include(item => item.PriceSnapshot).FirstOrDefaultAsync(item => item.Id == input.OrderDetailId);
            var serial = await _context.GoldBarSerials.Include(item => item.BusinessLocation).FirstOrDefaultAsync(item => item.Id == input.GoldBarSerialId);
            var kyc = await _context.CustomerKycProfiles.FirstOrDefaultAsync(item => item.Id == input.CustomerKycProfileId);
            if (detail == null || serial == null || kyc == null) return NotFound();
            if (!await _permissions.CanAsync(User, ManagementFeatureCatalog.GoldBarCompliance, detail.Order.BranchId)) return Forbid();
            if (detail.Product.ProductLegalClass != ProductLegalClassOptions.GoldBarRegulated || serial.ProductId != detail.ProductId) return BadRequest("Serial không khớp sản phẩm vàng miếng.");
            if (serial.Status != GoldBarSerialStatusOptions.Available || serial.BusinessLocation.BranchId != detail.Order.BranchId) return BadRequest("Serial không khả dụng tại chi nhánh của đơn.");
            if (detail.PriceSnapshot == null) return BadRequest("Đơn hàng chưa có snapshot giá.");
            if (kyc.BranchId != detail.Order.BranchId || !kyc.IsVerified) return BadRequest("Hồ sơ KYC chưa được xác minh cho chi nhánh của đơn.");
            if (!await HasValidLicenseAsync(serial.BusinessLocationId)) return BadRequest("Địa điểm chưa có giấy phép còn hiệu lực.");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var now = DateTime.UtcNow;
            var record = new GoldBarSaleRecord { OrderId = detail.OrderId, OrderDetailId = detail.Id, GoldBarSerialId = serial.Id, CustomerKycProfileId = kyc.Id, BusinessLocationId = serial.BusinessLocationId, PriceSnapshotId = detail.PriceSnapshot.Id, CreatedByUserId = actorId, SoldAt = now, RetainUntil = now.AddYears(10), NhnnSubmissionStatus = NhnnSubmissionStatusOptions.Ready };
            serial.Status = GoldBarSerialStatusOptions.Sold;
            _context.GoldBarSaleRecords.Add(record);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> NhnnReport(DateTime? from, DateTime? to)
        {
            if (!await CanAccessAsync()) return Forbid();
            var start = from?.ToUniversalTime() ?? DateTime.UtcNow.Date.AddDays(-30);
            var end = to?.ToUniversalTime() ?? DateTime.UtcNow;
            var records = await _context.GoldBarSaleRecords.Include(item => item.GoldBarSerial).Include(item => item.CustomerKycProfile).Include(item => item.PriceSnapshot).Where(item => item.SoldAt >= start && item.SoldAt <= end).OrderBy(item => item.SoldAt).ToListAsync();
            var csv = new StringBuilder().AppendLine("SaleId,OrderDetailId,SerialNumber,IdentityNumber,TaxCode,SoldAt,Quantity,UnitPrice,TotalAmount,SubmissionStatus");
            foreach (var record in records) csv.AppendLine(string.Join(",", record.Id, record.OrderDetailId, Csv(record.GoldBarSerial.SerialNumber), Csv(record.CustomerKycProfile.IdentityNumber), Csv(record.CustomerKycProfile.TaxCode), record.SoldAt.ToString("O", CultureInfo.InvariantCulture), 1, record.PriceSnapshot.SellUnitPrice.ToString(CultureInfo.InvariantCulture), record.PriceSnapshot.SellUnitPrice.ToString(CultureInfo.InvariantCulture), record.NhnnSubmissionStatus));
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"goldbar-nhnn-{start:yyyyMMdd}-{end:yyyyMMdd}.csv");
        }

        private async Task<bool> CanAccessAsync() => User.IsInRole(RoleCatalog.Admin) || await _permissions.CanAsync(User, ManagementFeatureCatalog.GoldBarCompliance, (await _userManager.GetUserAsync(User))?.BranchId);
        private Task<bool> HasValidLicenseAsync(int locationId) => _context.BusinessLicenses.AnyAsync(item => item.BusinessLocationId == locationId && item.IsVerified && item.ValidFrom <= DateTime.UtcNow && (!item.ValidTo.HasValue || item.ValidTo > DateTime.UtcNow));
        private static string Csv(string value) => $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";
    }
}
