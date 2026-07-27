using GoldManagementSystem.Data;
using GoldManagementSystem.Models;
using GoldManagementSystem.Models.ViewModels;
using GoldManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace GoldManagementSystem.Controllers
{
    [Authorize(Roles = RoleCatalog.ManagementRoles)]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AuthNotificationService _notificationService;
        private readonly InventoryStockService _inventoryStockService;

        public AdminController(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AuthNotificationService notificationService,
            InventoryStockService inventoryStockService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _notificationService = notificationService;
            _inventoryStockService = inventoryStockService;
        }  
        //2//
 
        // 1. Dashboard Quản lý
        [Authorize(Roles = RoleCatalog.Admin)]
        public async Task<IActionResult> Dashboard()
        {
            var totalOrders = await _context.Orders.CountAsync();
            var totalRevenue = await _context.Orders
                .Where(o => o.Status == "Hoàn thành")
                .Select(o => (decimal?)o.TotalAmount)
                .SumAsync() ?? 0m;
            var totalUsers = await _userManager.Users.CountAsync();
            var totalProducts = await _context.Products.CountAsync();

            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.RecentOrders = recentOrders;

            return View();
        }

        // Giữ URL cũ nhưng chuyển sang cổng quản trị riêng.
        public IActionResult Management(string tab = "overview", int? branchId = null)
            => RedirectToAction("Index", "Management", new { tab, branchId });
        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult> SupplierManagement(
            string searchTerm = null,
            string statusFilter = null,
            int? branchId = null)
        {
            ViewBag.SelectedBranchId = branchId;

            return View(
                await BuildSupplierManagementViewModelAsync(
                    searchTerm,
                    statusFilter));
        }

        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult> SupplierPurchaseOrders(
            int? branchId = null)
        {
            ViewBag.SelectedBranchId = branchId;

            ViewBag.ActiveSuppliersForEdit = await _context.Suppliers
                .Where(supplier => supplier.IsActive)
                .OrderBy(supplier => supplier.Name)
                .ToListAsync();

            ViewBag.ActiveBranchesForEdit = await _context.Branches
                .Where(branch => branch.IsActive)
                .OrderBy(branch => branch.BranchName)
                .ToListAsync();

            var currentUser = await _userManager.GetUserAsync(User);
            var scopedBranchId = branchId;

            if (!User.IsInRole(RoleCatalog.Admin)
                && !scopedBranchId.HasValue)
            {
                scopedBranchId = currentUser?.BranchId;
            }
            
            var purchaseOrderQuery = _context.SupplierPurchaseOrders
                .AsNoTracking()
                .Include(order => order.Supplier)
                .Include(order => order.Branch)
                .Include(order => order.Details)
                .Include(order => order.Receipts)
                .Include(order => order.Payments)
                .AsQueryable();

            if (scopedBranchId.HasValue)
            {
                purchaseOrderQuery = purchaseOrderQuery.Where(order =>
                    order.BranchId == scopedBranchId.Value);
            }
            var warehouseQuery =
                    _context.Warehouses
                        .AsNoTracking()
                        .Include(warehouse => warehouse.Branch)
                        .Where(warehouse =>
                            warehouse.IsActive
                            && warehouse.Branch.IsActive
                            && warehouse.LocationType
                                == Warehouse.LocationTypeStorage)
                        .AsQueryable();

            if (scopedBranchId.HasValue)
            {
                warehouseQuery = warehouseQuery.Where(warehouse =>
                    warehouse.BranchId == scopedBranchId.Value);
            }

            ViewBag.ActiveWarehousesForReceipt =
                await warehouseQuery
                    .OrderBy(warehouse => warehouse.Name)
                    .ToListAsync();
            var model = new SupplierManagementViewModel
            {
                PurchaseOrders = await purchaseOrderQuery
                    .OrderByDescending(order => order.CreatedAt)
                    .ToListAsync(),

                SupplierOptions = await BuildActiveSupplierOptionsAsync(),
                BranchOptions = await BuildActiveBranchOptionsAsync(),
                ProductLineOptions = BuildSupplierProductLineOptions()
            };

            return View(model);
        }

        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult> WarehouseManagement(
            int? branchId = null)
        {
            var currentUser =
                await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Forbid();
            }

            /*
            * Admin được xem chi nhánh đang chọn.
            * Các vai trò khác chỉ xem chi nhánh được phân công.
            */
            int? scopedBranchId =
                User.IsInRole(RoleCatalog.Admin)
                    ? branchId
                    : currentUser.BranchId;

            ViewBag.SelectedBranchId =
                scopedBranchId;

            var warehouseQuery =
                _context.Warehouses
                    .AsNoTracking()
                    .Include(warehouse => warehouse.Branch)
                    .Where(warehouse =>
                        warehouse.IsActive
                        && warehouse.Branch.IsActive
                        && warehouse.LocationType
                            == Warehouse.LocationTypeStorage)
                    .AsQueryable();

            var purchaseOrderQuery =
                _context.SupplierPurchaseOrders
                    .AsNoTracking()
                    .Include(order => order.Supplier)
                    .Include(order => order.Branch)
                    .Include(order => order.Details)
                    .Include(order => order.Receipts)
                    .Include(order => order.Payments)
                    .Where(order =>
                        order.Status
                            != SupplierPurchaseOrder.StatusCancelled)
                    .AsQueryable();

            var receiptQuery =
                _context.SupplierGoodsReceipts
                    .AsNoTracking()
                    .Include(receipt =>
                        receipt.SupplierPurchaseOrder)
                        .ThenInclude(order =>
                            order.Supplier)
                    .Include(receipt =>
                        receipt.SupplierPurchaseOrder)
                        .ThenInclude(order =>
                            order.Branch)
                    .Include(receipt =>
                        receipt.Warehouse)
                    .Include(receipt =>
                        receipt.CreatedByUser)
                    .Include(receipt =>
                        receipt.Details)
                        .ThenInclude(detail =>
                            detail.SupplierPurchaseOrderDetail)
                    .AsQueryable();

            if (scopedBranchId.HasValue)
            {
                warehouseQuery =
                    warehouseQuery.Where(warehouse =>
                        warehouse.BranchId
                            == scopedBranchId.Value);

                purchaseOrderQuery =
                    purchaseOrderQuery.Where(order =>
                        order.BranchId
                            == scopedBranchId.Value);

                receiptQuery =
                    receiptQuery.Where(receipt =>
                        receipt.SupplierPurchaseOrder.BranchId
                            == scopedBranchId.Value);
            }
            else if (!User.IsInRole(RoleCatalog.Admin))
            {
                warehouseQuery =
                    warehouseQuery.Where(item => false);

                purchaseOrderQuery =
                    purchaseOrderQuery.Where(item => false);

                receiptQuery =
                    receiptQuery.Where(item => false);
            }

            ViewBag.ActiveWarehousesForReceipt =
                await warehouseQuery
                    .OrderBy(warehouse =>
                        warehouse.Name)
                    .ToListAsync();

            var purchaseOrders =
                await purchaseOrderQuery
                    .OrderByDescending(order =>
                        order.CreatedAt)
                    .ToListAsync();

            var receipts =
                await receiptQuery
                    .OrderByDescending(receipt =>
                        receipt.ReceivedAt)
                    .ToListAsync();

            var model =
                new SupplierManagementViewModel
                {
                    PurchaseOrders =
                        purchaseOrders,

                    RecentReceipts =
                        receipts,

                    ActivePurchaseOrders =
                        purchaseOrders.Count(order =>
                            order.Status
                                != SupplierPurchaseOrder.StatusReceived),

                    PendingReceiptCount =
                        receipts.Count(receipt =>
                            receipt.Status
                                == SupplierGoodsReceipt.StatusPendingApproval)
                };

            return View(model);
        }
                
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult> CreateSupplier(
            string Name,
            string TaxCode,
            string ContactPerson,
            string Phone,
            string Email,
            string Address,
            string[] SupplierTypes,
            int PaymentTermDays,
            string BankName,
            string BankAccountNumber,
            string BankAccountName,
            string Note)
        {
            // Kiểm tra request có được gửi bằng AJAX hay không
            var isAjaxRequest = string.Equals(
                Request.Headers["X-Requested-With"].ToString(),
                "XMLHttpRequest",
                StringComparison.OrdinalIgnoreCase);

            // Chuẩn hóa dữ liệu
            Name = NormalizeOrEmpty(Name);
            TaxCode = KeepDigitsOnly(TaxCode);
            ContactPerson = NormalizeOrEmpty(ContactPerson);
            Phone = KeepDigitsOnly(Phone);
            Email = NormalizeOrEmpty(Email);
            Address = NormalizeOrEmpty(Address);
            BankName = NormalizeOrEmpty(BankName);
            BankAccountNumber = KeepDigitsOnly(BankAccountNumber);
            BankAccountName = NormalizeOrEmpty(BankAccountName);
            Note = NormalizeOrEmpty(Note);

            var supplierTypeText = BuildSupplierTypeText(SupplierTypes);

            // Kiểm tra dữ liệu hợp lệ
            var validationError = await ValidateSupplierInputAsync(
                null,
                Name,
                TaxCode,
                ContactPerson,
                Phone,
                Email,
                Address,
                supplierTypeText,
                PaymentTermDays,
                BankName,
                BankAccountNumber,
                BankAccountName);

            // Có lỗi thì trả lỗi về popup, không đóng popup
            if (!string.IsNullOrWhiteSpace(validationError))
            {
                if (isAjaxRequest)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = validationError
                    });
                }

                TempData["ErrorMessage"] = validationError;
                return RedirectToAction(nameof(SupplierManagement));
            }

            var supplier = new Supplier
            {
                Name = Name,
                TaxCode = TaxCode,
                ContactPerson = ContactPerson,
                Phone = Phone,
                Email = Email,
                Address = Address,
                SupplierType = supplierTypeText,
                PaymentTermDays = PaymentTermDays,
                BankName = BankName,
                BankAccountNumber = BankAccountNumber,
                BankAccountName = BankAccountName,
                Note = Note,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                _context.Suppliers.Add(supplier);

                var affectedRows = await _context.SaveChangesAsync();

                // Kiểm tra chắc chắn database đã tạo bản ghi
                if (affectedRows <= 0 || supplier.Id <= 0)
                {
                    const string saveError =
                        "Không thể lưu nhà cung cấp vào cơ sở dữ liệu. Vui lòng thử lại.";

                    if (isAjaxRequest)
                    {
                        return StatusCode(500, new
                        {
                            success = false,
                            message = saveError
                        });
                    }

                    TempData["ErrorMessage"] = saveError;
                    return RedirectToAction(nameof(SupplierManagement));
                }
            }
            catch (DbUpdateException ex)
            {
                // In lỗi thật ra Terminal để kiểm tra khi chạy dự án
                Console.WriteLine("Lỗi thêm nhà cung cấp:");
                Console.WriteLine(ex.InnerException?.Message ?? ex.Message);

                const string databaseError =
                    "Không thể lưu nhà cung cấp. Hãy kiểm tra database, migration hoặc dữ liệu bị trùng.";

                if (isAjaxRequest)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = databaseError
                    });
                }

                TempData["ErrorMessage"] = databaseError;
                return RedirectToAction(nameof(SupplierManagement));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi không xác định khi thêm nhà cung cấp:");
                Console.WriteLine(ex.Message);

                const string unknownError =
                    "Đã xảy ra lỗi khi thêm nhà cung cấp. Vui lòng kiểm tra Terminal.";

                if (isAjaxRequest)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = unknownError
                    });
                }

                TempData["ErrorMessage"] = unknownError;
                return RedirectToAction(nameof(SupplierManagement));
            }

            var successMessage = $"Đã thêm nhà cung cấp {Name}.";
            TempData["SuccessMessage"] = successMessage;

            // AJAX nhận JSON thành công
            if (isAjaxRequest)
            {
                return Json(new
                {
                    success = true,
                    message = successMessage,
                    supplierId = supplier.Id
                });
            }

            return RedirectToAction(nameof(SupplierManagement));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult> CreateSupplierPurchaseOrder(
            int SupplierId,
            int BranchId,
            DateTime? ExpectedDeliveryDate,
            string PurchaseOrderNote,
            string[] ProductLines,
            string[] Categories,
            string[] CustomCategories,
            string[] ProductNames,
            string[] GoldTypes,
            string[] CustomGoldTypes,
            string[] Quantities,
            string[] Weights,
            string[] DiamondCarats,
            string[] DiamondCertificates,
            string[] UnitCosts,
            string[] DetailNotes)
        {
            PurchaseOrderNote = NormalizeOrEmpty(PurchaseOrderNote);

            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(item => item.Id == SupplierId && item.IsActive);

            if (supplier == null)
            {
                TempData["ErrorMessage"] = "Nhà cung cấp không tồn tại hoặc đã tạm ngưng.";
                return RedirectToAction(nameof(SupplierPurchaseOrders));
            }

            var branch = await _context.Branches
                .FirstOrDefaultAsync(item => item.Id == BranchId && item.IsActive);

            if (branch == null)
            {
                TempData["ErrorMessage"] = "Chi nhánh nhận hàng không tồn tại hoặc đã tạm khóa.";
                return RedirectToAction(nameof(SupplierPurchaseOrders));
            }

            if (!ExpectedDeliveryDate.HasValue || ExpectedDeliveryDate.Value.Date < DateTime.Today)
            {
                TempData["ErrorMessage"] = "Ngày dự kiến giao hàng không được nhỏ hơn ngày hiện tại.";
                return RedirectToAction(nameof(SupplierPurchaseOrders));
            }

            string GetValue(string[] values, int index)
            {
                if (values == null || index < 0 || index >= values.Length)
                {
                    return string.Empty;
                }

                return NormalizeOrEmpty(values[index]);
            }

            int ParseIntValue(string value)
            {
                return int.TryParse(value, out var result) ? result : 0;
            }

            decimal ParseDecimalValue(string value)
            {
                value = NormalizeOrEmpty(value).Replace(",", ".");

                return decimal.TryParse(
                    value,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out var result)
                    ? result
                    : 0m;
            }

            string ResolveSelectValue(string selectedValue, string customValue)
            {
                selectedValue = NormalizeOrEmpty(selectedValue);
                customValue = NormalizeOrEmpty(customValue);

                if (string.Equals(selectedValue, "__other__", StringComparison.OrdinalIgnoreCase))
                {
                    return customValue;
                }

                return selectedValue;
            }

            var rowCount = new[]
            {
                ProductLines?.Length ?? 0,
                Categories?.Length ?? 0,
                ProductNames?.Length ?? 0,
                GoldTypes?.Length ?? 0,
                Quantities?.Length ?? 0,
                UnitCosts?.Length ?? 0
            }.Max();

            var details = new List<SupplierPurchaseOrderDetail>();

            for (var i = 0; i < rowCount; i++)
            {
                var productLine = GetValue(ProductLines, i);
                var category = ResolveSelectValue(GetValue(Categories, i), GetValue(CustomCategories, i));
                var productName = GetValue(ProductNames, i);
                var goldType = ResolveSelectValue(GetValue(GoldTypes, i), GetValue(CustomGoldTypes, i));
                var quantity = ParseIntValue(GetValue(Quantities, i));
                var weight = ParseDecimalValue(GetValue(Weights, i));
                var diamondCaratValue = ParseDecimalValue(GetValue(DiamondCarats, i));
                var diamondCarat = diamondCaratValue > 0 ? diamondCaratValue : (decimal?)null;
                var diamondCertificate = GetValue(DiamondCertificates, i);
                var unitCost = ParseDecimalValue(GetValue(UnitCosts, i));
                var detailNote = GetValue(DetailNotes, i);

                var isEmptyRow =
                    string.IsNullOrWhiteSpace(productLine)
                    && string.IsNullOrWhiteSpace(category)
                    && string.IsNullOrWhiteSpace(productName)
                    && string.IsNullOrWhiteSpace(goldType)
                    && quantity <= 0
                    && unitCost <= 0;

                if (isEmptyRow)
                {
                    continue;
                }

                var rowNumber = i + 1;

                if (!AllowedPurchaseOrderProductLines.Any(item => string.Equals(item, productLine, StringComparison.OrdinalIgnoreCase)))
                {
                    TempData["ErrorMessage"] = $"Dòng {rowNumber}: Vui lòng chọn nhóm hàng hợp lệ.";
                    return RedirectToAction(nameof(SupplierPurchaseOrders));
                }

                if (!SupplierSupportsPurchaseProductLine(supplier, productLine))
                {
                    TempData["ErrorMessage"] = $"Dòng {rowNumber}: Nhà cung cấp không có nhóm cung ứng phù hợp với dòng hàng {productLine}.";
                    return RedirectToAction(nameof(SupplierPurchaseOrders));
                }

                if (string.IsNullOrWhiteSpace(category))
                {
                    TempData["ErrorMessage"] = $"Dòng {rowNumber}: Vui lòng chọn hoặc nhập danh mục hàng.";
                    return RedirectToAction(nameof(SupplierPurchaseOrders));
                }

                if (string.IsNullOrWhiteSpace(productName))
                {
                    TempData["ErrorMessage"] = $"Dòng {rowNumber}: Vui lòng nhập tên sản phẩm dự kiến nhập.";
                    return RedirectToAction(nameof(SupplierPurchaseOrders));
                }

                if (string.IsNullOrWhiteSpace(goldType))
                {
                    TempData["ErrorMessage"] = $"Dòng {rowNumber}: Vui lòng chọn hoặc nhập chất liệu / phân loại.";
                    return RedirectToAction(nameof(SupplierPurchaseOrders));
                }

                if (quantity <= 0)
                {
                    TempData["ErrorMessage"] = $"Dòng {rowNumber}: Số lượng đặt hàng phải lớn hơn 0.";
                    return RedirectToAction(nameof(SupplierPurchaseOrders));
                }

                if (unitCost <= 0)
                {
                    TempData["ErrorMessage"] = $"Dòng {rowNumber}: Đơn giá nhập phải lớn hơn 0.";
                    return RedirectToAction(nameof(SupplierPurchaseOrders));
                }

                if (IsWeightRequiredPurchaseLine(productLine) && weight <= 0)
                {
                    TempData["ErrorMessage"] = $"Dòng {rowNumber}: Trọng lượng phải lớn hơn 0 đối với vàng hoặc bạc.";
                    return RedirectToAction(nameof(SupplierPurchaseOrders));
                }

                if (IsCaratRequiredPurchaseLine(productLine) && (!diamondCarat.HasValue || diamondCarat.Value <= 0))
                {
                    TempData["ErrorMessage"] = $"Dòng {rowNumber}: Carat phải lớn hơn 0 đối với kim cương hoặc đá quý.";
                    return RedirectToAction(nameof(SupplierPurchaseOrders));
                }

                if (!IsWeightRequiredPurchaseLine(productLine))
                {
                    weight = 0;
                }

                if (!IsCaratRequiredPurchaseLine(productLine))
                {
                    diamondCarat = null;
                }

                var totalCost = quantity * unitCost;

                details.Add(new SupplierPurchaseOrderDetail
                {
                    ProductLine = productLine,
                    Category = category,
                    ProductName = productName,
                    GoldType = goldType,
                    Quantity = quantity,
                    Weight = weight,
                    DiamondCarat = diamondCarat,
                    DiamondCertificate = diamondCertificate,
                    UnitCost = unitCost,
                    TotalCost = totalCost,
                    ReceivedQuantity = 0,
                    AcceptedQuantity = 0,
                    RejectedQuantity = 0,
                    Note = detailNote
                });
            }

            if (!details.Any())
            {
                TempData["ErrorMessage"] = "Vui lòng thêm ít nhất một dòng hàng hợp lệ cho đơn đặt hàng.";
                return RedirectToAction(nameof(SupplierPurchaseOrders));
            }

            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Forbid();
            }

            var purchaseOrder = new SupplierPurchaseOrder
            {
                OrderCode = $"PO-{DateTime.Now:yyyyMMddHHmmss}",
                SupplierId = supplier.Id,
                BranchId = branch.Id,
                CreatedByUserId = currentUser.Id,
                CreatedAt = DateTime.Now,
                ExpectedDeliveryDate = ExpectedDeliveryDate.Value.Date,
                Status = SupplierPurchaseOrder.StatusOrdered,
                TotalAmount = details.Sum(item => item.TotalCost),
                Note = PurchaseOrderNote
            };

            foreach (var detail in details)
            {
                purchaseOrder.Details.Add(detail);
            }

            _context.SupplierPurchaseOrders.Add(purchaseOrder);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã tạo đơn đặt hàng NCC {purchaseOrder.OrderCode}.";
            return RedirectToAction(nameof(SupplierPurchaseOrders));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult> UpdateSupplierPurchaseOrder(
            int PurchaseOrderId,
            int SupplierId,
            int BranchId,
            DateTime? ExpectedDeliveryDate,
            string PurchaseOrderNote,
            string[] ProductLines,
            string[] Categories,
            string[] ProductNames,
            string[] GoldTypes,
            string[] Quantities,
            string[] Weights,
            string[] DiamondCarats,
            string[] DiamondCertificates,
            string[] UnitCosts,
            string[] DetailNotes)
        {
            var order = await _context.SupplierPurchaseOrders
                .Include(item => item.Details)
                .Include(item => item.Receipts)
                .Include(item => item.Payments)
                .FirstOrDefaultAsync(item => item.Id == PurchaseOrderId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn đặt hàng nhà cung cấp.";
                return RedirectToAction(nameof(SupplierPurchaseOrders));
            }

            if (order.Status != SupplierPurchaseOrder.StatusOrdered)
            {
                TempData["ErrorMessage"] = "Chỉ được sửa đơn đặt hàng đang ở trạng thái Đã đặt hàng.";
                return RedirectToAction(nameof(SupplierPurchaseOrders));
            }

            if ((order.Receipts != null && order.Receipts.Any()) || (order.Payments != null && order.Payments.Any()))
            {
                TempData["ErrorMessage"] = "Không thể sửa đơn đã phát sinh kiểm hàng hoặc thanh toán.";
                return RedirectToAction(nameof(SupplierPurchaseOrders));
            }

            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(item => item.Id == SupplierId && item.IsActive);

            if (supplier == null)
            {
                TempData["ErrorMessage"] = "Nhà cung cấp không tồn tại hoặc đã tạm ngưng.";
                return RedirectToAction(nameof(SupplierPurchaseOrders));
            }

            var branch = await _context.Branches
                .FirstOrDefaultAsync(item => item.Id == BranchId && item.IsActive);

            if (branch == null)
            {
                TempData["ErrorMessage"] = "Chi nhánh nhận hàng không tồn tại hoặc đã tạm khóa.";
                return RedirectToAction(nameof(SupplierPurchaseOrders));
            }

            if (!ExpectedDeliveryDate.HasValue || ExpectedDeliveryDate.Value.Date < DateTime.Today)
            {
                TempData["ErrorMessage"] = "Ngày dự kiến giao hàng không được nhỏ hơn ngày hiện tại.";
                return RedirectToAction(nameof(SupplierPurchaseOrders));
            }

            var detailResult = BuildSupplierPurchaseOrderDetails(
                supplier,
                ProductLines,
                Categories,
                ProductNames,
                GoldTypes,
                Quantities,
                Weights,
                DiamondCarats,
                DiamondCertificates,
                UnitCosts,
                DetailNotes);

            if (!detailResult.IsValid)
            {
                TempData["ErrorMessage"] = detailResult.ErrorMessage;
                return RedirectToAction(nameof(SupplierPurchaseOrders));
            }

            order.SupplierId = supplier.Id;
            order.BranchId = branch.Id;
            order.ExpectedDeliveryDate = ExpectedDeliveryDate.Value.Date;
            order.Note = NormalizeOrEmpty(PurchaseOrderNote);
            order.TotalAmount = detailResult.Details.Sum(item => item.TotalCost);

            _context.SupplierPurchaseOrderDetails.RemoveRange(order.Details);

            foreach (var detail in detailResult.Details)
            {
                order.Details.Add(detail);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã cập nhật đơn đặt hàng NCC {order.OrderCode}.";
            return RedirectToAction(nameof(SupplierPurchaseOrders));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult> CancelSupplierPurchaseOrder(int purchaseOrderId)
        {
            var order = await _context.SupplierPurchaseOrders
                .Include(item => item.Receipts)
                .Include(item => item.Payments)
                .FirstOrDefaultAsync(item => item.Id == purchaseOrderId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn đặt hàng nhà cung cấp.";
                return RedirectToAction(nameof(SupplierPurchaseOrders));
            }

            if (order.Status != SupplierPurchaseOrder.StatusOrdered)
            {
                TempData["ErrorMessage"] = "Chỉ được hủy đơn đặt hàng đang ở trạng thái Đã đặt hàng.";
                return RedirectToAction(nameof(SupplierPurchaseOrders));
            }

            if ((order.Receipts != null && order.Receipts.Any()) || (order.Payments != null && order.Payments.Any()))
            {
                TempData["ErrorMessage"] = "Không thể hủy đơn đã phát sinh kiểm hàng hoặc thanh toán.";
                return RedirectToAction(nameof(SupplierPurchaseOrders));
            }

            order.Status = SupplierPurchaseOrder.StatusCancelled;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã hủy đơn đặt hàng NCC {order.OrderCode}.";
            return RedirectToAction(nameof(SupplierPurchaseOrders));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult> CreateSupplierGoodsReceipt(
            int PurchaseOrderId,
            int WarehouseId,
            DateTime? ReceivedAt,
            string DeliveryDocumentNumber,
            string DeliveredBy,
            string ReceiptNote,
            int[] PurchaseOrderDetailIds,
            string[] ReceivedQuantities,
            string[] ActualWeights,
            string[] ActualDiamondCarats,
            string[] ActualDiamondCertificates,
            string[] ReceivingNotes)
        {
            DeliveryDocumentNumber =
                NormalizeOrEmpty(DeliveryDocumentNumber);

            DeliveredBy =
                NormalizeOrEmpty(DeliveredBy);

            ReceiptNote =
                NormalizeOrEmpty(ReceiptNote);

            if (DeliveryDocumentNumber.Length > 100)
            {
                TempData["ErrorMessage"] =
                    "Số chứng từ giao hàng không được vượt quá 100 ký tự.";

                return RedirectToAction(nameof(WarehouseManagement));
            }

            if (DeliveredBy.Length > 150)
            {
                TempData["ErrorMessage"] =
                    "Tên người giao hàng không được vượt quá 150 ký tự.";

                return RedirectToAction(nameof(WarehouseManagement));
            }

            if (ReceiptNote.Length > 1000)
            {
                TempData["ErrorMessage"] =
                    "Ghi chú phiếu nhận không được vượt quá 1000 ký tự.";

                return RedirectToAction(nameof(WarehouseManagement));
            }

            var order = await _context.SupplierPurchaseOrders
                .Include(item => item.Supplier)
                .Include(item => item.Branch)
                .Include(item => item.Details)
                .Include(item => item.Receipts)
                    .ThenInclude(receipt => receipt.Details)
                .FirstOrDefaultAsync(item =>
                    item.Id == PurchaseOrderId);

            if (order == null)
            {
                TempData["ErrorMessage"] =
                    "Không tìm thấy đơn đặt hàng nhà cung cấp.";

                return RedirectToAction(nameof(WarehouseManagement));
            }

            if (order.Status == SupplierPurchaseOrder.StatusCancelled)
            {
                TempData["ErrorMessage"] =
                    "Không thể nhận hàng cho đơn đã bị hủy.";

                return RedirectToAction(nameof(WarehouseManagement));
            }

            if (order.Status == SupplierPurchaseOrder.StatusReceived)
            {
                TempData["ErrorMessage"] =
                    "Đơn đặt hàng này đã được nhận đủ.";

                return RedirectToAction(nameof(WarehouseManagement));
            }

            if (order.Status != SupplierPurchaseOrder.StatusOrdered
                && order.Status
                    != SupplierPurchaseOrder.StatusPartiallyReceived)
            {
                TempData["ErrorMessage"] =
                    "Trạng thái đơn đặt hàng không cho phép tạo phiếu nhận.";

                return RedirectToAction(nameof(WarehouseManagement));
            }

            var currentUser =
                await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Forbid();
            }

            /*
            * Manager và Branch Owner chỉ được nhận đơn của
            * chi nhánh đang được phân công.
            */
            if (!User.IsInRole(RoleCatalog.Admin)
                && currentUser.BranchId != order.BranchId)
            {
                TempData["ErrorMessage"] =
                    "Bạn không có quyền nhận hàng cho chi nhánh này.";

                return RedirectToAction(nameof(WarehouseManagement));
            }

            var warehouse = await _context.Warehouses
                .Include(item => item.Branch)
                .FirstOrDefaultAsync(item =>
                    item.Id == WarehouseId
                    && item.IsActive
                    && item.LocationType
                        == Warehouse.LocationTypeStorage);

            if (warehouse == null)
            {
                TempData["ErrorMessage"] =
                    "Kho nhận hàng không tồn tại, đã tạm ngưng hoặc không phải kho lưu trữ.";

                return RedirectToAction(nameof(WarehouseManagement));
            }

            if (warehouse.BranchId != order.BranchId)
            {
                TempData["ErrorMessage"] =
                    "Kho được chọn không thuộc chi nhánh nhận hàng của đơn đặt hàng.";

                return RedirectToAction(nameof(WarehouseManagement));
            }

            if (!ReceivedAt.HasValue)
            {
                TempData["ErrorMessage"] =
                    "Vui lòng nhập thời điểm nhận hàng.";

                return RedirectToAction(nameof(WarehouseManagement));
            }

            if (ReceivedAt.Value > DateTime.Now.AddMinutes(5))
            {
                TempData["ErrorMessage"] =
                    "Thời điểm nhận hàng không được lớn hơn thời điểm hiện tại.";

                return RedirectToAction(nameof(WarehouseManagement));
            }

            if (ReceivedAt.Value.Date < order.CreatedAt.Date)
            {
                TempData["ErrorMessage"] =
                    "Thời điểm nhận hàng không được trước ngày tạo đơn.";

                return RedirectToAction(nameof(WarehouseManagement));
            }

            if (PurchaseOrderDetailIds == null
                || PurchaseOrderDetailIds.Length == 0)
            {
                TempData["ErrorMessage"] =
                    "Đơn đặt hàng không có dòng hàng để nhận.";

                return RedirectToAction(nameof(WarehouseManagement));
            }

            var orderDetailDictionary =
                order.Details.ToDictionary(
                    detail => detail.Id);

            var receiptDetails =
                new List<SupplierGoodsReceiptDetail>();

            var receivedQuantityByDetail =
                new Dictionary<int, int>();

            var submittedDetailIds =
                new HashSet<int>();

            for (var index = 0;
                index < PurchaseOrderDetailIds.Length;
                index++)
            {
                var detailId =
                    PurchaseOrderDetailIds[index];

                if (!submittedDetailIds.Add(detailId))
                {
                    TempData["ErrorMessage"] =
                        "Dữ liệu dòng hàng nhận bị trùng.";

                    return RedirectToAction(
                        nameof(WarehouseManagement));
                }

                if (!orderDetailDictionary.TryGetValue(
                    detailId,
                    out var orderDetail))
                {
                    TempData["ErrorMessage"] =
                        "Có dòng hàng không thuộc đơn đặt hàng.";

                    return RedirectToAction(
                        nameof(WarehouseManagement));
                }

                var receivedQuantity =
                    ParseSupplierInt(
                        GetArrayValue(
                            ReceivedQuantities,
                            index));

                var actualWeight =
                    ParseSupplierDecimal(
                        GetArrayValue(
                            ActualWeights,
                            index));

                var actualCaratValue =
                    ParseSupplierDecimal(
                        GetArrayValue(
                            ActualDiamondCarats,
                            index));

                var actualCarat =
                    actualCaratValue > 0
                        ? actualCaratValue
                        : (decimal?)null;

                var actualCertificate =
                    GetArrayValue(
                        ActualDiamondCertificates,
                        index);

                var receivingNote =
                    GetArrayValue(
                        ReceivingNotes,
                        index);

                /*
                * Cho phép nhập 0 để bỏ qua một dòng trong lần giao này.
                */
                if (receivedQuantity == 0)
                {
                    continue;
                }

                if (receivedQuantity < 0)
                {
                    TempData["ErrorMessage"] =
                        $"Dòng {index + 1}: "
                        + "Số lượng nhận không được nhỏ hơn 0.";

                    return RedirectToAction(
                        nameof(WarehouseManagement));
                }

                var remainingQuantity =
                    orderDetail.Quantity
                    - orderDetail.ReceivedQuantity;

                if (remainingQuantity <= 0)
                {
                    TempData["ErrorMessage"] =
                        $"Dòng {index + 1}: "
                        + "Hàng này đã được nhận đủ.";

                    return RedirectToAction(
                        nameof(WarehouseManagement));
                }

                if (receivedQuantity > remainingQuantity)
                {
                    TempData["ErrorMessage"] =
                        $"Dòng {index + 1}: "
                        + $"Chỉ còn được nhận tối đa {remainingQuantity} sản phẩm.";

                    return RedirectToAction(
                        nameof(WarehouseManagement));
                }

                if (IsWeightRequiredPurchaseLine(
                    orderDetail.ProductLine)
                    && actualWeight <= 0)
                {
                    TempData["ErrorMessage"] =
                        $"Dòng {index + 1}: "
                        + "Vui lòng nhập trọng lượng thực nhận.";

                    return RedirectToAction(
                        nameof(WarehouseManagement));
                }

                if (IsCaratRequiredPurchaseLine(
                    orderDetail.ProductLine)
                    && (!actualCarat.HasValue
                        || actualCarat.Value <= 0))
                {
                    TempData["ErrorMessage"] =
                        $"Dòng {index + 1}: "
                        + "Vui lòng nhập carat thực nhận.";

                    return RedirectToAction(
                        nameof(WarehouseManagement));
                }

                if (!IsWeightRequiredPurchaseLine(
                    orderDetail.ProductLine))
                {
                    actualWeight = 0;
                }

                if (!IsCaratRequiredPurchaseLine(
                    orderDetail.ProductLine))
                {
                    actualCarat = null;
                    actualCertificate = string.Empty;
                }

                receiptDetails.Add(
                    new SupplierGoodsReceiptDetail
                    {
                        SupplierPurchaseOrderDetailId =
                            orderDetail.Id,

                        ReceivedQuantity =
                            receivedQuantity,

                        AcceptedQuantity = 0,
                        RejectedQuantity = 0,

                        ActualWeight =
                            actualWeight,

                        ActualDiamondCarat =
                            actualCarat,

                        ActualDiamondCertificate =
                            actualCertificate,

                        ActualUnitCost =
                            orderDetail.UnitCost,

                        LineValue = 0,

                        QualityStatus =
                            SupplierGoodsReceiptDetail
                                .QualityPending,

                        Resolution =
                            SupplierGoodsReceiptDetail
                                .ResolutionNone,

                        ReceivingNote =
                            receivingNote
                    });

                receivedQuantityByDetail[orderDetail.Id] =
                    receivedQuantity;
            }

            if (!receiptDetails.Any())
            {
                TempData["ErrorMessage"] =
                    "Vui lòng nhập số lượng nhận cho ít nhất một dòng hàng.";

                return RedirectToAction(nameof(WarehouseManagement));
            }
            try
            {
                var receipt = new SupplierGoodsReceipt
                {
                    ReceiptCode =
                        BuildSupplierGoodsReceiptCode(),

                    SupplierPurchaseOrderId =
                        order.Id,

                    WarehouseId =
                        warehouse.Id,

                    CreatedByUserId =
                        currentUser.Id,

                    ReceivedAt =
                        ReceivedAt.Value,

                    Status =
                        SupplierGoodsReceipt.StatusPendingInspection,

                    DeliveryDocumentNumber =
                        DeliveryDocumentNumber,

                    DeliveredBy =
                        DeliveredBy,

                    TotalAcceptedValue = 0,

                    Note =
                        ReceiptNote
                };

                // Thêm các dòng hàng vào phiếu nhận
                foreach (var receiptDetail in receiptDetails)
                {
                    receipt.Details.Add(receiptDetail);
                }

                // Cập nhật số lượng đã nhận của từng dòng đơn đặt hàng
                foreach (var quantityEntry in receivedQuantityByDetail)
                {
                    var orderDetail =
                        orderDetailDictionary[quantityEntry.Key];

                    orderDetail.ReceivedQuantity +=
                        quantityEntry.Value;
                }

                // Kiểm tra đơn đã nhận đủ tất cả dòng hàng chưa
                var allItemsReceived =
                    order.Details.All(detail =>
                        detail.ReceivedQuantity >= detail.Quantity);

                order.Status = allItemsReceived
                    ? SupplierPurchaseOrder.StatusReceived
                    : SupplierPurchaseOrder.StatusPartiallyReceived;

                _context.SupplierGoodsReceipts.Add(receipt);

                /*
                * Chỉ gọi SaveChangesAsync một lần.
                * EF Core sẽ tự sử dụng transaction cho toàn bộ:
                *
                * - Phiếu nhận hàng
                * - Chi tiết phiếu nhận
                * - Số lượng đã nhận
                * - Trạng thái đơn đặt hàng
                */
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    $"Đã tạo phiếu nhận hàng {receipt.ReceiptCode}. "
                    + "Hàng đang chờ kiểm tra và chưa được nhập kho.";

                return RedirectToAction(
                    nameof(WarehouseManagement));
            }
            catch (Exception exception)
            {
                var errorMessage =
                    exception.GetBaseException().Message;

                Console.WriteLine("====================================");
                Console.WriteLine("LỖI TẠO PHIẾU NHẬN HÀNG NCC");
                Console.WriteLine(errorMessage);
                Console.WriteLine(exception);
                Console.WriteLine("====================================");

                TempData["ErrorMessage"] =
                    "Không thể tạo phiếu nhận hàng. Chi tiết lỗi: "
                    + errorMessage;

                return RedirectToAction(
                    nameof(WarehouseManagement));
            }           
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult> MarkSupplierPurchaseOrderAsShipping(
            int purchaseOrderId)
        {
            var order = await _context.SupplierPurchaseOrders
                .Include(item => item.Details)
                .FirstOrDefaultAsync(item =>
                    item.Id == purchaseOrderId);

            if (order == null)
            {
                TempData["ErrorMessage"] =
                    "Không tìm thấy đơn đặt hàng nhà cung cấp.";

                return RedirectToAction(
                    nameof(WarehouseManagement));
            }

            var currentUser =
                await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Forbid();
            }

            if (!User.IsInRole(RoleCatalog.Admin)
                && currentUser.BranchId != order.BranchId)
            {
                TempData["ErrorMessage"] =
                    "Bạn không có quyền cập nhật đơn của chi nhánh này.";

                return RedirectToAction(
                    nameof(WarehouseManagement),
                    new { branchId = order.BranchId });
            }

            var canStartShipping =
                order.Status == SupplierPurchaseOrder.StatusOrdered
                || order.Status
                    == SupplierPurchaseOrder.StatusPartiallyReceived
                || order.Status
                    == SupplierPurchaseOrder.StatusAwaitingReplacement;

            if (!canStartShipping)
            {
                TempData["ErrorMessage"] =
                    "Trạng thái hiện tại không cho phép chuyển sang Đang giao.";

                return RedirectToAction(
                    nameof(WarehouseManagement),
                    new { branchId = order.BranchId });
            }

            order.Status =
                SupplierPurchaseOrder.StatusShipping;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Đơn {order.OrderCode} đã chuyển sang trạng thái Đang giao.";

            return RedirectToAction(
                nameof(WarehouseManagement),
                new { branchId = order.BranchId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult> ConfirmSupplierDeliveryInspection(
            int PurchaseOrderId,
            int WarehouseId,
            DateTime? ReceivedAt,
            string DeliveryDocumentNumber,
            string DeliveredBy,
            string ReceiptNote,
            int[] PurchaseOrderDetailIds,
            string[] DeliveredQuantities,
            string[] AcceptedQuantities,
            string[] AcceptedWeights,
            string[] AcceptedDiamondCarats,
            string[] AcceptedDiamondCertificates,
            string[] RejectionReasons)
        {
            DeliveryDocumentNumber =
                NormalizeOrEmpty(DeliveryDocumentNumber);

            DeliveredBy =
                NormalizeOrEmpty(DeliveredBy);

            ReceiptNote =
                NormalizeOrEmpty(ReceiptNote);

            if (DeliveryDocumentNumber.Length > 100)
            {
                TempData["ErrorMessage"] =
                    "Số chứng từ giao hàng không được vượt quá 100 ký tự.";

                return RedirectToAction(
                    nameof(WarehouseManagement));
            }

            if (DeliveredBy.Length > 150)
            {
                TempData["ErrorMessage"] =
                    "Tên người giao hàng không được vượt quá 150 ký tự.";

                return RedirectToAction(
                    nameof(WarehouseManagement));
            }

            if (ReceiptNote.Length > 1000)
            {
                TempData["ErrorMessage"] =
                    "Ghi chú không được vượt quá 1000 ký tự.";

                return RedirectToAction(
                    nameof(WarehouseManagement));
            }

            var order = await _context.SupplierPurchaseOrders
                .Include(item => item.Supplier)
                .Include(item => item.Branch)
                .Include(item => item.Details)
                .FirstOrDefaultAsync(item =>
                    item.Id == PurchaseOrderId);

            if (order == null)
            {
                TempData["ErrorMessage"] =
                    "Không tìm thấy đơn đặt hàng.";

                return RedirectToAction(
                    nameof(WarehouseManagement));
            }

            if (order.Status != SupplierPurchaseOrder.StatusShipping)
            {
                TempData["ErrorMessage"] =
                    "Chỉ được kiểm hàng khi đơn đang ở trạng thái Đang giao.";

                return RedirectToAction(
                        nameof(WarehouseManagement),
                        new
                        {
                            branchId = order.BranchId,
                            warehouseTab = "receipts"
                        });
            }

            var currentUser =
                await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Forbid();
            }

            if (!User.IsInRole(RoleCatalog.Admin)
                && currentUser.BranchId != order.BranchId)
            {
                TempData["ErrorMessage"] =
                    "Bạn không có quyền kiểm hàng của chi nhánh này.";

                return RedirectToAction(
                    nameof(WarehouseManagement),
                    new { branchId = order.BranchId });
            }

            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(item =>
                    item.Id == WarehouseId
                    && item.IsActive
                    && item.LocationType
                        == Warehouse.LocationTypeStorage);

            if (warehouse == null)
            {
                TempData["ErrorMessage"] =
                    "Kho nhận hàng không tồn tại, đã tạm ngưng hoặc không phải kho lưu trữ.";

                return RedirectToAction(
                    nameof(WarehouseManagement),
                    new { branchId = order.BranchId });
            }

            if (warehouse.BranchId != order.BranchId)
            {
                TempData["ErrorMessage"] =
                    "Kho nhận không thuộc chi nhánh của đơn đặt hàng.";

                return RedirectToAction(
                    nameof(WarehouseManagement),
                    new { branchId = order.BranchId });
            }

            if (!ReceivedAt.HasValue)
            {
                TempData["ErrorMessage"] =
                    "Vui lòng nhập thời điểm hàng tới kho.";

                return RedirectToAction(
                    nameof(WarehouseManagement),
                    new { branchId = order.BranchId });
            }

            if (ReceivedAt.Value > DateTime.Now.AddMinutes(5))
            {
                TempData["ErrorMessage"] =
                    "Thời điểm hàng tới không được lớn hơn hiện tại.";

                return RedirectToAction(
                    nameof(WarehouseManagement),
                    new { branchId = order.BranchId });
            }

            if (PurchaseOrderDetailIds == null
                || PurchaseOrderDetailIds.Length == 0)
            {
                TempData["ErrorMessage"] =
                    "Đơn đặt hàng không có hàng để kiểm tra.";

                return RedirectToAction(
                    nameof(WarehouseManagement),
                    new { branchId = order.BranchId });
            }

            var orderDetailDictionary =
                order.Details.ToDictionary(detail => detail.Id);

            var receiptDetails =
                new List<SupplierGoodsReceiptDetail>();

            var submittedDetailIds =
                new HashSet<int>();

            var totalDeliveredQuantity = 0;
            var totalAcceptedQuantity = 0;
            var totalRejectedQuantity = 0;

            for (var index = 0;
                index < PurchaseOrderDetailIds.Length;
                index++)
            {
                var detailId =
                    PurchaseOrderDetailIds[index];

                if (!submittedDetailIds.Add(detailId))
                {
                    TempData["ErrorMessage"] =
                        "Dữ liệu dòng hàng bị trùng.";

                    return RedirectToAction(
                        nameof(WarehouseManagement),
                        new { branchId = order.BranchId });
                }

                if (!orderDetailDictionary.TryGetValue(
                    detailId,
                    out var orderDetail))
                {
                    TempData["ErrorMessage"] =
                        "Có dòng hàng không thuộc đơn đặt hàng.";

                    return RedirectToAction(
                        nameof(WarehouseManagement),
                        new { branchId = order.BranchId });
                }

                var deliveredQuantity =
                    ParseSupplierInt(
                        GetArrayValue(
                            DeliveredQuantities,
                            index));

                var acceptedQuantity =
                    ParseSupplierInt(
                        GetArrayValue(
                            AcceptedQuantities,
                            index));

                /*
                * Cho phép nhập 0 để bỏ qua một dòng
                * trong lần giao hàng hiện tại.
                */
                if (deliveredQuantity == 0
                    && acceptedQuantity == 0)
                {
                    continue;
                }

                if (deliveredQuantity <= 0)
                {
                    TempData["ErrorMessage"] =
                        $"Dòng {index + 1}: "
                        + "Số lượng giao phải lớn hơn 0.";

                    return RedirectToAction(
                        nameof(WarehouseManagement),
                        new { branchId = order.BranchId });
                }

                if (acceptedQuantity < 0)
                {
                    TempData["ErrorMessage"] =
                        $"Dòng {index + 1}: "
                        + "Số lượng đạt không được nhỏ hơn 0.";

                    return RedirectToAction(
                        nameof(WarehouseManagement),
                        new { branchId = order.BranchId });
                }

                if (acceptedQuantity > deliveredQuantity)
                {
                    TempData["ErrorMessage"] =
                        $"Dòng {index + 1}: "
                        + "Số lượng đạt không được lớn hơn số lượng giao.";

                    return RedirectToAction(
                        nameof(WarehouseManagement),
                        new { branchId = order.BranchId });
                }

                var remainingQuantity =
                    orderDetail.Quantity
                    - orderDetail.AcceptedQuantity;

                if (remainingQuantity <= 0)
                {
                    TempData["ErrorMessage"] =
                        $"Dòng {index + 1}: "
                        + "Sản phẩm này đã được nhận đủ.";

                    return RedirectToAction(
                        nameof(WarehouseManagement),
                        new { branchId = order.BranchId });
                }

                if (deliveredQuantity > remainingQuantity)
                {
                    TempData["ErrorMessage"] =
                        $"Dòng {index + 1}: "
                        + $"Nhà cung cấp chỉ còn phải giao "
                        + $"{remainingQuantity} sản phẩm.";

                    return RedirectToAction(
                        nameof(WarehouseManagement),
                        new { branchId = order.BranchId });
                }

                var rejectedQuantity =
                    deliveredQuantity - acceptedQuantity;

                var acceptedWeight =
                    ParseSupplierDecimal(
                        GetArrayValue(
                            AcceptedWeights,
                            index));

                var acceptedCaratValue =
                    ParseSupplierDecimal(
                        GetArrayValue(
                            AcceptedDiamondCarats,
                            index));

                var acceptedCarat =
                    acceptedCaratValue > 0
                        ? acceptedCaratValue
                        : (decimal?)null;

                var acceptedCertificate =
                    NormalizeOrEmpty(
                        GetArrayValue(
                            AcceptedDiamondCertificates,
                            index));

                var rejectionReason =
                    NormalizeOrEmpty(
                        GetArrayValue(
                            RejectionReasons,
                            index));

                if (acceptedCertificate.Length > 120)
                {
                    TempData["ErrorMessage"] =
                        $"Dòng {index + 1}: "
                        + "Mã kiểm định không được vượt quá 120 ký tự.";

                    return RedirectToAction(
                        nameof(WarehouseManagement),
                        new { branchId = order.BranchId });
                }

                if (rejectionReason.Length > 500)
                {
                    TempData["ErrorMessage"] =
                        $"Dòng {index + 1}: "
                        + "Lý do không đạt không được vượt quá 500 ký tự.";

                    return RedirectToAction(
                        nameof(WarehouseManagement),
                        new { branchId = order.BranchId });
                }

                if (rejectedQuantity > 0
                    && string.IsNullOrWhiteSpace(rejectionReason))
                {
                    TempData["ErrorMessage"] =
                        $"Dòng {index + 1}: "
                        + "Vui lòng nhập lý do cho số lượng không đạt.";

                    return RedirectToAction(
                        nameof(WarehouseManagement),
                        new { branchId = order.BranchId });
                }

                if (acceptedQuantity > 0
                    && IsWeightRequiredPurchaseLine(
                        orderDetail.ProductLine)
                    && acceptedWeight <= 0)
                {
                    TempData["ErrorMessage"] =
                        $"Dòng {index + 1}: "
                        + "Vui lòng nhập trọng lượng của hàng đạt.";

                    return RedirectToAction(
                        nameof(WarehouseManagement),
                        new { branchId = order.BranchId });
                }

                if (acceptedQuantity > 0
                    && IsCaratRequiredPurchaseLine(
                        orderDetail.ProductLine)
                    && (!acceptedCarat.HasValue
                        || acceptedCarat.Value <= 0))
                {
                    TempData["ErrorMessage"] =
                        $"Dòng {index + 1}: "
                        + "Vui lòng nhập carat của hàng đạt.";

                    return RedirectToAction(
                        nameof(WarehouseManagement),
                        new { branchId = order.BranchId });
                }

                if (acceptedQuantity == 0
                    || !IsWeightRequiredPurchaseLine(
                        orderDetail.ProductLine))
                {
                    acceptedWeight = 0;
                }

                if (acceptedQuantity == 0
                    || !IsCaratRequiredPurchaseLine(
                        orderDetail.ProductLine))
                {
                    acceptedCarat = null;
                    acceptedCertificate = string.Empty;
                }

                var qualityStatus =
                    acceptedQuantity == 0
                        ? SupplierGoodsReceiptDetail.QualityFailed
                        : rejectedQuantity == 0
                            ? SupplierGoodsReceiptDetail.QualityPassed
                            : SupplierGoodsReceiptDetail
                                .QualityPartiallyPassed;

                var receiptDetail =
                    new SupplierGoodsReceiptDetail
                    {
                        SupplierPurchaseOrderDetailId =
                            orderDetail.Id,

                        ReceivedQuantity =
                            deliveredQuantity,

                        AcceptedQuantity =
                            acceptedQuantity,

                        RejectedQuantity =
                            rejectedQuantity,

                        ActualWeight =
                            acceptedWeight,

                        ActualDiamondCarat =
                            acceptedCarat,

                        ActualDiamondCertificate =
                            acceptedCertificate,

                        ActualUnitCost =
                            orderDetail.UnitCost,

                        LineValue =
                            acceptedQuantity
                            * orderDetail.UnitCost,

                        QualityStatus =
                            qualityStatus,

                        RejectionReason =
                            rejectedQuantity > 0
                                ? rejectionReason
                                : string.Empty,

                        Resolution =
                            SupplierGoodsReceiptDetail
                                .ResolutionNone
                    };

                receiptDetails.Add(receiptDetail);

                /*
                * ReceivedQuantity trên đơn được hiểu là
                * số lượng cửa hàng thực sự chấp nhận nhận.
                */
                orderDetail.ReceivedQuantity +=
                    acceptedQuantity;

                orderDetail.AcceptedQuantity +=
                    acceptedQuantity;

                orderDetail.RejectedQuantity +=
                    rejectedQuantity;

                totalDeliveredQuantity +=
                    deliveredQuantity;

                totalAcceptedQuantity +=
                    acceptedQuantity;

                totalRejectedQuantity +=
                    rejectedQuantity;
            }

            if (!receiptDetails.Any())
            {
                TempData["ErrorMessage"] =
                    "Vui lòng nhập số lượng giao cho ít nhất một dòng.";

                return RedirectToAction(
                    nameof(WarehouseManagement),
                    new { branchId = order.BranchId });
            }

            var receipt = new SupplierGoodsReceipt
            {
                ReceiptCode =
                    BuildSupplierGoodsReceiptCode(),

                SupplierPurchaseOrderId =
                    order.Id,

                WarehouseId =
                    warehouse.Id,

                CreatedByUserId =
                    currentUser.Id,

                ReceivedAt =
                    ReceivedAt.Value,

                /*
                * Có hàng đạt thì chờ nhập kho.
                * Không có hàng đạt thì lưu kết quả không đạt.
                */
                Status =
                    totalAcceptedQuantity > 0
                        ? SupplierGoodsReceipt.StatusPendingApproval
                        : SupplierGoodsReceipt.StatusRejected,

                DeliveryDocumentNumber =
                    DeliveryDocumentNumber,

                DeliveredBy =
                    DeliveredBy,

                TotalAcceptedValue =
                    receiptDetails.Sum(detail =>
                        detail.LineValue),

                Note =
                    ReceiptNote
            };

            foreach (var receiptDetail in receiptDetails)
            {
                receipt.Details.Add(receiptDetail);
            }

            var allItemsAccepted =
                order.Details.All(detail =>
                    detail.AcceptedQuantity
                    >= detail.Quantity);

            var hasAcceptedItems =
                order.Details.Any(detail =>
                    detail.AcceptedQuantity > 0);

            if (allItemsAccepted)
            {
                order.Status =
                    SupplierPurchaseOrder.StatusReceived;
            }
            else if (hasAcceptedItems)
            {
                order.Status =
                    SupplierPurchaseOrder
                        .StatusPartiallyReceived;
            }
            else
            {
                order.Status =
                    SupplierPurchaseOrder
                        .StatusAwaitingReplacement;
            }

            _context.SupplierGoodsReceipts.Add(receipt);

            await _context.SaveChangesAsync();

            if (totalAcceptedQuantity == 0)
            {
                TempData["SuccessMessage"] =
                    $"Đã lưu kết quả kiểm hàng {receipt.ReceiptCode}. "
                    + $"Toàn bộ {totalRejectedQuantity} sản phẩm không đạt; "
                    + "không có hàng nào được nhận.";
            }
            else
            {
                TempData["SuccessMessage"] =
                    $"Đã xác nhận nhận {totalAcceptedQuantity}/"
                    + $"{totalDeliveredQuantity} sản phẩm. "
                    + $"Phiếu {receipt.ReceiptCode} đang chờ nhập kho.";
            }

            return RedirectToAction(
                nameof(WarehouseManagement),
                new { branchId = order.BranchId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult>
            PostSupplierGoodsReceiptToInventory(
                int receiptId)
        {
            var receipt = await _context.SupplierGoodsReceipts
                .Include(item => item.Warehouse)
                .Include(item => item.SupplierPurchaseOrder)
                    .ThenInclude(order => order.Details)
                .Include(item => item.Details)
                    .ThenInclude(detail =>
                        detail.SupplierPurchaseOrderDetail)
                .FirstOrDefaultAsync(item =>
                    item.Id == receiptId);

            if (receipt == null)
            {
                TempData["ErrorMessage"] =
                    "Không tìm thấy phiếu nhập kho.";

                return RedirectToAction(
                    nameof(WarehouseManagement));
            }

            if (receipt.Status
                != SupplierGoodsReceipt.StatusPendingApproval)
            {
                TempData["ErrorMessage"] =
                    "Phiếu này không ở trạng thái Chờ nhập kho.";

                return RedirectToAction(
                    nameof(InventoryManagement),
                    new
                    {
                        branchId =
                            receipt.SupplierPurchaseOrder.BranchId,

                        inventoryTab = "items"
                    });
            }

            var currentUser =
                await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Forbid();
            }

            if (!User.IsInRole(RoleCatalog.Admin)
                && currentUser.BranchId
                    != receipt.SupplierPurchaseOrder.BranchId)
            {
                TempData["ErrorMessage"] =
                    "Bạn không có quyền nhập kho phiếu này.";

                return RedirectToAction(
                    nameof(WarehouseManagement),
                    new
                    {
                        branchId =
                            receipt.SupplierPurchaseOrder.BranchId
                    });
            }

            var acceptedDetails =
                receipt.Details
                    .Where(detail =>
                        detail.AcceptedQuantity > 0)
                    .ToList();

            if (!acceptedDetails.Any())
            {
                TempData["ErrorMessage"] =
                    "Phiếu không có hàng đạt để nhập kho.";

                return RedirectToAction(
                    nameof(WarehouseManagement),
                    new
                    {
                        branchId =
                            receipt.SupplierPurchaseOrder.BranchId
                    });
            }

            try
            {
                foreach (var detail in acceptedDetails)
                {
                    await _inventoryStockService
                        .PrepareSupplierReceiptEntryAsync(
                            receiptDetailId:
                                detail.Id,

                            warehouseId:
                                receipt.WarehouseId,

                            acceptedQuantity:
                                detail.AcceptedQuantity,

                            acceptedWeight:
                                detail.ActualWeight,

                            acceptedCarat:
                                detail.ActualDiamondCarat,

                            approvedUnitCost:
                                detail.ActualUnitCost,

                            createdByUserId:
                                currentUser.Id,

                            note:
                                $"Nhập kho từ phiếu {receipt.ReceiptCode}.");
                }

                var hasRejectedItems =
                    receipt.Details.Any(detail =>
                        detail.RejectedQuantity > 0);

                receipt.Status =
                    hasRejectedItems
                        ? SupplierGoodsReceipt
                            .StatusPartiallyApproved
                        : SupplierGoodsReceipt
                            .StatusApproved;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    $"Đã nhập {acceptedDetails.Sum(detail => detail.AcceptedQuantity)} "
                    + $"sản phẩm từ phiếu {receipt.ReceiptCode} vào kho.";

                return RedirectToAction(
                    nameof(WarehouseManagement),
                    new
                    {
                        branchId =
                            receipt.SupplierPurchaseOrder.BranchId
                    });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);

                TempData["ErrorMessage"] =
                    "Không thể nhập hàng vào tồn kho. "
                    + "Vui lòng kiểm tra lại dữ liệu phiếu.";

                return RedirectToAction(
                    nameof(WarehouseManagement),
                    new
                    {
                        branchId =
                            receipt.SupplierPurchaseOrder.BranchId
                    });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult> ApproveSupplierGoodsReceipt(
            int ReceiptId,
            Dictionary<int, string> QualityResults,
            Dictionary<int, string> RejectionReasons)
        {
            QualityResults ??= new Dictionary<int, string>();
            RejectionReasons ??= new Dictionary<int, string>();

            var receipt = await _context.SupplierGoodsReceipts
                .Include(item => item.Warehouse)
                .Include(item => item.SupplierPurchaseOrder)
                .ThenInclude(order => order.Details)
                .Include(item => item.Details)
                    .ThenInclude(detail =>
                        detail.SupplierPurchaseOrderDetail)
                .FirstOrDefaultAsync(item =>
                    item.Id == ReceiptId);

            if (receipt == null)
            {
                TempData["ErrorMessage"] =
                    "Không tìm thấy phiếu nhận hàng.";

                return RedirectToAction(
                    nameof(WarehouseManagement));
            }

            /*
            * Hỗ trợ cả trạng thái cũ nếu trước đó bạn đã thử
            * chức năng kiểm hàng hai bước.
            */
            var canApprove =
                receipt.Status
                    == SupplierGoodsReceipt.StatusPendingInspection
                || receipt.Status
                    == SupplierGoodsReceipt.StatusInspecting
                || receipt.Status
                    == SupplierGoodsReceipt.StatusPendingApproval;

            if (!canApprove)
            {
                TempData["ErrorMessage"] =
                    "Phiếu này đã được duyệt hoặc không còn ở trạng thái cho phép kiểm hàng.";

                return RedirectToAction(
                    nameof(WarehouseManagement));
            }

            if (receipt.Details == null
                || receipt.Details.Count == 0)
            {
                TempData["ErrorMessage"] =
                    "Phiếu nhận hàng không có dòng hàng.";

                return RedirectToAction(
                    nameof(WarehouseManagement));
            }

            var currentUser =
                await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Forbid();
            }

            /*
            * Admin được duyệt mọi chi nhánh.
            * Manager và Branch Owner chỉ duyệt chi nhánh của mình.
            */
            if (!User.IsInRole(RoleCatalog.Admin)
                && currentUser.BranchId
                    != receipt.SupplierPurchaseOrder.BranchId)
            {
                TempData["ErrorMessage"] =
                    "Bạn không có quyền duyệt phiếu nhận hàng của chi nhánh này.";

                return RedirectToAction(
                    nameof(WarehouseManagement));
            }

            /*
            * Kiểm tra toàn bộ dữ liệu trước.
            * Chưa tạo hàng kho tại vòng lặp này.
            */
            foreach (var detail in receipt.Details)
            {
                if (!QualityResults.TryGetValue(
                    detail.Id,
                    out var result))
                {
                    TempData["ErrorMessage"] =
                        $"Vui lòng chọn Đạt hoặc Không đạt cho dòng "
                        + $"{detail.SupplierPurchaseOrderDetail?.ProductName ?? detail.Id.ToString()}.";

                    return RedirectToAction(
                        nameof(WarehouseManagement));
                }

                result = NormalizeOrEmpty(result);

                if (result
                        != SupplierGoodsReceiptDetail.QualityPassed
                    && result
                        != SupplierGoodsReceiptDetail.QualityFailed)
                {
                    TempData["ErrorMessage"] =
                        "Kết quả kiểm hàng không hợp lệ.";

                    return RedirectToAction(
                        nameof(WarehouseManagement));
                }

                var rejectionReason =
                    RejectionReasons.TryGetValue(
                        detail.Id,
                        out var reason)
                        ? NormalizeOrEmpty(reason)
                        : string.Empty;

                if (result
                        == SupplierGoodsReceiptDetail.QualityFailed
                    && string.IsNullOrWhiteSpace(rejectionReason))
                {
                    TempData["ErrorMessage"] =
                        $"Vui lòng nhập lý do không đạt cho "
                        + $"{detail.SupplierPurchaseOrderDetail?.ProductName ?? "dòng hàng"}.";

                    return RedirectToAction(
                        nameof(WarehouseManagement));
                }

                if (rejectionReason.Length > 500)
                {
                    TempData["ErrorMessage"] =
                        "Lý do không đạt không được vượt quá 500 ký tự.";

                    return RedirectToAction(
                        nameof(WarehouseManagement));
                }

                if (result
                        == SupplierGoodsReceiptDetail.QualityPassed
                    && detail.ReceivedQuantity <= 0)
                {
                    TempData["ErrorMessage"] =
                        "Số lượng hàng đạt phải lớn hơn 0.";

                    return RedirectToAction(
                        nameof(WarehouseManagement));
                }
            }

            try
            {
                var totalAcceptedQuantity = 0;
                var totalRejectedQuantity = 0;

                foreach (var detail in receipt.Details)
                {
                    var result =
                        NormalizeOrEmpty(
                            QualityResults[detail.Id]);

                    var rejectionReason =
                        RejectionReasons.TryGetValue(
                            detail.Id,
                            out var reason)
                            ? NormalizeOrEmpty(reason)
                            : string.Empty;

                    var purchaseOrderDetail =
                        detail.SupplierPurchaseOrderDetail;

                    if (purchaseOrderDetail == null)
                    {
                        throw new InvalidOperationException(
                            "Chi tiết phiếu nhận không liên kết với chi tiết đơn đặt hàng.");
                    }

                    /*
                    * Trường hợp dòng hàng đạt:
                    * Toàn bộ số lượng của dòng được nhập kho.
                    */
                    if (result
                        == SupplierGoodsReceiptDetail.QualityPassed)
                    {
                        var resolvedUnitCost =
                            detail.ActualUnitCost > 0
                                ? detail.ActualUnitCost
                                : purchaseOrderDetail.UnitCost;

                        if (resolvedUnitCost <= 0)
                        {
                            throw new InvalidOperationException(
                                $"Đơn giá của {purchaseOrderDetail.ProductName} không hợp lệ.");
                        }

                        detail.AcceptedQuantity =
                            detail.ReceivedQuantity;

                        detail.RejectedQuantity = 0;

                        detail.ActualUnitCost =
                            resolvedUnitCost;

                        detail.LineValue =
                            detail.ReceivedQuantity
                            * resolvedUnitCost;

                        detail.QualityStatus =
                            SupplierGoodsReceiptDetail
                                .QualityPassed;

                        detail.RejectionReason =
                            string.Empty;

                        detail.Resolution =
                            SupplierGoodsReceiptDetail
                                .ResolutionNone;

                        /*
                        * Ghi số lượng đạt tổng hợp trên đơn đặt hàng.
                        */
                        purchaseOrderDetail.AcceptedQuantity +=
                            detail.ReceivedQuantity;

                        totalAcceptedQuantity +=
                            detail.ReceivedQuantity;

                        /*
                        * Chuẩn bị InventoryItem và InventoryTransaction.
                        * Chưa SaveChanges ở đây.
                        */
                        await _inventoryStockService
                            .PrepareSupplierReceiptEntryAsync(
                                receiptDetailId:
                                    detail.Id,

                                warehouseId:
                                    receipt.WarehouseId,

                                acceptedQuantity:
                                    detail.ReceivedQuantity,

                                acceptedWeight:
                                    detail.ActualWeight,

                                acceptedCarat:
                                    detail.ActualDiamondCarat,

                                approvedUnitCost:
                                    resolvedUnitCost,

                                createdByUserId:
                                    currentUser.Id,

                                note:
                                    $"Nhập kho từ phiếu {receipt.ReceiptCode}.");
                    }
                    /*
                    * Trường hợp không đạt:
                    * Không tạo InventoryItem và không tạo giao dịch kho.
                    */
                    else
                    {
                        detail.AcceptedQuantity = 0;

                        detail.RejectedQuantity =
                            detail.ReceivedQuantity;

                        detail.LineValue = 0;

                        detail.QualityStatus =
                            SupplierGoodsReceiptDetail
                                .QualityFailed;

                        detail.RejectionReason =
                            rejectionReason;

                        /*
                        * Tạm để Chưa xử lý.
                        * Sau này chức năng trả/đổi NCC sẽ cập nhật.
                        */
                        detail.Resolution =
                            SupplierGoodsReceiptDetail
                                .ResolutionNone;

                        purchaseOrderDetail.RejectedQuantity +=
                            detail.ReceivedQuantity;

                        /*
                        * Hàng không đạt không được tính là đã hoàn thành nhận hàng.
                        * Trả lại số lượng để có thể nhận hàng thay thế từ nhà cung cấp.
                        */
                        purchaseOrderDetail.ReceivedQuantity =
                            Math.Max(
                                0,
                                purchaseOrderDetail.ReceivedQuantity
                                - detail.ReceivedQuantity);

                        totalRejectedQuantity +=
                            detail.ReceivedQuantity;
                    }
                }

                var purchaseOrder =
                    receipt.SupplierPurchaseOrder;

                var allItemsReceived =
                    purchaseOrder.Details.All(detail =>
                        detail.ReceivedQuantity >= detail.Quantity);

                var hasReceivedItems =
                    purchaseOrder.Details.Any(detail =>
                        detail.ReceivedQuantity > 0);

                purchaseOrder.Status =
                    allItemsReceived
                        ? SupplierPurchaseOrder.StatusReceived
                        : hasReceivedItems
                            ? SupplierPurchaseOrder.StatusPartiallyReceived
                            : SupplierPurchaseOrder.StatusOrdered;
                            
                receipt.TotalAcceptedValue =
                    receipt.Details.Sum(
                        detail => detail.LineValue);

                /*
                * Hệ thống tự xác định trạng thái cuối.
                */
                if (totalAcceptedQuantity == 0)
                {
                    receipt.Status =
                        SupplierGoodsReceipt.StatusRejected;
                }
                else if (totalRejectedQuantity > 0)
                {
                    receipt.Status =
                        SupplierGoodsReceipt
                            .StatusPartiallyApproved;
                }
                else
                {
                    receipt.Status =
                        SupplierGoodsReceipt.StatusApproved;
                }

                /*
                * Chỉ SaveChanges một lần:
                * - Kết quả kiểm hàng
                * - InventoryItem
                * - InventoryTransaction
                * - Trạng thái phiếu
                *
                * EF Core tự thực hiện transaction cho lần lưu này.
                */
                await _context.SaveChangesAsync();

                if (receipt.Status
                    == SupplierGoodsReceipt.StatusRejected)
                {
                    TempData["SuccessMessage"] =
                        $"Phiếu {receipt.ReceiptCode} đã được duyệt là Không đạt. "
                        + "Không có hàng nào được nhập kho.";
                }
                else if (receipt.Status
                    == SupplierGoodsReceipt.StatusPartiallyApproved)
                {
                    TempData["SuccessMessage"] =
                        $"Đã duyệt phiếu {receipt.ReceiptCode}. "
                        + $"{totalAcceptedQuantity} sản phẩm đạt đã được nhập kho; "
                        + $"{totalRejectedQuantity} sản phẩm không đạt chưa được nhập kho.";
                }
                else
                {
                    TempData["SuccessMessage"] =
                        $"Đã duyệt phiếu {receipt.ReceiptCode}. "
                        + $"Toàn bộ {totalAcceptedQuantity} sản phẩm đã được nhập kho.";
                }

                return RedirectToAction(
                    nameof(WarehouseManagement));
            }
            catch (Exception exception)
            {
                var errorMessage =
                    exception.GetBaseException().Message;

                Console.WriteLine(
                    "====================================");

                Console.WriteLine(
                    "LỖI DUYỆT PHIẾU NHẬN HÀNG");

                Console.WriteLine(errorMessage);
                Console.WriteLine(exception);

                Console.WriteLine(
                    "====================================");

                TempData["ErrorMessage"] =
                    "Không thể duyệt phiếu nhận hàng. Chi tiết lỗi: "
                    + errorMessage;

                return RedirectToAction(
                    nameof(WarehouseManagement));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult> UpdateSupplier(
            int SupplierId,
            string Name,
            string TaxCode,
            string ContactPerson,
            string Phone,
            string Email,
            string Address,
            string[] SupplierTypes,
            int PaymentTermDays,
            string BankName,
            string BankAccountNumber,
            string BankAccountName,
            string Note)
        {
            var supplier = await _context.Suppliers.FindAsync(SupplierId);

            if (supplier == null)
            {
                return NotFound();
            }

            Name = NormalizeOrEmpty(Name);
            TaxCode = KeepDigitsOnly(TaxCode);
            ContactPerson = NormalizeOrEmpty(ContactPerson);
            Phone = KeepDigitsOnly(Phone);
            Email = NormalizeOrEmpty(Email);
            Address = NormalizeOrEmpty(Address);
            BankName = NormalizeOrEmpty(BankName);
            BankAccountNumber = KeepDigitsOnly(BankAccountNumber);
            BankAccountName = NormalizeOrEmpty(BankAccountName);
            Note = NormalizeOrEmpty(Note);

            var supplierTypeText = BuildSupplierTypeText(SupplierTypes);

            var validationError = await ValidateSupplierInputAsync(
                SupplierId,
                Name,
                TaxCode,
                ContactPerson,
                Phone,
                Email,
                Address,
                supplierTypeText,
                PaymentTermDays,
                BankName,
                BankAccountNumber,
                BankAccountName);

            if (!string.IsNullOrWhiteSpace(validationError))
            {
                TempData["ErrorMessage"] = validationError;
                return RedirectToAction(nameof(SupplierManagement));
            }

            supplier.Name = Name;
            supplier.TaxCode = TaxCode;
            supplier.ContactPerson = ContactPerson;
            supplier.Phone = Phone;
            supplier.Email = Email;
            supplier.Address = Address;
            supplier.SupplierType = supplierTypeText;
            supplier.PaymentTermDays = PaymentTermDays;
            supplier.BankName = BankName;
            supplier.BankAccountNumber = BankAccountNumber;
            supplier.BankAccountName = BankAccountName;
            supplier.Note = Note;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã cập nhật nhà cung cấp {supplier.Name}.";
            return RedirectToAction(nameof(SupplierManagement));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult> ToggleSupplierStatus(int supplierId)
        {
            var supplier = await _context.Suppliers
                .Include(item => item.PurchaseOrders)
                    .ThenInclude(order => order.Receipts)
                .Include(item => item.PurchaseOrders)
                    .ThenInclude(order => order.Payments)
                .FirstOrDefaultAsync(item => item.Id == supplierId);

            if (supplier == null)
            {
                return NotFound();
            }

            if (supplier.IsActive)
            {
                var hasOpenOrder = supplier.PurchaseOrders.Any(order =>
                    order.Status != SupplierPurchaseOrder.StatusReceived
                    && order.Status != SupplierPurchaseOrder.StatusCancelled);

                if (hasOpenOrder)
                {
                    TempData["ErrorMessage"] = "Không thể tạm ngưng nhà cung cấp đang còn đơn đặt hàng chưa xử lý xong.";
                    return RedirectToAction(nameof(SupplierManagement));
                }

                var debt = supplier.PurchaseOrders.Sum(CalculateSupplierOrderDebt);

                if (debt > 0)
                {
                    TempData["ErrorMessage"] = "Không thể tạm ngưng nhà cung cấp đang còn công nợ.";
                    return RedirectToAction(nameof(SupplierManagement));
                }
            }

            supplier.IsActive = !supplier.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = supplier.IsActive
                ? $"Đã kích hoạt lại nhà cung cấp {supplier.Name}."
                : $"Đã tạm ngưng nhà cung cấp {supplier.Name}.";

            return RedirectToAction(nameof(SupplierManagement));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult> ToggleSupplierStatusAjax(int supplierId)
        {
            var supplier = await _context.Suppliers
                .Include(item => item.PurchaseOrders)
                    .ThenInclude(order => order.Receipts)
                .Include(item => item.PurchaseOrders)
                    .ThenInclude(order => order.Payments)
                .FirstOrDefaultAsync(item => item.Id == supplierId);

            if (supplier == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Kh\u00f4ng t\u00ecm th\u1ea5y nh\u00e0 cung c\u1ea5p."
                });
            }

            if (supplier.IsActive)
            {
                var hasOpenOrder = supplier.PurchaseOrders.Any(order =>
                    order.Status != SupplierPurchaseOrder.StatusReceived
                    && order.Status != SupplierPurchaseOrder.StatusCancelled);

                if (hasOpenOrder)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Kh\u00f4ng th\u1ec3 t\u1ea1m ng\u01b0ng nh\u00e0 cung c\u1ea5p \u0111ang c\u00f2n \u0111\u01a1n \u0111\u1eb7t h\u00e0ng ch\u01b0a x\u1eed l\u00fd xong."
                    });
                }

                var debt = supplier.PurchaseOrders.Sum(CalculateSupplierOrderDebt);

                if (debt > 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Kh\u00f4ng th\u1ec3 t\u1ea1m ng\u01b0ng nh\u00e0 cung c\u1ea5p \u0111ang c\u00f2n c\u00f4ng n\u1ee3."
                    });
                }
            }

            supplier.IsActive = !supplier.IsActive;

            await _context.SaveChangesAsync();

            var isActive = supplier.IsActive;
            var message = isActive
                ? $"\u0110\u00e3 k\u00edch ho\u1ea1t l\u1ea1i nh\u00e0 cung c\u1ea5p {supplier.Name}."
                : $"\u0110\u00e3 t\u1ea1m ng\u01b0ng nh\u00e0 cung c\u1ea5p {supplier.Name}.";

            return Json(new
            {
                success = true,
                message,
                supplierId = supplier.Id,
                isActive,
                status = isActive ? "active" : "inactive",
                statusText = isActive ? "\u0110ang h\u1ee3p t\u00e1c" : "T\u1ea1m ng\u01b0ng",
                buttonText = isActive ? "T\u1ea1m ng\u01b0ng" : "K\u00edch ho\u1ea1t",
                buttonClass = isActive ? "admin-button admin-button--danger" : "admin-button admin-button--secondary",
                statusChipClass = isActive ? "status-chip status-chip--success" : "status-chip status-chip--neutral"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult> DeleteSupplier(int supplierId)
        {
            var supplier = await _context.Suppliers
                .Include(item => item.PurchaseOrders)
                .Include(item => item.Payments)
                .FirstOrDefaultAsync(item => item.Id == supplierId);

            if (supplier == null)
            {
                return NotFound();
            }

            var hasTransactions =
                supplier.PurchaseOrders.Any()
                || supplier.Payments.Any();

            if (hasTransactions)
            {
                TempData["ErrorMessage"] = "Không thể xóa nhà cung cấp đã phát sinh giao dịch. Bạn chỉ nên dùng chức năng tạm ngưng để giữ lịch sử chứng từ.";
                return RedirectToAction(nameof(SupplierManagement));
            }

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã xóa nhà cung cấp {supplier.Name}.";
            return RedirectToAction(nameof(SupplierManagement));
        }

        [Authorize(Roles = RoleCatalog.Admin)]
        public async Task<IActionResult> BranchManagement()
        {
            return View(await BuildBranchManagementViewModelAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleCatalog.Admin)]
        public async Task<IActionResult> CreateBranch(BranchManagementViewModel model)
        {
            model.BranchName = NormalizeOrEmpty(model.BranchName);
            model.Address = NormalizeOrEmpty(model.Address);
            model.ManagerUserId = NormalizeOrEmpty(model.ManagerUserId);

            var hasExistingBranch = await _context.Branches.AnyAsync(branch =>
                branch.BranchName.ToLower() == model.BranchName.ToLower());

            if (hasExistingBranch)
            {
                ModelState.AddModelError(nameof(model.BranchName), "Tên chi nhánh này đã tồn tại.");
            }

            var selectedWarehouse = model.WarehouseId.HasValue
                ? await _context.Warehouses.FirstOrDefaultAsync(warehouse =>
                    warehouse.Id == model.WarehouseId.Value && warehouse.IsActive)
                : null;
            if (selectedWarehouse == null)
                ModelState.AddModelError(nameof(model.WarehouseId), "Vui lòng chọn một kho đang hoạt động.");

            var selectedManager = string.IsNullOrWhiteSpace(model.ManagerUserId)
                ? null
                : await _userManager.FindByIdAsync(model.ManagerUserId);
            if (selectedManager == null
                || !selectedManager.IsActive
                || !(await _userManager.GetRolesAsync(selectedManager)).Contains(RoleCatalog.Manager))
                ModelState.AddModelError(nameof(model.ManagerUserId), "Chủ quản lí chi nhánh không hợp lệ hoặc đã bị khóa.");

            if (!ModelState.IsValid)
            {
                var viewModel = await BuildBranchManagementViewModelAsync(model);
                return View(nameof(BranchManagement), viewModel);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var createdBranch = new Branch
                {
                    BranchName = model.BranchName,
                    Address = model.Address,
                    PhoneNumber = string.Empty,
                    IsActive = true
                };
                _context.Branches.Add(createdBranch);
                await _context.SaveChangesAsync();

                _context.BranchWarehouseAccesses.Add(new BranchWarehouseAccess
                {
                    BranchId = createdBranch.Id,
                    WarehouseId = selectedWarehouse.Id,
                    IsPrimary = true
                });

                selectedManager.BranchId = createdBranch.Id;
                var managerUpdate = await _userManager.UpdateAsync(selectedManager);
                if (!managerUpdate.Succeeded)
                    throw new InvalidOperationException(string.Join(" ", managerUpdate.Errors.Select(error => error.Description)));

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = $"Đã thêm chi nhánh {model.BranchName} và gán kho {selectedWarehouse.Name}.";
                return RedirectToAction(nameof(BranchManagement));
            }
            catch
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Không thể tạo chi nhánh. Vui lòng kiểm tra dữ liệu và thử lại.");
                return View(nameof(BranchManagement), await BuildBranchManagementViewModelAsync(model));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleCatalog.Admin)]
        public async Task<IActionResult> ToggleBranchStatus(int branchId)
        {
            var branch = await _context.Branches.FindAsync(branchId);
            if (branch == null)
            {
                return NotFound();
            }

            branch.IsActive = !branch.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = branch.IsActive
                ? $"Đã kích hoạt lại chi nhánh {branch.BranchName}."
                : $"Đã tạm khóa chi nhánh {branch.BranchName}.";

            return RedirectToAction(nameof(BranchManagement));
        }

        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult> BranchTeam(int branchId)
        {
            var actor = await GetCurrentActorAsync();
            var branch = await _context.Branches.FirstOrDefaultAsync(item => item.Id == branchId);
            if (branch == null)
            {
                return NotFound();
            }

            var manageableRoles = GetManageableBranchRoles(actor, branchId);
            if (manageableRoles.Count == 0)
            {
                return Forbid();
            }

            return View(await BuildBranchTeamViewModelAsync(branch, actor, manageableRoles));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult> AddExistingMember(int branchId, BranchTeamViewModel model)
        {
            var actor = await GetCurrentActorAsync();
            var branch = await _context.Branches.FirstOrDefaultAsync(item => item.Id == branchId);
            if (branch == null)
            {
                return NotFound();
            }

            var manageableRoles = GetManageableBranchRoles(actor, branchId);
            if (manageableRoles.Count == 0)
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(model.ExistingUserId))
            {
                TempData["ErrorMessage"] = "Vui lòng chọn tài khoản cần thêm vào chi nhánh.";
                return RedirectToAction(nameof(BranchTeam), new { branchId });
            }

            var targetUser = await _userManager.FindByIdAsync(model.ExistingUserId);
            if (targetUser == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài khoản được chọn.";
                return RedirectToAction(nameof(BranchTeam), new { branchId });
            }

            var targetRoles = await _userManager.GetRolesAsync(targetUser);
            var highestRole = RoleCatalog.GetHighestRole(targetRoles);
            if (!manageableRoles.Contains(highestRole))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thêm tài khoản này vào chi nhánh.";
                return RedirectToAction(nameof(BranchTeam), new { branchId });
            }

            targetUser.BranchId = branchId;
            var updateResult = await _userManager.UpdateAsync(targetUser);
            if (!updateResult.Succeeded)
            {
                TempData["ErrorMessage"] = BuildIdentityErrorMessage(updateResult, "Không thể thêm tài khoản vào chi nhánh.");
                return RedirectToAction(nameof(BranchTeam), new { branchId });
            }

            TempData["SuccessMessage"] = $"Đã thêm {targetUser.FullName} vào chi nhánh {branch.BranchName}.";
            return RedirectToAction(nameof(BranchTeam), new { branchId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult> CreateBranchMember(int branchId, BranchTeamViewModel model)
        {
            model.NewMemberFullName = NormalizeOrEmpty(model.NewMemberFullName);
            model.NewMemberEmail = NormalizeOrEmpty(model.NewMemberEmail);
            model.NewMemberRole = NormalizeOrEmpty(model.NewMemberRole);

            var actor = await GetCurrentActorAsync();
            var branch = await _context.Branches.FirstOrDefaultAsync(item => item.Id == branchId);
            if (branch == null)
            {
                return NotFound();
            }

            var manageableRoles = GetManageableBranchRoles(actor, branchId);
            if (manageableRoles.Count == 0)
            {
                return Forbid();
            }

            if (!manageableRoles.Contains(model.NewMemberRole))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền tạo vai trò này cho chi nhánh.";
                return RedirectToAction(nameof(BranchTeam), new { branchId });
            }

            if (string.IsNullOrWhiteSpace(model.NewMemberFullName)
                || string.IsNullOrWhiteSpace(model.NewMemberEmail)
                || string.IsNullOrWhiteSpace(model.NewMemberPassword))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ họ tên, email và mật khẩu khởi tạo.";
                return RedirectToAction(nameof(BranchTeam), new { branchId });
            }

            if (await _userManager.FindByEmailAsync(model.NewMemberEmail) != null)
            {
                TempData["ErrorMessage"] = "Email này đã tồn tại trong hệ thống.";
                return RedirectToAction(nameof(BranchTeam), new { branchId });
            }

            var user = new AppUser
            {
                UserName = model.NewMemberEmail,
                Email = model.NewMemberEmail,
                FullName = model.NewMemberFullName,
                BranchId = branchId,
                IsActive = true,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user, model.NewMemberPassword);
            if (!createResult.Succeeded)
            {
                TempData["ErrorMessage"] = BuildIdentityErrorMessage(createResult, "Không thể tạo tài khoản mới cho chi nhánh.");
                return RedirectToAction(nameof(BranchTeam), new { branchId });
            }

            var roleResult = await _userManager.AddToRoleAsync(user, model.NewMemberRole);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                TempData["ErrorMessage"] = BuildIdentityErrorMessage(roleResult, "Không thể gán vai trò cho tài khoản mới.");
                return RedirectToAction(nameof(BranchTeam), new { branchId });
            }

            TempData["SuccessMessage"] = $"Đã tạo tài khoản {user.Email} cho chi nhánh {branch.BranchName}.";
            return RedirectToAction(nameof(BranchTeam), new { branchId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult> RemoveBranchMember(int branchId, string userId)
        {
            var actor = await GetCurrentActorAsync();
            var branch = await _context.Branches.FirstOrDefaultAsync(item => item.Id == branchId);
            if (branch == null)
            {
                return NotFound();
            }

            var manageableRoles = GetManageableBranchRoles(actor, branchId);
            if (manageableRoles.Count == 0)
            {
                return Forbid();
            }

            var targetUser = await _userManager.FindByIdAsync(userId);
            if (targetUser == null || targetUser.BranchId != branchId)
            {
                TempData["ErrorMessage"] = "Không tìm thấy nhân sự thuộc chi nhánh này.";
                return RedirectToAction(nameof(BranchTeam), new { branchId });
            }

            if (!actor.IsAdmin && string.Equals(targetUser.Id, actor.User?.Id, StringComparison.Ordinal))
            {
                TempData["ErrorMessage"] = "Bạn không thể tự xóa chính mình khỏi chi nhánh hiện tại.";
                return RedirectToAction(nameof(BranchTeam), new { branchId });
            }

            var targetRoles = await _userManager.GetRolesAsync(targetUser);
            var highestRole = RoleCatalog.GetHighestRole(targetRoles);
            if (!manageableRoles.Contains(highestRole))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xóa tài khoản này khỏi chi nhánh.";
                return RedirectToAction(nameof(BranchTeam), new { branchId });
            }

            targetUser.BranchId = null;
            var updateResult = await _userManager.UpdateAsync(targetUser);
            if (!updateResult.Succeeded)
            {
                TempData["ErrorMessage"] = BuildIdentityErrorMessage(updateResult, "Không thể xóa tài khoản khỏi chi nhánh.");
                return RedirectToAction(nameof(BranchTeam), new { branchId });
            }

            TempData["SuccessMessage"] = $"Đã gỡ {targetUser.FullName} khỏi chi nhánh {branch.BranchName}.";
            return RedirectToAction(nameof(BranchTeam), new { branchId });
        }

        // 2. Quản lý Đơn hàng
        public async Task<IActionResult> OrderManagement()
        {
            await CancelExpiredDepositOrdersAsync();
            var actor = await GetCurrentActorAsync();
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Branch)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.PendingConfirmationCount = orders.Count(order =>
                order.Status == Order.StatusPendingConfirmation && CanConfirmOrder(actor, order));
            ViewBag.ActorRoles = actor.Roles;
            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string newStatus, string cancelReason = null)
        {
            await CancelExpiredDepositOrdersAsync();

            var order = await _context.Orders
                .Include(item => item.OrderDetails)
                    .ThenInclude(detail => detail.Product)
                .FirstOrDefaultAsync(item => item.Id == orderId);
            if (order == null) return NotFound();

            var actor = await GetCurrentActorAsync();
            var allowedStatuses = new[]
            {
                Order.StatusPendingConfirmation, // Allow manual cash deposit confirmation
                Order.StatusConfirmed,
                Order.StatusProcessing,
                Order.StatusShipping,
                Order.StatusCompleted,
                Order.StatusCancelled
            };

            if (!allowedStatuses.Contains(newStatus))
            {
                TempData["ErrorMessage"] = "Trạng thái đơn hàng không hợp lệ.";
                return RedirectToAction(nameof(OrderManagement));
            }

            if (newStatus == Order.StatusPendingConfirmation)
            {
                if (order.Status != Order.StatusUnpaidDeposit && order.Status != Order.StatusAwaitingDepositPayment)
                {
                    TempData["ErrorMessage"] = "Chỉ đơn hàng chưa thanh toán cọc mới có thể xác nhận đã nhận cọc.";
                    return RedirectToAction(nameof(OrderManagement));
                }
            }

            if (newStatus == Order.StatusConfirmed && !CanConfirmOrder(actor, order))
            {
                TempData["ErrorMessage"] = order.TotalAmount > 50_000_000m
                    ? "Đơn trên 50 triệu chỉ quản lí hoặc admin được xác nhận."
                    : "Bạn không có quyền xác nhận đơn hàng này.";
                return RedirectToAction(nameof(OrderManagement));
            }

            if (newStatus == Order.StatusConfirmed && order.Status != Order.StatusPendingConfirmation)
            {
                TempData["ErrorMessage"] = "Chỉ đơn đã thanh toán cọc và đang chờ xác nhận mới được xác nhận.";
                return RedirectToAction(nameof(OrderManagement));
            }

            order.Status = newStatus;
            if (newStatus == Order.StatusPendingConfirmation)
            {
                order.DepositPaidAt = DateTime.UtcNow;
            }

            if (newStatus == Order.StatusConfirmed)
            {
                order.ConfirmedAt = DateTime.UtcNow;
            }

            if (newStatus == Order.StatusCancelled)
            {
                order.CancelReason = !string.IsNullOrWhiteSpace(cancelReason) 
                    ? cancelReason.Trim() 
                    : "Đơn hàng bị hủy bởi nhân sự xử lý.";
                RestoreProductsForCancelledOrder(order);
            }

            await _context.SaveChangesAsync();

            // Trigger notifications
            if (newStatus == Order.StatusPendingConfirmation)
            {
                await NotifyAuthorizedStaffForPendingOrderAsync(order);
            }
            else if (newStatus == Order.StatusConfirmed)
            {
                try
                {
                    var customer = await _userManager.FindByIdAsync(order.UserId);
                    var destination = customer != null && !string.IsNullOrWhiteSpace(customer.Email)
                        ? customer.Email
                        : order.CustomerPhone;

                    if (!string.IsNullOrWhiteSpace(destination))
                    {
                        await _notificationService.SendOrderConfirmedNotificationAsync(
                            destination,
                            order.CustomerName ?? customer?.FullName ?? "Quý khách",
                            order.OrderNumber);
                    }
                }
                catch (Exception)
                {
                    // Ignore exceptions for safety
                }
            }
            else if (newStatus == Order.StatusCancelled)
            {
                try
                {
                    var customer = await _userManager.FindByIdAsync(order.UserId);
                    var destination = customer != null && !string.IsNullOrWhiteSpace(customer.Email)
                        ? customer.Email
                        : order.CustomerPhone;

                    if (!string.IsNullOrWhiteSpace(destination))
                    {
                        await _notificationService.SendOrderRejectedNotificationAsync(
                            destination,
                            order.CustomerName ?? customer?.FullName ?? "Quý khách",
                            order.OrderNumber,
                            order.CancelReason);
                    }
                }
                catch (Exception)
                {
                    // Ignore exceptions for safety
                }
            }

            TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn hàng #{order.OrderNumber} thành {newStatus}.";
            return RedirectToAction(nameof(OrderManagement));
        }

        // 3. Quản lý Người dùng
        [Authorize(Roles = RoleCatalog.Admin)]
        public async Task<IActionResult> UserManagement()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    Roles = roles
                });
            }

            // Gửi danh sách Role hệ thống xuống View để lọc hoặc cấp
            ViewBag.AllRoles = (await _roleManager.Roles.Select(r => r.Name).ToListAsync())
                .OrderByDescending(RoleCatalog.GetPriority)
                .ToList();
            return View(userViewModels);
        }

        [HttpPost]
        [Authorize(Roles = RoleCatalog.Admin)]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            var targetUser = await _userManager.FindByIdAsync(userId);
            if (targetUser == null) return NotFound();

            var targetRoles = await _userManager.GetRolesAsync(targetUser);
            var actor = await GetCurrentActorAsync();

            // Logic khóa
            if (targetUser.IsActive
                && string.Equals(targetUser.Id, actor.User?.Id, StringComparison.Ordinal)
                && (actor.IsAdmin || actor.IsManager))
            {
                TempData["ErrorMessage"] = "Admin và quản lí không thể tự khóa chính tài khoản của mình.";
                return RedirectToAction(nameof(UserManagement));
            }

            if (targetRoles.Contains(RoleCatalog.Admin) && !actor.IsAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền khóa tài khoản Admin.";
                return RedirectToAction(nameof(UserManagement));
            }

            if ((targetRoles.Contains(RoleCatalog.BranchOwner) || targetRoles.Contains(RoleCatalog.Manager))
                && actor.IsManager
                && !actor.IsAdmin)
            {
                TempData["ErrorMessage"] = "Quản lý (Manager) không thể khóa tài khoản đồng cấp hoặc cấp trên.";
                return RedirectToAction(nameof(UserManagement));
            }

            // Toggle logic
            targetUser.IsActive = !targetUser.IsActive;
            var updateResult = await _userManager.UpdateAsync(targetUser);
            if (!updateResult.Succeeded)
            {
                TempData["ErrorMessage"] = BuildIdentityErrorMessage(updateResult, "Không thể cập nhật trạng thái tài khoản.");
                return RedirectToAction(nameof(UserManagement));
            }

            TempData["SuccessMessage"] = targetUser.IsActive
                ? $"Đã mở khóa tài khoản {targetUser.Email}"
                : $"Đã khóa tài khoản {targetUser.Email}. Tài khoản này sẽ tự bị đăng xuất ở lần tải trang tiếp theo.";
            return RedirectToAction(nameof(UserManagement));
        }
        
        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult> InventoryIssues(
            string searchTerm = null,
            int? branchId = null,
            int? warehouseId = null,
            string issueType = null,
            string status = null)
        {
            searchTerm = NormalizeOrEmpty(searchTerm);
            issueType = NormalizeOrEmpty(issueType);
            status = NormalizeOrEmpty(status);

            var currentUser =
                await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Forbid();
            }

            var isAdmin =
                User.IsInRole(RoleCatalog.Admin);

            /*
            * Admin xem chi nhánh đang chọn.
            * Vai trò khác chỉ xem chi nhánh của tài khoản.
            */
            int? scopedBranchId =
                isAdmin
                    ? branchId
                    : currentUser.BranchId;

            if (!isAdmin)
            {
                branchId = scopedBranchId;
            }

            var warehouseQuery =
                _context.Warehouses
                    .AsNoTracking()
                    .Include(warehouse => warehouse.Branch)
                    .Where(warehouse =>
                            warehouse.IsActive
                            && warehouse.Branch.IsActive
                            && warehouse.LocationType
                                == Warehouse.LocationTypeStorage)
                    .AsQueryable();

            List<int> accessibleWarehouseIds;

            if (scopedBranchId.HasValue)
            {
                /*
                * Bao gồm kho thuộc chi nhánh và kho được
                * cấp quyền sử dụng chung cho chi nhánh.
                */
                accessibleWarehouseIds =
                    await _context.BranchWarehouseAccesses
                        .AsNoTracking()
                        .Where(access =>
                            access.BranchId
                            == scopedBranchId.Value)
                        .Select(access =>
                            access.WarehouseId)
                        .Concat(
                            _context.Warehouses
                                .Where(warehouse =>
                                    warehouse.BranchId
                                    == scopedBranchId.Value)
                                .Select(warehouse =>
                                    warehouse.Id))
                        .Distinct()
                        .ToListAsync();

                warehouseQuery =
                    warehouseQuery.Where(warehouse =>
                        accessibleWarehouseIds.Contains(
                            warehouse.Id));
            }
            else if (isAdmin)
            {
                accessibleWarehouseIds =
                    await warehouseQuery
                        .Select(warehouse => warehouse.Id)
                        .ToListAsync();
            }
            else
            {
                accessibleWarehouseIds =
                    new List<int>();

                warehouseQuery =
                    warehouseQuery.Where(warehouse => false);

                ViewBag.InventoryScopeWarning =
                    "Tài khoản chưa được gán chi nhánh nên không thể xem phiếu xuất kho.";
            }

            var accessibleWarehouses =
                await warehouseQuery
                    .OrderBy(warehouse =>
                        warehouse.Branch.BranchName)
                    .ThenBy(warehouse =>
                        warehouse.Name)
                    .ToListAsync();

            var issueQuery =
                _context.InventoryIssues
                    .AsNoTracking()
                    .Include(issue => issue.Branch)
                    .Include(issue => issue.Warehouse)
                    .Include(issue => issue.DestinationWarehouse)
                    .Include(issue => issue.ReceiverUser)
                    .Include(issue => issue.Supplier)
                    .Include(issue => issue.CreatedByUser)
                    .Include(issue => issue.ConfirmedByUser)
                    .Include(issue => issue.Details)
                        .ThenInclude(detail =>
                            detail.InventoryItem)
                    .AsQueryable();

            /*
            * Phạm vi phiếu được tính theo chi nhánh
            * thực hiện nghiệp vụ.
            */
            if (scopedBranchId.HasValue)
            {
                issueQuery =
                    issueQuery.Where(issue =>
                        issue.BranchId
                        == scopedBranchId.Value);
            }
            else if (!isAdmin)
            {
                issueQuery =
                    issueQuery.Where(issue => false);
            }

            /*
            * Thống kê trước khi áp dụng các bộ lọc giao diện.
            */
            var totalIssues =
                await issueQuery.CountAsync();

            var pendingIssues =
                await issueQuery.CountAsync(issue =>
                    issue.Status
                    == InventoryIssue.StatusPending);

            var issuedIssues =
                await issueQuery.CountAsync(issue =>
                    issue.Status
                    == InventoryIssue.StatusIssued);

            var totalIssuedQuantity =
                await issueQuery
                    .Where(issue =>
                        issue.Status
                        == InventoryIssue.StatusIssued)
                    .SelectMany(issue => issue.Details)
                    .SumAsync(detail =>
                        (int?)detail.Quantity)
                ?? 0;

            /*
            * Áp dụng bộ lọc.
            */
            var filteredIssueQuery =
                issueQuery;

            if (warehouseId.HasValue)
            {
                filteredIssueQuery =
                    filteredIssueQuery.Where(issue =>
                        issue.WarehouseId
                        == warehouseId.Value);
            }

            if (!string.IsNullOrWhiteSpace(issueType))
            {
                filteredIssueQuery =
                    filteredIssueQuery.Where(issue =>
                        issue.IssueType == issueType);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                filteredIssueQuery =
                    filteredIssueQuery.Where(issue =>
                        issue.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                filteredIssueQuery =
                    filteredIssueQuery.Where(issue =>
                        issue.IssueCode.Contains(searchTerm)
                        || (
                            issue.ReceiverUser != null
                            && issue.ReceiverUser.FullName.Contains(
                                searchTerm)
                        )
                        || (
                            issue.DestinationWarehouse != null
                            && issue.DestinationWarehouse.Name.Contains(
                                searchTerm)
                        )
                        || (
                            issue.ReferenceCode != null
                            && issue.ReferenceCode.Contains(
                                searchTerm)
                        )
                        || issue.Warehouse.Name.Contains(
                            searchTerm)
                        || (
                            issue.Supplier != null
                            && issue.Supplier.Name.Contains(
                                searchTerm)
                        ));
            }

            var issues =
                await filteredIssueQuery
                    .OrderByDescending(issue =>
                        issue.CreatedAt)
                    .ThenByDescending(issue =>
                        issue.Id)
                    .Take(300)
                    .ToListAsync();

            /*
            * Hàng có thể chọn để xuất.
            * Chỉ lấy hàng sẵn sàng và còn số lượng.
            */
            var availableItems =
                await _context.InventoryItems
                    .AsNoTracking()
                    .Include(item => item.Warehouse)
                        .ThenInclude(warehouse =>
                            warehouse.Branch)
                    .Include(item => item.Supplier)
                    .Where(item =>
                        accessibleWarehouseIds.Contains(
                            item.WarehouseId)
                        && item.QuantityOnHand > 0
                        && item.Status
                            == InventoryItem.StatusAvailable)
                    .OrderBy(item =>
                        item.Warehouse.Name)
                    .ThenBy(item =>
                        item.ProductName)
                    .ToListAsync();

            var branchQuery =
                _context.Branches
                    .AsNoTracking()
                    .Where(branch => branch.IsActive)
                    .AsQueryable();

            if (!isAdmin)
            {
                if (scopedBranchId.HasValue)
                {
                    branchQuery =
                        branchQuery.Where(branch =>
                            branch.Id
                            == scopedBranchId.Value);
                }
                else
                {
                    branchQuery =
                        branchQuery.Where(branch => false);
                }
            }

            var branches =
                await branchQuery
                    .OrderBy(branch =>
                        branch.BranchName)
                    .ToListAsync();

            var model =
                new InventoryIssueManagementViewModel
                {
                    SearchTerm = searchTerm,
                    BranchId = branchId,
                    WarehouseId = warehouseId,
                    IssueType = issueType,
                    Status = status,

                    TotalIssues = totalIssues,
                    PendingIssues = pendingIssues,
                    IssuedIssues = issuedIssues,
                    TotalIssuedQuantity =
                        totalIssuedQuantity,

                    Issues = issues,
                    AvailableItems =
                        availableItems,

                    BranchOptions =
                        new[]
                        {
                            new SelectListItem
                            {
                                Value = string.Empty,
                                Text = "-- Tất cả chi nhánh --"
                            }
                        }
                        .Concat(
                            branches.Select(branch =>
                                new SelectListItem
                                {
                                    Value =
                                        branch.Id.ToString(),

                                    Text =
                                        branch.BranchName,

                                    Selected =
                                        branchId == branch.Id
                                }))
                        .ToList(),

                    WarehouseOptions =
                        new[]
                        {
                            new SelectListItem
                            {
                                Value = string.Empty,
                                Text = "-- Tất cả kho --"
                            }
                        }
                        .Concat(
                            accessibleWarehouses.Select(
                                warehouse =>
                                    new SelectListItem
                                    {
                                        Value =
                                            warehouse.Id
                                                .ToString(),

                                        Text =
                                            $"{warehouse.Code} - {warehouse.Name}",

                                        Selected =
                                            warehouseId
                                            == warehouse.Id
                                    }))
                        .ToList(),

                    IssueTypeOptions =
                        new[]
                        {
                            new SelectListItem
                            {
                                Value = string.Empty,
                                Text = "-- Tất cả loại xuất --"
                            },
                            new SelectListItem
                            {
                                Value =
                                    InventoryIssue.TypeSale,

                                Text =
                                    InventoryIssue.TypeSale,

                                Selected =
                                    issueType
                                    == InventoryIssue.TypeSale
                            },
                            new SelectListItem
                            {
                                Value =
                                    InventoryIssue
                                        .TypeSupplierReturn,

                                Text =
                                    InventoryIssue
                                        .TypeSupplierReturn,

                                Selected =
                                    issueType
                                    == InventoryIssue
                                        .TypeSupplierReturn
                            }
                        },

                    StatusOptions =
                        new[]
                        {
                            new SelectListItem
                            {
                                Value = string.Empty,
                                Text = "-- Tất cả trạng thái --"
                            },
                            new SelectListItem
                            {
                                Value =
                                    InventoryIssue.StatusPending,

                                Text =
                                    InventoryIssue.StatusPending,

                                Selected =
                                    status
                                    == InventoryIssue.StatusPending
                            },
                            new SelectListItem
                            {
                                Value =
                                    InventoryIssue.StatusIssued,

                                Text =
                                    InventoryIssue.StatusIssued,

                                Selected =
                                    status
                                    == InventoryIssue.StatusIssued
                            },
                            new SelectListItem
                            {
                                Value =
                                    InventoryIssue.StatusCancelled,

                                Text =
                                    InventoryIssue.StatusCancelled,

                                Selected =
                                    status
                                    == InventoryIssue.StatusCancelled
                            }
                        }
                };
            ViewBag.InventoryIssueReceivers =
                await _userManager.Users
                    .AsNoTracking()
                    .Include(user => user.Branch)
                    .Where(user =>
                        user.IsActive
                        && user.BranchId.HasValue)
                    .OrderBy(user => user.FullName)
                    .ToListAsync();
            ViewBag.DisplayWarehouses =
                await _context.Warehouses
                    .AsNoTracking()
                    .Include(warehouse => warehouse.Branch)
                    .Where(warehouse =>
                        warehouse.IsActive
                        && warehouse.Branch.IsActive
                        && warehouse.LocationType
                            == Warehouse.LocationTypeDisplay)
                    .OrderBy(warehouse => warehouse.Name)
                    .ToListAsync();

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult>
            SearchInventoryItemsForIssue(
                int branchId,
                int warehouseId,
                string keyword = null,
                int page = 1)
        {
            var currentUser =
                await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Forbid();
            }

            var isAdmin =
                User.IsInRole(RoleCatalog.Admin);

            var actingBranchId =
                isAdmin
                    ? branchId
                    : currentUser.BranchId ?? 0;

            if (actingBranchId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message =
                        "Không xác định được chi nhánh xuất kho."
                });
            }

            if (warehouseId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message =
                        "Vui lòng chọn kho xuất hàng."
                });
            }

            var warehouse =
                await _context.Warehouses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(item =>
                        item.Id == warehouseId
                        && item.IsActive);

            if (warehouse == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message =
                        "Kho không tồn tại hoặc đã tạm ngưng."
                });
            }

            var canUseWarehouse =
                warehouse.BranchId == actingBranchId
                || await _context.BranchWarehouseAccesses
                    .AsNoTracking()
                    .AnyAsync(access =>
                        access.BranchId == actingBranchId
                        && access.WarehouseId
                            == warehouseId);

            if (!canUseWarehouse)
            {
                return Forbid();
            }

            keyword = NormalizeOrEmpty(keyword);
            page = Math.Max(page, 1);

            const int pageSize = 8;

            var inventoryQuery =
                _context.InventoryItems
                    .AsNoTracking()
                    .Include(item => item.Supplier)
                    .Where(item =>
                        item.WarehouseId == warehouseId
                        && item.QuantityOnHand > 0
                        && item.Status
                            == InventoryItem.StatusAvailable)
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                inventoryQuery =
                    inventoryQuery.Where(item =>
                        item.StockCode.Contains(keyword)
                        || item.ProductName.Contains(keyword)
                        || item.ProductLine.Contains(keyword)
                        || item.Category.Contains(keyword)
                        || item.MaterialType.Contains(keyword)
                        || (
                            item.CertificateCode != null
                            && item.CertificateCode.Contains(
                                keyword)
                        )
                        || (
                            item.Supplier != null
                            && item.Supplier.Name.Contains(
                                keyword)
                        ));
            }

            var totalItems =
                await inventoryQuery.CountAsync();

            var totalPages =
                Math.Max(
                    1,
                    (int)Math.Ceiling(
                        totalItems / (double)pageSize));

            if (page > totalPages)
            {
                page = totalPages;
            }

            var inventoryItems =
                await inventoryQuery
                    .OrderByDescending(item =>
                        item.UpdatedAt)
                    .ThenBy(item =>
                        item.ProductName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

            return Json(new
            {
                success = true,
                page,
                totalPages,
                totalItems,

                items = inventoryItems.Select(item =>
                    new
                    {
                        id = item.Id,
                        stockCode = item.StockCode,
                        productName = item.ProductName,
                        productLine = item.ProductLine,
                        category = item.Category,
                        materialType = item.MaterialType,
                        quantityOnHand =
                            item.QuantityOnHand,
                        weightOnHand =
                            item.WeightOnHand,
                        unitCost =
                            item.UnitCost,
                        certificateCode =
                            item.CertificateCode,
                        supplierName =
                            item.Supplier != null
                                ? item.Supplier.Name
                                : string.Empty
                    })
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult> CreateInventoryIssue(
            int BranchId,
            int WarehouseId,
            int DestinationWarehouseId,
            string ReceiverUserId,
            string ReferenceCode,
            string Note,
            int[] SelectedInventoryItemIds,
            Dictionary<int, string> Quantities,
            Dictionary<int, string> IssuedWeights,
            Dictionary<int, string> DetailNotes)
        {
            var currentUser =
                await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Forbid();
            }

            var isAdmin =
                User.IsInRole(RoleCatalog.Admin);

            int? actingBranchId =
                isAdmin
                    ? BranchId
                    : currentUser.BranchId;

            IActionResult ReturnWithError(string message)
            {
                TempData["ErrorMessage"] = message;

                return RedirectToAction(
                    nameof(InventoryIssues),
                    new
                    {
                        branchId = actingBranchId
                    });
            }

            if (!actingBranchId.HasValue
                || actingBranchId.Value <= 0)
            {
                return ReturnWithError(
                    "Vui lòng chọn chi nhánh thực hiện xuất kho.");
            }

            var branch =
                await _context.Branches
                    .AsNoTracking()
                    .FirstOrDefaultAsync(item =>
                        item.Id == actingBranchId.Value
                        && item.IsActive);

            if (branch == null)
            {
                return ReturnWithError(
                    "Chi nhánh không tồn tại hoặc đã tạm ngưng.");
            }

            ReferenceCode = NormalizeOrEmpty(ReferenceCode);
            Note = NormalizeOrEmpty(Note);


            var warehouse =
                await _context.Warehouses
                    .AsNoTracking()
                    .Include(item => item.Branch)
                    .FirstOrDefaultAsync(item =>
                        item.Id == WarehouseId
                        && item.IsActive);

            if (warehouse == null)
            {
                return ReturnWithError(
                    "Kho xuất hàng không tồn tại hoặc đã tạm ngưng.");
            }
            if (warehouse.LocationType
                != Warehouse.LocationTypeStorage)
            {
                return ReturnWithError(
                    "Chỉ được xuất hàng từ kho lưu trữ.");
            }
            /*
            * Kho phải thuộc chi nhánh hoặc được cấp quyền
            * sử dụng chung cho chi nhánh.
            */
            var canUseWarehouse =
                warehouse.BranchId == actingBranchId.Value
                || await _context.BranchWarehouseAccesses
                    .AsNoTracking()
                    .AnyAsync(access =>
                        access.BranchId
                            == actingBranchId.Value
                        && access.WarehouseId
                            == WarehouseId);

            if (!canUseWarehouse)
            {
                return ReturnWithError(
                    "Chi nhánh không có quyền xuất hàng từ kho đã chọn.");
            }

            var destinationWarehouse =
                await _context.Warehouses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(item =>
                        item.Id == DestinationWarehouseId
                        && item.IsActive);

            if (destinationWarehouse == null
                || destinationWarehouse.LocationType
                    != Warehouse.LocationTypeDisplay)
            {
                return ReturnWithError(
                    "Quầy trưng bày nhận hàng không hợp lệ.");
            }

            if (destinationWarehouse.BranchId
                != actingBranchId.Value)
            {
                return ReturnWithError(
                    "Quầy nhận không thuộc chi nhánh đang thực hiện xuất kho.");
            }

            var receiver =
                await _userManager.Users
                    .FirstOrDefaultAsync(user =>
                        user.Id == ReceiverUserId
                        && user.IsActive);

            if (receiver == null)
            {
                return ReturnWithError(
                    "Người nhận tại quầy không tồn tại hoặc đã ngừng hoạt động.");
            }

            if (receiver.BranchId
                != destinationWarehouse.BranchId)
            {
                return ReturnWithError(
                    "Người nhận không thuộc chi nhánh của quầy nhận hàng.");
            }

            var selectedItemIds =
                (SelectedInventoryItemIds
                    ?? Array.Empty<int>())
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (selectedItemIds.Count == 0)
            {
                return ReturnWithError(
                    "Vui lòng chọn ít nhất một mặt hàng để xuất.");
            }

            var inventoryItems =
                await _context.InventoryItems
                    .Include(item => item.Supplier)
                    .Where(item =>
                        selectedItemIds.Contains(item.Id)
                        && item.WarehouseId == WarehouseId)
                    .ToListAsync();

            if (inventoryItems.Count
                != selectedItemIds.Count)
            {
                return ReturnWithError(
                    "Có mã tồn không tồn tại hoặc không thuộc kho đã chọn.");
            }

            Quantities ??=
                new Dictionary<int, string>();

            IssuedWeights ??=
                new Dictionary<int, string>();

            DetailNotes ??=
                new Dictionary<int, string>();

            int ParseQuantity(int inventoryItemId)
            {
                if (!Quantities.TryGetValue(
                    inventoryItemId,
                    out var value))
                {
                    return 0;
                }

                return int.TryParse(
                    NormalizeOrEmpty(value),
                    out var result)
                        ? result
                        : 0;
            }

            decimal ParseWeight(int inventoryItemId)
            {
                if (!IssuedWeights.TryGetValue(
                    inventoryItemId,
                    out var value))
                {
                    return 0m;
                }

                value = NormalizeOrEmpty(value)
                    .Replace(",", ".");

                return decimal.TryParse(
                    value,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out var result)
                        ? result
                        : 0m;
            }

            string GetDetailNote(int inventoryItemId)
            {
                return DetailNotes.TryGetValue(
                    inventoryItemId,
                    out var value)
                        ? NormalizeOrEmpty(value)
                        : string.Empty;
            }

            var details =
                new List<InventoryIssueDetail>();

            foreach (var item in inventoryItems)
            {
                if (item.Status
                        != InventoryItem.StatusAvailable
                    || item.QuantityOnHand <= 0)
                {
                    return ReturnWithError(
                        $"Mã tồn {item.StockCode} không ở trạng thái sẵn sàng để xuất.");
                }


                var quantity =
                    ParseQuantity(item.Id);

                if (quantity <= 0)
                {
                    return ReturnWithError(
                        $"Số lượng xuất của {item.ProductName} phải lớn hơn 0.");
                }

                if (quantity > item.QuantityOnHand)
                {
                    return ReturnWithError(
                        $"Số lượng xuất của {item.ProductName} vượt quá tồn kho hiện tại.");
                }

                var issuedWeight =
                    ParseWeight(item.Id);

                if (item.WeightOnHand > 0)
                {
                    /*
                    * Nếu xuất hết số lượng thì tự dùng toàn bộ
                    * trọng lượng còn lại để tránh tồn 0 sản phẩm
                    * nhưng vẫn còn trọng lượng.
                    */
                    if (quantity == item.QuantityOnHand)
                    {
                        issuedWeight =
                            item.WeightOnHand;
                    }
                    else
                    {
                        if (issuedWeight <= 0)
                        {
                            return ReturnWithError(
                                $"Vui lòng nhập trọng lượng thực tế xuất của {item.ProductName}.");
                        }

                        if (issuedWeight
                            >= item.WeightOnHand)
                        {
                            return ReturnWithError(
                                $"Xuất một phần {item.ProductName} thì trọng lượng xuất phải nhỏ hơn trọng lượng đang tồn.");
                        }
                    }
                }
                else
                {
                    issuedWeight = 0;
                }

                var detailNote =
                    GetDetailNote(item.Id);

                if (detailNote.Length > 500)
                {
                    return ReturnWithError(
                        $"Ghi chú của {item.ProductName} không được vượt quá 500 ký tự.");
                }

                details.Add(
                    new InventoryIssueDetail
                    {
                        InventoryItemId = item.Id,
                        Quantity = quantity,
                        IssuedWeight = issuedWeight,
                        UnitCost = item.UnitCost,
                        Note = detailNote
                    });
            }

            var issueCodeSuffix =
                Guid.NewGuid()
                    .ToString("N")
                    .Substring(0, 6)
                    .ToUpperInvariant();

            var issue =
                new InventoryIssue
                {
                    IssueCode =
                        $"PXK-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{issueCodeSuffix}",

                    BranchId =
                        actingBranchId.Value,

                    WarehouseId =
                        WarehouseId,
                    DestinationWarehouseId =
                        destinationWarehouse.Id,

                    ReceiverUserId =
                        receiver.Id,
                    SupplierId = null,

                    IssueType =
                        InventoryIssue.TypeSale,

                    Status =
                        InventoryIssue.StatusPending,

                

                    ReferenceCode =
                        string.IsNullOrWhiteSpace(
                            ReferenceCode)
                            ? null
                            : ReferenceCode,

                    Reason =
                        null,

                    Note =
                        string.IsNullOrWhiteSpace(Note)
                            ? null
                            : Note,

                    CreatedByUserId =
                        currentUser.Id,

                    CreatedAt =
                        DateTime.UtcNow,

                    Details =
                        details
                };

            try
            {
                _context.InventoryIssues.Add(issue);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    $"Đã tạo phiếu {issue.IssueCode}. "
                    + "Phiếu đang chờ xác nhận xuất kho.";

                return RedirectToAction(
                    nameof(InventoryIssues),
                    new
                    {
                        branchId =
                            actingBranchId.Value
                    });
            }
            catch (Exception exception)
            {
                TempData["ErrorMessage"] =
                    "Không thể tạo phiếu xuất kho. Chi tiết lỗi: "
                    + exception.GetBaseException().Message;

                return RedirectToAction(
                    nameof(InventoryIssues),
                    new
                    {
                        branchId =
                            actingBranchId.Value
                    });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult> ConfirmInventoryIssue(
            int issueId)
        {
            var currentUser =
                await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Forbid();
            }

            InventoryIssue issue = null;

            IActionResult ReturnWithError(string message)
            {
                TempData["ErrorMessage"] = message;

                return RedirectToAction(
                    nameof(InventoryIssues),
                    new
                    {
                        branchId = issue?.BranchId
                    });
            }

            try
            {
                issue =
                    await _context.InventoryIssues
                        .Include(item => item.Warehouse)
                        .Include(item =>
                            item.DestinationWarehouse)
                        .Include(item => item.ReceiverUser)
                        .Include(item => item.Details)
                            .ThenInclude(detail =>
                                detail.InventoryItem)
                        .FirstOrDefaultAsync(item =>
                            item.Id == issueId);

                if (issue == null)
                {
                    return ReturnWithError(
                        "Không tìm thấy phiếu xuất kho.");
                }

                if (issue.Status
                    != InventoryIssue.StatusPending)
                {
                    return ReturnWithError(
                        "Phiếu này không còn ở trạng thái chờ xuất kho.");
                }

                if (!User.IsInRole(RoleCatalog.Admin)
                    && currentUser.BranchId
                        != issue.BranchId)
                {
                    return ReturnWithError(
                        "Bạn không có quyền xác nhận phiếu của chi nhánh này.");
                }

                if (issue.Warehouse == null
                    || !issue.Warehouse.IsActive
                    || issue.Warehouse.LocationType
                        != Warehouse.LocationTypeStorage)
                {
                    return ReturnWithError(
                        "Kho xuất không tồn tại, đã tạm ngưng hoặc không phải kho lưu trữ.");
                }

                if (issue.DestinationWarehouse == null
                    || !issue.DestinationWarehouse.IsActive
                    || issue.DestinationWarehouse.LocationType
                        != Warehouse.LocationTypeDisplay)
                {
                    return ReturnWithError(
                        "Quầy nhận không tồn tại, đã tạm ngưng hoặc không phải quầy trưng bày.");
                }

                if (issue.DestinationWarehouse.BranchId
                    != issue.BranchId)
                {
                    return ReturnWithError(
                        "Quầy nhận không thuộc chi nhánh của phiếu xuất.");
                }

                if (issue.ReceiverUser == null
                    || !issue.ReceiverUser.IsActive)
                {
                    return ReturnWithError(
                        "Người nhận tại quầy không còn hoạt động.");
                }

                if (issue.ReceiverUser.BranchId
                    != issue.DestinationWarehouse.BranchId)
                {
                    return ReturnWithError(
                        "Người nhận không thuộc chi nhánh của quầy.");
                }

                if (issue.Details == null
                    || issue.Details.Count == 0)
                {
                    return ReturnWithError(
                        "Phiếu xuất không có hàng hóa.");
                }

                /*
                * Kiểm tra toàn bộ trước khi cập nhật tồn kho.
                */
                foreach (var detail in issue.Details)
                {
                    var sourceItem =
                        detail.InventoryItem;

                    if (sourceItem == null)
                    {
                        return ReturnWithError(
                            "Có mặt hàng trong phiếu không còn tồn tại.");
                    }

                    if (sourceItem.WarehouseId
                        != issue.WarehouseId)
                    {
                        return ReturnWithError(
                            $"Mã tồn {sourceItem.StockCode} không thuộc kho xuất.");
                    }

                    if (sourceItem.Status
                            != InventoryItem.StatusAvailable
                        || sourceItem.QuantityOnHand
                            < detail.Quantity)
                    {
                        return ReturnWithError(
                            $"Mã tồn {sourceItem.StockCode} không còn đủ số lượng để xuất.");
                    }

                    if (detail.IssuedWeight < 0
                        || detail.IssuedWeight
                            > sourceItem.WeightOnHand)
                    {
                        return ReturnWithError(
                            $"Trọng lượng xuất của mã tồn {sourceItem.StockCode} không hợp lệ.");
                    }
                }

                var now = DateTime.UtcNow;

                string BuildCode(string prefix)
                {
                    var suffix =
                        Guid.NewGuid()
                            .ToString("N")
                            .Substring(0, 6)
                            .ToUpperInvariant();

                    return $"{prefix}-{now:yyyyMMddHHmmssfff}-{suffix}";
                }

                foreach (var detail in issue.Details)
                {
                    var sourceItem =
                        detail.InventoryItem;

                    /*
                    * Trừ tồn tại kho lưu trữ.
                    */
                    sourceItem.QuantityOnHand -=
                        detail.Quantity;

                    sourceItem.WeightOnHand =
                        Math.Max(
                            0m,
                            sourceItem.WeightOnHand
                                - detail.IssuedWeight);

                    sourceItem.UpdatedAt = now;

                    if (sourceItem.QuantityOnHand == 0)
                    {
                        sourceItem.WeightOnHand = 0;

                        sourceItem.Status =
                            InventoryItem.StatusOutOfStock;
                    }

                    /*
                    * Tạo mã tồn mới tại quầy để giữ nguyên
                    * nguồn gốc của lô hàng.
                    */
                    var displayItem =
                        new InventoryItem
                        {
                            StockCode =
                                BuildCode("INV"),

                            WarehouseId =
                                issue.DestinationWarehouse.Id,

                            SupplierId =
                                sourceItem.SupplierId,

                            SupplierPurchaseOrderId =
                                sourceItem.SupplierPurchaseOrderId,

                            SupplierGoodsReceiptDetailId =
                                sourceItem.SupplierGoodsReceiptDetailId,

                            ProductLine =
                                sourceItem.ProductLine,

                            Category =
                                sourceItem.Category,

                            ProductName =
                                sourceItem.ProductName,

                            MaterialType =
                                sourceItem.MaterialType,

                            QuantityOnHand =
                                detail.Quantity,

                            WeightOnHand =
                                detail.IssuedWeight,

                            DiamondCarat =
                                sourceItem.DiamondCarat,

                            CertificateCode =
                                sourceItem.CertificateCode,

                            UnitCost =
                                detail.UnitCost,

                            Status =
                                InventoryItem.StatusAvailable,

                            Note =
                                $"Nhận từ mã tồn {sourceItem.StockCode} theo phiếu {issue.IssueCode}.",

                            CreatedAt = now,
                            UpdatedAt = now
                        };

                    /*
                    * Giao dịch trừ tại kho nguồn.
                    */
                    var transferOut =
                        new InventoryTransaction
                        {
                            TransactionCode =
                                BuildCode("ITX"),

                            WarehouseId =
                                issue.WarehouseId,

                            InventoryItemId =
                                sourceItem.Id,

                            TransactionType =
                                InventoryTransaction
                                    .TypeTransferOut,

                            QuantityChange =
                                -detail.Quantity,

                            WeightChange =
                                -detail.IssuedWeight,

                            QuantityAfter =
                                sourceItem.QuantityOnHand,

                            WeightAfter =
                                sourceItem.WeightOnHand,

                            ReferenceType =
                                "Phiếu xuất kho",

                            ReferenceId =
                                issue.Id,

                            Note =
                                $"Xuất ra {issue.DestinationWarehouse.Name} theo phiếu {issue.IssueCode}.",

                            CreatedByUserId =
                                currentUser.Id,

                            CreatedAt = now
                        };

                    /*
                    * Giao dịch cộng tại quầy.
                    */
                    var transferIn =
                        new InventoryTransaction
                        {
                            TransactionCode =
                                BuildCode("ITX"),

                            WarehouseId =
                                issue.DestinationWarehouse.Id,

                            InventoryItem =
                                displayItem,

                            TransactionType =
                                InventoryTransaction
                                    .TypeTransferIn,

                            QuantityChange =
                                detail.Quantity,

                            WeightChange =
                                detail.IssuedWeight,

                            QuantityAfter =
                                detail.Quantity,

                            WeightAfter =
                                detail.IssuedWeight,

                            ReferenceType =
                                "Phiếu xuất kho",

                            ReferenceId =
                                issue.Id,

                            Note =
                                $"Nhận từ {issue.Warehouse.Name} theo phiếu {issue.IssueCode}.",

                            CreatedByUserId =
                                currentUser.Id,

                            CreatedAt = now
                        };

                    _context.InventoryItems.Add(
                        displayItem);

                    _context.InventoryTransactions.AddRange(
                        transferOut,
                        transferIn);
                }

                issue.Status =
                    InventoryIssue.StatusIssued;

                issue.ConfirmedByUserId =
                    currentUser.Id;

                issue.IssuedAt = now;

                await _context.SaveChangesAsync();


                TempData["SuccessMessage"] =
                    $"Đã xác nhận phiếu {issue.IssueCode}. "
                    + $"Hàng đã được chuyển sang "
                    + $"{issue.DestinationWarehouse.Name}.";

                return RedirectToAction(
                    nameof(InventoryIssues),
                    new
                    {
                        branchId = issue.BranchId
                    });
            }
            catch (Exception exception)
            {

                return ReturnWithError(
                    "Không thể xác nhận xuất kho: "
                    + exception.GetBaseException().Message);
            }
        }

        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult> InventoryManagement(
            string searchTerm = null,
            int? branchId = null,
            int? warehouseId = null,
            string statusFilter = null)
        {
            searchTerm = NormalizeOrEmpty(searchTerm);
            statusFilter = NormalizeOrEmpty(statusFilter);

            var currentUser = await _userManager.GetUserAsync(User);

            var isAdmin = User.IsInRole(RoleCatalog.Admin);

            int? scopedBranchId =
                isAdmin
                    ? branchId
                    : currentUser?.BranchId;

            if (!isAdmin)
            {
                branchId = scopedBranchId;
            }

            var warehouseQuery = _context.Warehouses
                .Include(warehouse => warehouse.Branch)
                .Include(warehouse => warehouse.InventoryItems)
                .AsQueryable();

            var inventoryQuery = _context.InventoryItems
                .AsNoTracking()
                .Include(item => item.Warehouse)
                    .ThenInclude(warehouse => warehouse.Branch)
                .Include(item => item.Supplier)
                .Include(item => item.SupplierPurchaseOrder)
                .Include(item => item.SupplierGoodsReceiptDetail)
                    .ThenInclude(detail => detail.SupplierGoodsReceipt)
                .AsQueryable();

            /*
            * Admin được xem toàn bộ kho.
            * Các vai trò còn lại chỉ xem kho thuộc chi nhánh được gán.
            */
            if (!isAdmin)
            {
                if (scopedBranchId.HasValue)
                {
                    var accessibleWarehouseIds = await _context.BranchWarehouseAccesses
                        .Where(access => access.BranchId == scopedBranchId.Value)
                        .Select(access => access.WarehouseId)
                        .Concat(_context.Warehouses.Where(warehouse => warehouse.BranchId == scopedBranchId.Value).Select(warehouse => warehouse.Id))
                        .Distinct().ToListAsync();
                    warehouseQuery = warehouseQuery.Where(
                        warehouse => accessibleWarehouseIds.Contains(warehouse.Id));

                    inventoryQuery = inventoryQuery.Where(
                        item => accessibleWarehouseIds.Contains(item.WarehouseId));

                    branchId = scopedBranchId.Value;
                }
                else
                {
                    warehouseQuery = warehouseQuery.Where(warehouse => false);
                    inventoryQuery = inventoryQuery.Where(item => false);

                    ViewBag.InventoryScopeWarning =
                        "Tài khoản của bạn chưa được gán chi nhánh nên chưa thể xem dữ liệu kho.";
                }
            }

            var accessibleWarehouses = await warehouseQuery
                .OrderBy(warehouse => warehouse.Branch.BranchName)
                .ThenBy(warehouse => warehouse.Name)
                .ToListAsync();

            /*
            * Danh sách này dùng để tính thống kê tổng quan,
            * chưa áp dụng bộ lọc trên giao diện.
            */
            

            if (branchId.HasValue)
            {
                var selectedWarehouseIds = await _context.BranchWarehouseAccesses
                    .Where(access => access.BranchId == branchId.Value)
                    .Select(access => access.WarehouseId)
                    .Concat(_context.Warehouses.Where(warehouse => warehouse.BranchId == branchId.Value).Select(warehouse => warehouse.Id))
                    .Distinct().ToListAsync();
                inventoryQuery = inventoryQuery.Where(
                    item => selectedWarehouseIds.Contains(item.WarehouseId));
            }

            if (warehouseId.HasValue)
            {
                inventoryQuery = inventoryQuery.Where(
                    item => item.WarehouseId == warehouseId.Value);
            }

            var overviewItems = await inventoryQuery
                .AsNoTracking()
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                inventoryQuery = inventoryQuery.Where(
                    item => item.Status == statusFilter);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                inventoryQuery = inventoryQuery.Where(item =>
                    item.StockCode.Contains(searchTerm)
                    || item.ProductName.Contains(searchTerm)
                    || item.ProductLine.Contains(searchTerm)
                    || item.Category.Contains(searchTerm)
                    || item.MaterialType.Contains(searchTerm)
                    || (
                        item.CertificateCode != null
                        && item.CertificateCode.Contains(searchTerm)
                    )
                    || (
                        item.Supplier != null
                        && item.Supplier.Name.Contains(searchTerm)
                    )
                    || item.Warehouse.Name.Contains(searchTerm));
            }

            var inventoryItems = await inventoryQuery
                .OrderByDescending(item => item.UpdatedAt)
                .ThenBy(item => item.StockCode)
                .ToListAsync();

            var branchQuery = _context.Branches
                .Where(branch => branch.IsActive)
                .AsQueryable();

            if (!isAdmin)
            {
                if (scopedBranchId.HasValue)
                {
                    branchQuery = branchQuery.Where(
                        branch => branch.Id == scopedBranchId.Value);
                }
                else
                {
                    branchQuery = branchQuery.Where(branch => false);
                }
            }

            var branches = await branchQuery
                .OrderBy(branch => branch.BranchName)
                .ToListAsync();

            var model = new InventoryManagementViewModel
            {
                SearchTerm = searchTerm,
                BranchId = branchId,
                WarehouseId = warehouseId,
                StatusFilter = statusFilter,

                TotalWarehouses = accessibleWarehouses.Count,

                ActiveWarehouses = accessibleWarehouses.Count(
                    warehouse => warehouse.IsActive),

                TotalItemLines = overviewItems.Count,

                TotalQuantity = overviewItems.Sum(
                    item => item.QuantityOnHand),

                TotalWeight = overviewItems.Sum(
                    item => item.WeightOnHand),

                TotalInventoryValue = overviewItems.Sum(
                    item => item.InventoryValue),

                Warehouses = accessibleWarehouses,

                InventoryItems = inventoryItems,

                BranchOptions = new[]
                {
                    new SelectListItem
                    {
                        Value = string.Empty,
                        Text = "-- Tất cả chi nhánh --"
                    }
                }
                .Concat(branches.Select(branch => new SelectListItem
                {
                    Value = branch.Id.ToString(),
                    Text = branch.BranchName,
                    Selected = branchId == branch.Id
                }))
                .ToList(),

                WarehouseOptions = new[]
                {
                    new SelectListItem
                    {
                        Value = string.Empty,
                        Text = "-- Tất cả kho --"
                    }
                }
                .Concat(accessibleWarehouses.Select(warehouse => new SelectListItem
                {
                    Value = warehouse.Id.ToString(),
                    Text = $"{warehouse.Code} - {warehouse.Name}",
                    Selected = warehouseId == warehouse.Id
                }))
                .ToList(),

                StatusOptions = new[]
                {
                    new SelectListItem
                    {
                        Value = string.Empty,
                        Text = "-- Tất cả trạng thái --"
                    },
                    new SelectListItem
                    {
                        Value = InventoryItem.StatusAvailable,
                        Text = InventoryItem.StatusAvailable,
                        Selected = statusFilter == InventoryItem.StatusAvailable
                    },
                    new SelectListItem
                    {
                        Value = InventoryItem.StatusReserved,
                        Text = InventoryItem.StatusReserved,
                        Selected = statusFilter == InventoryItem.StatusReserved
                    },
                    new SelectListItem
                    {
                        Value = InventoryItem.StatusQuarantined,
                        Text = InventoryItem.StatusQuarantined,
                        Selected = statusFilter == InventoryItem.StatusQuarantined
                    },
                    new SelectListItem
                    {
                        Value = InventoryItem.StatusOutOfStock,
                        Text = InventoryItem.StatusOutOfStock,
                        Selected = statusFilter == InventoryItem.StatusOutOfStock
                    }
                }
            };

            return View(model);
        }
        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult> InventoryHistory(
            string searchTerm = null,
            int? branchId = null,
            int? warehouseId = null,
            string transactionType = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            searchTerm = NormalizeOrEmpty(searchTerm);
            transactionType = NormalizeOrEmpty(transactionType);

            var currentUser = await _userManager.GetUserAsync(User);

            var isAdmin = User.IsInRole(RoleCatalog.Admin);

            int? scopedBranchId = null;

            if (!isAdmin)
            {
                scopedBranchId = branchId ?? currentUser?.BranchId;

                /*
                * Manager và Branch Owner bắt buộc chỉ được xem
                * lịch sử kho thuộc chi nhánh của tài khoản.
                */
                branchId = scopedBranchId;
            }

            var scopedTransactionQuery = _context.InventoryTransactions
                .AsNoTracking()
                .Include(transaction => transaction.Warehouse)
                    .ThenInclude(warehouse => warehouse.Branch)
                .Include(transaction => transaction.InventoryItem)
                    .ThenInclude(item => item.Supplier)
                .Include(transaction => transaction.CreatedByUser)
                .AsQueryable();

            var accessibleWarehouseQuery = _context.Warehouses
                .AsNoTracking()
                .Include(warehouse => warehouse.Branch)
                .AsQueryable();

            if (!isAdmin)
            {
                if (scopedBranchId.HasValue)
                {
                    var accessibleWarehouseIds = await _context.BranchWarehouseAccesses
                        .Where(access => access.BranchId == scopedBranchId.Value)
                        .Select(access => access.WarehouseId)
                        .Concat(_context.Warehouses.Where(warehouse => warehouse.BranchId == scopedBranchId.Value).Select(warehouse => warehouse.Id))
                        .Distinct().ToListAsync();
                    scopedTransactionQuery = scopedTransactionQuery.Where(
                        transaction =>
                            accessibleWarehouseIds.Contains(transaction.WarehouseId));

                    accessibleWarehouseQuery = accessibleWarehouseQuery.Where(
                        warehouse =>
                            accessibleWarehouseIds.Contains(warehouse.Id));
                }
                else
                {
                    scopedTransactionQuery = scopedTransactionQuery.Where(
                        transaction => false);

                    accessibleWarehouseQuery = accessibleWarehouseQuery.Where(
                        warehouse => false);

                    ViewBag.InventoryScopeWarning =
                        "Tài khoản của bạn chưa được gán chi nhánh nên chưa thể xem lịch sử kho.";
                }
            }

            /*
            * Thống kê được tính theo toàn bộ dữ liệu mà tài khoản
            * có quyền xem, chưa áp dụng bộ lọc trên giao diện.
            */
            var totalTransactions =
                await scopedTransactionQuery.CountAsync();

            var totalQuantityReceived =
                await scopedTransactionQuery
                    .Where(transaction =>
                        transaction.QuantityChange > 0)
                    .SumAsync(transaction =>
                        (int?)transaction.QuantityChange)
                ?? 0;

            var totalNegativeQuantity =
                await scopedTransactionQuery
                    .Where(transaction =>
                        transaction.QuantityChange < 0)
                    .SumAsync(transaction =>
                        (int?)transaction.QuantityChange)
                ?? 0;

            var totalQuantityIssued =
                Math.Abs(totalNegativeQuantity);

            var totalPositiveWeight =
                await scopedTransactionQuery
                    .Where(transaction =>
                        transaction.WeightChange > 0)
                    .SumAsync(transaction =>
                        (decimal?)transaction.WeightChange)
                ?? 0m;

            var totalNegativeWeight =
                await scopedTransactionQuery
                    .Where(transaction =>
                        transaction.WeightChange < 0)
                    .SumAsync(transaction =>
                        (decimal?)transaction.WeightChange)
                ?? 0m;

            var totalWeightMoved =
                totalPositiveWeight + Math.Abs(totalNegativeWeight);

            var filteredTransactionQuery =
                scopedTransactionQuery;

            if (branchId.HasValue)
            {
                var selectedWarehouseIds = await _context.BranchWarehouseAccesses
                    .Where(access => access.BranchId == branchId.Value)
                    .Select(access => access.WarehouseId)
                    .Concat(_context.Warehouses.Where(warehouse => warehouse.BranchId == branchId.Value).Select(warehouse => warehouse.Id))
                    .Distinct().ToListAsync();
                filteredTransactionQuery =
                    filteredTransactionQuery.Where(
                        transaction =>
                            selectedWarehouseIds.Contains(transaction.WarehouseId));
            }

            if (warehouseId.HasValue)
            {
                filteredTransactionQuery =
                    filteredTransactionQuery.Where(
                        transaction =>
                            transaction.WarehouseId
                            == warehouseId.Value);
            }

            if (!string.IsNullOrWhiteSpace(transactionType))
            {
                filteredTransactionQuery =
                    filteredTransactionQuery.Where(
                        transaction =>
                            transaction.TransactionType
                            == transactionType);
            }

            if (fromDate.HasValue)
            {
                var startDate = fromDate.Value.Date;

                filteredTransactionQuery =
                    filteredTransactionQuery.Where(
                        transaction =>
                            transaction.CreatedAt >= startDate);
            }

            if (toDate.HasValue)
            {
                var endDateExclusive =
                    toDate.Value.Date.AddDays(1);

                filteredTransactionQuery =
                    filteredTransactionQuery.Where(
                        transaction =>
                            transaction.CreatedAt
                            < endDateExclusive);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                filteredTransactionQuery =
                    filteredTransactionQuery.Where(transaction =>
                        transaction.TransactionCode.Contains(searchTerm)
                        || transaction.InventoryItem.StockCode.Contains(searchTerm)
                        || transaction.InventoryItem.ProductName.Contains(searchTerm)
                        || transaction.InventoryItem.ProductLine.Contains(searchTerm)
                        || transaction.InventoryItem.Category.Contains(searchTerm)
                        || transaction.InventoryItem.MaterialType.Contains(searchTerm)
                        || transaction.Warehouse.Code.Contains(searchTerm)
                        || transaction.Warehouse.Name.Contains(searchTerm)
                        || transaction.Warehouse.Branch.BranchName.Contains(searchTerm)
                        || (
                            transaction.InventoryItem.Supplier != null
                            && transaction.InventoryItem.Supplier.Name.Contains(searchTerm)
                        )
                        || (
                            transaction.CreatedByUser != null
                            && transaction.CreatedByUser.FullName.Contains(searchTerm)
                        )
                        || (
                            transaction.Note != null
                            && transaction.Note.Contains(searchTerm)
                        ));
            }

            /*
            * Tạm giới hạn 500 giao dịch mới nhất để trang không quá nặng.
            * Thống kê phía trên vẫn tính trên toàn bộ dữ liệu.
            */
            var transactions = await filteredTransactionQuery
                .OrderByDescending(transaction =>
                    transaction.CreatedAt)
                .ThenByDescending(transaction =>
                    transaction.Id)
                .Take(500)
                .ToListAsync();

            var branchQuery = _context.Branches
                .AsNoTracking()
                .Where(branch => branch.IsActive)
                .AsQueryable();

            if (!isAdmin)
            {
                if (scopedBranchId.HasValue)
                {
                    branchQuery = branchQuery.Where(
                        branch =>
                            branch.Id == scopedBranchId.Value);
                }
                else
                {
                    branchQuery = branchQuery.Where(
                        branch => false);
                }
            }

            var branches = await branchQuery
                .OrderBy(branch => branch.BranchName)
                .ToListAsync();

            var accessibleWarehouses =
                await accessibleWarehouseQuery
                    .OrderBy(warehouse =>
                        warehouse.Branch.BranchName)
                    .ThenBy(warehouse =>
                        warehouse.Name)
                    .ToListAsync();

            var warehousesForFilter =
                accessibleWarehouses.AsEnumerable();

            if (branchId.HasValue)
            {
                warehousesForFilter =
                    warehousesForFilter.Where(
                        warehouse =>
                            warehouse.BranchId
                            == branchId.Value);
            }

            var transactionTypes = new[]
            {
                InventoryTransaction.TypeSupplierReceipt,
                InventoryTransaction.TypeCustomerIssue,
                InventoryTransaction.TypeSupplierReturn,
                InventoryTransaction.TypeAdjustmentIncrease,
                InventoryTransaction.TypeAdjustmentDecrease,
                InventoryTransaction.TypeTransferIn,
                InventoryTransaction.TypeTransferOut
            };

            var model = new InventoryHistoryViewModel
            {
                SearchTerm = searchTerm,
                BranchId = branchId,
                WarehouseId = warehouseId,
                TransactionType = transactionType,
                FromDate = fromDate,
                ToDate = toDate,

                TotalTransactions = totalTransactions,
                TotalQuantityReceived = totalQuantityReceived,
                TotalQuantityIssued = totalQuantityIssued,
                TotalWeightMoved = totalWeightMoved,

                Transactions = transactions,

                BranchOptions = new[]
                {
                    new SelectListItem
                    {
                        Value = string.Empty,
                        Text = "-- Tất cả chi nhánh --"
                    }
                }
                .Concat(branches.Select(branch =>
                    new SelectListItem
                    {
                        Value = branch.Id.ToString(),
                        Text = branch.BranchName,
                        Selected = branchId == branch.Id
                    }))
                .ToList(),

                WarehouseOptions = new[]
                {
                    new SelectListItem
                    {
                        Value = string.Empty,
                        Text = "-- Tất cả kho --"
                    }
                }
                .Concat(warehousesForFilter.Select(warehouse =>
                    new SelectListItem
                    {
                        Value = warehouse.Id.ToString(),
                        Text =
                            $"{warehouse.Code} - {warehouse.Name}",
                        Selected =
                            warehouseId == warehouse.Id
                    }))
                .ToList(),

                TransactionTypeOptions = new[]
                {
                    new SelectListItem
                    {
                        Value = string.Empty,
                        Text = "-- Tất cả loại giao dịch --"
                    }
                }
                .Concat(transactionTypes.Select(type =>
                    new SelectListItem
                    {
                        Value = type,
                        Text = type,
                        Selected =
                            transactionType == type
                    }))
                .ToList()
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateWarehouse(
            string Code,
            string Name,
            int BranchId,
            string Location,
            string LocationType)
        {
            Code = NormalizeOrEmpty(Code).ToUpperInvariant();
            Name = NormalizeOrEmpty(Name);
            Location = NormalizeOrEmpty(Location);
            LocationType = NormalizeOrEmpty(LocationType);
    

            if (Code.Length < 3 || Code.Length > 30)
            {
                TempData["ErrorMessage"] =
                    "Mã kho phải có từ 3 đến 30 ký tự.";

                return RedirectToAction(nameof(InventoryManagement));
            }

            if (Name.Length < 3 || Name.Length > 150)
            {
                TempData["ErrorMessage"] =
                    "Tên kho phải có từ 3 đến 150 ký tự.";

                return RedirectToAction(nameof(InventoryManagement));
            }

            if (Location.Length > 300)
            {
                TempData["ErrorMessage"] =
                    "Vị trí kho không được vượt quá 300 ký tự.";

                return RedirectToAction(nameof(InventoryManagement));
            }

            var allowedLocationTypes =
                new[]
                {
                    Warehouse.LocationTypeStorage,
                    Warehouse.LocationTypeDisplay
                };

            if (!allowedLocationTypes.Contains(
                LocationType))
            {
                TempData["ErrorMessage"] =
                    "Loại địa điểm lưu giữ hàng không hợp lệ.";

                return RedirectToAction(
                    nameof(InventoryManagement),
                    new
                    {
                        branchId = BranchId,
                        inventoryTab = "warehouses"
                    });
            }

            var branch = await _context.Branches
                .FirstOrDefaultAsync(item =>
                    item.Id == BranchId
                    && item.IsActive);

            if (branch == null)
            {
                TempData["ErrorMessage"] =
                    "Chi nhánh không tồn tại hoặc đã tạm khóa.";

                return RedirectToAction(nameof(InventoryManagement));
            }

            var currentUser = await _userManager.GetUserAsync(User);

            if (!User.IsInRole(RoleCatalog.Admin)
                && currentUser?.BranchId != BranchId)
            {
                TempData["ErrorMessage"] =
                    "Bạn chỉ được tạo kho cho chi nhánh đang được phân công.";

                return RedirectToAction(nameof(InventoryManagement));
            }

            var normalizedCode = Code.ToLower();

            var codeExists = await _context.Warehouses.AnyAsync(
                warehouse => warehouse.Code.ToLower() == normalizedCode);

            if (codeExists)
            {
                TempData["ErrorMessage"] =
                    $"Mã kho {Code} đã tồn tại.";

                return RedirectToAction(nameof(InventoryManagement));
            }

            var warehouse = new Warehouse
            {
                Code = Code,
                Name = Name,
                BranchId = BranchId,
                Location = Location,
                LocationType = LocationType,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                _context.Warehouses.Add(warehouse);

                var savedRows =
                    await _context.SaveChangesAsync();

                if (savedRows <= 0)
                {
                    TempData["ErrorMessage"] =
                        "Không có dữ liệu kho nào được lưu.";

                    return RedirectToAction(
                        nameof(InventoryManagement),
                        new
                        {
                            branchId = BranchId,
                            inventoryTab = "warehouses"
                        });
                }

                TempData["SuccessMessage"] =
                    $"Đã tạo {LocationType.ToLower()} "
                    + $"{Code} - {Name} cho chi nhánh "
                    + $"{branch.BranchName}.";
            }
            catch (Exception exception)
            {
                TempData["ErrorMessage"] =
                    "Không thể tạo kho: "
                    + exception.GetBaseException().Message;
            }

            return RedirectToAction(
                nameof(InventoryManagement),
                new
                {
                    branchId = BranchId,
                    inventoryTab = "warehouses"
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleWarehouseStatus(
            int warehouseId)
        {
            var warehouse = await _context.Warehouses
                .Include(item => item.Branch)
                .Include(item => item.InventoryItems)
                .FirstOrDefaultAsync(item => item.Id == warehouseId);

            if (warehouse == null)
            {
                TempData["ErrorMessage"] =
                    "Không tìm thấy kho cần cập nhật.";

                return RedirectToAction(nameof(InventoryManagement));
            }

            var currentUser = await _userManager.GetUserAsync(User);

            if (!User.IsInRole(RoleCatalog.Admin)
                && currentUser?.BranchId != warehouse.BranchId)
            {
                TempData["ErrorMessage"] =
                    "Bạn không có quyền cập nhật kho của chi nhánh khác.";

                return RedirectToAction(nameof(InventoryManagement));
            }

            /*
            * Không cho tạm ngưng kho còn hàng.
            * Sau này phải điều chuyển hoặc xuất hết hàng trước.
            */
            if (warehouse.IsActive
                && warehouse.InventoryItems.Any(
                    item => item.QuantityOnHand > 0))
            {
                TempData["ErrorMessage"] =
                    "Không thể tạm ngưng kho đang còn hàng. "
                    + "Hãy chuyển hoặc xuất hết tồn kho trước.";

                return RedirectToAction(nameof(InventoryManagement));
            }

            warehouse.IsActive = !warehouse.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = warehouse.IsActive
                ? $"Đã kích hoạt lại kho {warehouse.Code}."
                : $"Đã tạm ngưng kho {warehouse.Code}.";

            return RedirectToAction(nameof(InventoryManagement));
        }
        [HttpPost]
        [Authorize(Roles = RoleCatalog.Admin)]
        public async Task<IActionResult> UpdateUserRole(string userId, string newRole)
        {
            var targetUser = await _userManager.FindByIdAsync(userId);
            if (targetUser == null) return NotFound();

            var actor = await GetCurrentActorAsync();
            if (!CanAssignRole(actor, newRole))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền gán vai trò này.";
                return RedirectToAction(nameof(UserManagement));
            }

            // Tiến hành cập nhật
            var existingRoles = await _userManager.GetRolesAsync(targetUser);
            
            // Xóa role cũ
            if (existingRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(targetUser, existingRoles);
                if (!removeResult.Succeeded)
                {
                    TempData["ErrorMessage"] = BuildIdentityErrorMessage(removeResult, "Không thể gỡ vai trò cũ của tài khoản.");
                    return RedirectToAction(nameof(UserManagement));
                }
            }
            
            // Thêm role mới
            var addResult = await _userManager.AddToRoleAsync(targetUser, newRole);
            if (!addResult.Succeeded)
            {
                TempData["ErrorMessage"] = BuildIdentityErrorMessage(addResult, "Không thể cập nhật vai trò tài khoản.");
                return RedirectToAction(nameof(UserManagement));
            }

            TempData["SuccessMessage"] = $"Đã cập nhật vai trò của {targetUser.Email} thành {RoleCatalog.GetVietnameseLabel(newRole)}.";
            return RedirectToAction(nameof(UserManagement));
        }

        [HttpPost]
        [Authorize(Roles = RoleCatalog.Admin)]
        public async Task<IActionResult> CreateUser(string FullName, string Email, string Password, string Role)
        {
            FullName = NormalizeOrEmpty(FullName);
            Email = NormalizeOrEmpty(Email);
            Role = NormalizeOrEmpty(Role);

            var actor = await GetCurrentActorAsync();
            if (!CanAssignRole(actor, Role))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền tạo tài khoản với vai trò này.";
                return RedirectToAction(nameof(UserManagement));
            }

            var newUser = new AppUser
            {
                UserName = Email,
                Email = Email,
                FullName = FullName,
                IsActive = true,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(newUser, Password);
            if (createResult.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(newUser, Role);
                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(newUser);
                    TempData["ErrorMessage"] = BuildIdentityErrorMessage(roleResult, "Không thể gán vai trò cho tài khoản mới.");
                    return RedirectToAction(nameof(UserManagement));
                }

                TempData["SuccessMessage"] = $"Thêm mới tài khoản {Email} thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = BuildIdentityErrorMessage(createResult, "Không thể tạo tài khoản mới.");
            }

            return RedirectToAction(nameof(UserManagement));
        }
        private async Task<SupplierManagementViewModel> BuildSupplierManagementViewModelAsync(string searchTerm = null, string statusFilter = null)
        {
            searchTerm = NormalizeOrEmpty(searchTerm);
            statusFilter = NormalizeOrEmpty(statusFilter);

            var suppliersQuery = _context.Suppliers
                .Include(supplier => supplier.PurchaseOrders)
                    .ThenInclude(order => order.Receipts)
                .Include(supplier => supplier.PurchaseOrders)
                    .ThenInclude(order => order.Payments)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                suppliersQuery = suppliersQuery.Where(supplier =>
                    supplier.Name.Contains(searchTerm)
                    || supplier.ContactPerson.Contains(searchTerm)
                    || supplier.Phone.Contains(searchTerm)
                    || (supplier.Email != null && supplier.Email.Contains(searchTerm))
                    || (supplier.TaxCode != null && supplier.TaxCode.Contains(searchTerm)));
            }

            if (string.Equals(statusFilter, "active", StringComparison.OrdinalIgnoreCase))
            {
                suppliersQuery = suppliersQuery.Where(supplier => supplier.IsActive);
            }
            else if (string.Equals(statusFilter, "inactive", StringComparison.OrdinalIgnoreCase))
            {
                suppliersQuery = suppliersQuery.Where(supplier => !supplier.IsActive);
            }
            var suppliers = await suppliersQuery
                .OrderByDescending(supplier => supplier.IsActive)
                .ThenBy(supplier => supplier.Name)
                .ToListAsync();
            var recentPurchaseOrders = await _context.SupplierPurchaseOrders
                .Include(order => order.Supplier)
                .Include(order => order.Branch)
                .Include(order => order.Details)
                .Include(order => order.Receipts)
                .Include(order => order.Payments)
                .OrderByDescending(order => order.CreatedAt)
                .Take(8)
                .ToListAsync();
            var recentReceipts = await _context.SupplierGoodsReceipts
                .AsNoTracking()
                .Include(receipt =>
                    receipt.SupplierPurchaseOrder)
                    .ThenInclude(order =>
                        order.Supplier)
                .Include(receipt =>
                    receipt.SupplierPurchaseOrder)
                    .ThenInclude(order =>
                        order.Branch)
                .Include(receipt =>
                    receipt.Warehouse)
                    .ThenInclude(warehouse =>
                        warehouse.Branch)
                .Include(receipt =>
                    receipt.CreatedByUser)
                .Include(receipt =>
                    receipt.Details)
                    .ThenInclude(detail =>
                        detail.SupplierPurchaseOrderDetail)
                .OrderByDescending(receipt =>
                    receipt.ReceivedAt)
                .Take(10)
                .ToListAsync();
            var activePurchaseOrders = await _context.SupplierPurchaseOrders
                .CountAsync(order =>
                    order.Status == SupplierPurchaseOrder.StatusOrdered
                    || order.Status == SupplierPurchaseOrder.StatusPartiallyReceived);

            var pendingReceiptCount =
            await _context.SupplierGoodsReceipts
                .CountAsync(receipt =>
                    receipt.Status
                        == SupplierGoodsReceipt.StatusPendingInspection
                    || receipt.Status
                        == SupplierGoodsReceipt.StatusInspecting
                    || receipt.Status
                        == SupplierGoodsReceipt.StatusPendingApproval);

            return new SupplierManagementViewModel
                {
                    SearchTerm = searchTerm,
                    StatusFilter = statusFilter,
                    TotalSuppliers = await _context.Suppliers.CountAsync(),
                    ActivePurchaseOrders = activePurchaseOrders,
                    PendingReceiptCount = pendingReceiptCount,

                    TotalSupplierDebt = suppliers.Sum(
                        supplier =>
                            supplier.PurchaseOrders.Sum(
                                CalculateSupplierOrderDebt)),

                    Suppliers = suppliers,
                    PurchaseOrders = recentPurchaseOrders,
                    RecentReceipts = recentReceipts,
                    RecentPayments = new List<SupplierPayment>(),

                    SupplierOptions =
                        await BuildActiveSupplierOptionsAsync(),

                    BranchOptions =
                        await BuildActiveBranchOptionsAsync(),

                    ProductLineOptions =
                        BuildSupplierProductLineOptions(),

                    PaymentMethodOptions =
                        BuildSupplierPaymentMethodOptions()
                };
        }

        private async Task<IReadOnlyList<SelectListItem>> BuildActiveSupplierOptionsAsync()
        {
            var suppliers = await _context.Suppliers
                .Where(supplier => supplier.IsActive)
                .OrderBy(supplier => supplier.Name)
                .ToListAsync();

            return new[]
                {
                    new SelectListItem
                    {
                        Value = string.Empty,
                        Text = "-- Chọn nhà cung cấp --"
                    }
                }
                .Concat(suppliers.Select(supplier => new SelectListItem
                {
                    Value = supplier.Id.ToString(),
                    Text = supplier.Name
                }))
                .ToList();
        }

        private async Task<IReadOnlyList<SelectListItem>> BuildActiveBranchOptionsAsync()
        {
            var branches = await _context.Branches
                .Where(branch => branch.IsActive)
                .OrderBy(branch => branch.BranchName)
                .ToListAsync();

            return new[]
                {
                    new SelectListItem
                    {
                        Value = string.Empty,
                        Text = "-- Chọn chi nhánh nhận hàng --"
                    }
                }
                .Concat(branches.Select(branch => new SelectListItem
                {
                    Value = branch.Id.ToString(),
                    Text = branch.BranchName
                }))
                .ToList();
        }

        private static IReadOnlyList<SelectListItem> BuildSupplierProductLineOptions()
        {
            return new[]
            {
                new SelectListItem { Value = "Vàng", Text = "Vàng" },
                new SelectListItem { Value = "Bạc", Text = "Bạc" },
                new SelectListItem { Value = "Kim cương", Text = "Kim cương" },
                new SelectListItem { Value = "Đá quý", Text = "Đá quý" },
                new SelectListItem { Value = "Phụ kiện / hộp đựng", Text = "Phụ kiện / hộp đựng" }
            };
        }

        private static IReadOnlyList<SelectListItem> BuildSupplierPaymentMethodOptions()
        {
            return new[]
            {
                new SelectListItem { Value = SupplierPayment.MethodBankTransfer, Text = SupplierPayment.MethodBankTransfer },
                new SelectListItem { Value = SupplierPayment.MethodCash, Text = SupplierPayment.MethodCash }
            };
        }

        private static decimal CalculateSupplierOrderAcceptedValue(SupplierPurchaseOrder order)
        {
            return order?.Receipts?.Sum(receipt => receipt.TotalAcceptedValue) ?? 0m;
        }

        private static decimal CalculateSupplierOrderPaidAmount(SupplierPurchaseOrder order)
        {
            return order?.Payments?.Sum(payment => payment.Amount) ?? 0m;
        }

        private static decimal CalculateSupplierOrderDebt(SupplierPurchaseOrder order)
        {
            var debt = CalculateSupplierOrderAcceptedValue(order) - CalculateSupplierOrderPaidAmount(order);
            return debt > 0 ? debt : 0m;
        }
        private static readonly string[] AllowedSupplierTypes =
        {
            "Vàng",
            "Bạc",
            "Kim cương",
            "Đá quý",
            "Phụ kiện / hộp đựng",
            "Gia công / sửa chữa"
        };
        private static readonly string[] AllowedPurchaseOrderProductLines =
        {
            "Vàng",
            "Bạc",
            "Kim cương",
            "Đá quý",
            "Phụ kiện / hộp đựng"
        };

        private static bool SupplierSupportsPurchaseProductLine(Supplier supplier, string productLine)
        {
            if (supplier == null || string.IsNullOrWhiteSpace(productLine))
            {
                return false;
            }

            var supplierTypes = supplier.SupplierType ?? string.Empty;

            if (supplierTypes.Contains("Vàng bạc đá quý", StringComparison.OrdinalIgnoreCase))
            {
                return productLine == "Vàng"
                    || productLine == "Bạc"
                    || productLine == "Kim cương"
                    || productLine == "Đá quý";
            }

            return supplierTypes
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim())
                .Any(item => string.Equals(item, productLine, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsWeightRequiredPurchaseLine(string productLine)
        {
            return string.Equals(productLine, "Vàng", StringComparison.OrdinalIgnoreCase)
                || string.Equals(productLine, "Bạc", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsCaratRequiredPurchaseLine(string productLine)
        {
            return string.Equals(productLine, "Kim cương", StringComparison.OrdinalIgnoreCase)
                || string.Equals(productLine, "Đá quý", StringComparison.OrdinalIgnoreCase);
        }
        private class SupplierPurchaseOrderDetailBuildResult
        {
            public bool IsValid { get; set; }

            public string ErrorMessage { get; set; } = string.Empty;

            public List<SupplierPurchaseOrderDetail> Details { get; set; } = new List<SupplierPurchaseOrderDetail>();
        }

        private SupplierPurchaseOrderDetailBuildResult BuildSupplierPurchaseOrderDetails(
            Supplier supplier,
            string[] productLines,
            string[] categories,
            string[] productNames,
            string[] goldTypes,
            string[] quantities,
            string[] weights,
            string[] diamondCarats,
            string[] diamondCertificates,
            string[] unitCosts,
            string[] detailNotes)
        {
            var result = new SupplierPurchaseOrderDetailBuildResult();

            var rowCount = new[]
            {
                productLines?.Length ?? 0,
                categories?.Length ?? 0,
                productNames?.Length ?? 0,
                goldTypes?.Length ?? 0,
                quantities?.Length ?? 0,
                unitCosts?.Length ?? 0
            }.Max();

            for (var i = 0; i < rowCount; i++)
            {
                var rowNumber = i + 1;

                var productLine = GetArrayValue(productLines, i);
                var category = GetArrayValue(categories, i);
                var productName = GetArrayValue(productNames, i);
                var goldType = GetArrayValue(goldTypes, i);
                var quantity = ParseSupplierInt(GetArrayValue(quantities, i));
                var weight = ParseSupplierDecimal(GetArrayValue(weights, i));
                var diamondCaratValue = ParseSupplierDecimal(GetArrayValue(diamondCarats, i));
                var diamondCarat = diamondCaratValue > 0 ? diamondCaratValue : (decimal?)null;
                var diamondCertificate = GetArrayValue(diamondCertificates, i);
                var unitCost = ParseSupplierDecimal(GetArrayValue(unitCosts, i));
                var detailNote = GetArrayValue(detailNotes, i);

                var isEmptyRow =
                    string.IsNullOrWhiteSpace(productLine)
                    && string.IsNullOrWhiteSpace(category)
                    && string.IsNullOrWhiteSpace(productName)
                    && string.IsNullOrWhiteSpace(goldType)
                    && quantity <= 0
                    && unitCost <= 0;

                if (isEmptyRow)
                {
                    continue;
                }

                if (!AllowedPurchaseOrderProductLines.Any(item => string.Equals(item, productLine, StringComparison.OrdinalIgnoreCase)))
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Dòng {rowNumber}: Vui lòng chọn nhóm hàng hợp lệ.";
                    return result;
                }

                if (!SupplierSupportsPurchaseProductLine(supplier, productLine))
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Dòng {rowNumber}: Nhà cung cấp không có nhóm cung ứng phù hợp với dòng hàng {productLine}.";
                    return result;
                }

                if (string.IsNullOrWhiteSpace(category))
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Dòng {rowNumber}: Vui lòng chọn danh mục hàng.";
                    return result;
                }

                if (string.IsNullOrWhiteSpace(goldType))
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Dòng {rowNumber}: Vui lòng chọn chất liệu / phân loại.";
                    return result;
                }

                if (string.IsNullOrWhiteSpace(productName))
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Dòng {rowNumber}: Vui lòng nhập tên sản phẩm dự kiến nhập.";
                    return result;
                }

                if (quantity <= 0)
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Dòng {rowNumber}: Số lượng đặt hàng phải lớn hơn 0.";
                    return result;
                }

                if (unitCost <= 0)
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Dòng {rowNumber}: Đơn giá nhập phải lớn hơn 0.";
                    return result;
                }

                if (IsWeightRequiredPurchaseLine(productLine) && weight <= 0)
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Dòng {rowNumber}: Trọng lượng phải lớn hơn 0 đối với vàng hoặc bạc.";
                    return result;
                }

                if (IsCaratRequiredPurchaseLine(productLine) && (!diamondCarat.HasValue || diamondCarat.Value <= 0))
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Dòng {rowNumber}: Carat phải lớn hơn 0 đối với kim cương hoặc đá quý.";
                    return result;
                }

                if (!IsWeightRequiredPurchaseLine(productLine))
                {
                    weight = 0;
                }

                if (!IsCaratRequiredPurchaseLine(productLine))
                {
                    diamondCarat = null;
                }

                result.Details.Add(new SupplierPurchaseOrderDetail
                {
                    ProductLine = productLine,
                    Category = category,
                    ProductName = productName,
                    GoldType = goldType,
                    Quantity = quantity,
                    Weight = weight,
                    DiamondCarat = diamondCarat,
                    DiamondCertificate = diamondCertificate,
                    UnitCost = unitCost,
                    TotalCost = quantity * unitCost,
                    ReceivedQuantity = 0,
                    AcceptedQuantity = 0,
                    RejectedQuantity = 0,
                    Note = detailNote
                });
            }

            if (!result.Details.Any())
            {
                result.IsValid = false;
                result.ErrorMessage = "Vui lòng thêm ít nhất một dòng hàng hợp lệ cho đơn đặt hàng.";
                return result;
            }

            result.IsValid = true;
            return result;
        }

        private static string GetArrayValue(string[] values, int index)
        {
            if (values == null || index < 0 || index >= values.Length)
            {
                return string.Empty;
            }

            return NormalizeOrEmpty(values[index]);
        }

        private static int ParseSupplierInt(string value)
        {
            return int.TryParse(value, out var result) ? result : 0;
        }

        private static decimal ParseSupplierDecimal(string value)
        {
            value = NormalizeOrEmpty(value).Replace(",", ".");

            return decimal.TryParse(
                value,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var result)
                ? result
                : 0m;
        }
        private static readonly int[] AllowedPaymentTerms =
        {
            0, 7, 15, 30, 45, 60, 90
        };
        private static readonly string[] AllowedBankNames =
        {
            "Vietcombank",
            "VietinBank",
            "BIDV",
            "Agribank",
            "Techcombank",
            "ACB",
            "MB Bank",
            "Sacombank",
            "VPBank",
            "TPBank",
            "Khác"
        };

        private static string KeepDigitsOnly(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return new string(value.Where(char.IsDigit).ToArray());
        }

        private static bool IsValidVietnamTaxCode(string taxCode)
        {
            if (string.IsNullOrWhiteSpace(taxCode))
            {
                return false;
            }

            return taxCode.All(char.IsDigit)
                && (taxCode.Length == 10 || taxCode.Length == 13);
        }

        private static bool IsValidVietnamPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return false;
            }

            return phone.All(char.IsDigit)
                && phone.Length == 10
                && phone.StartsWith("0");
        }

        private static string BuildSupplierTypeText(string[] supplierTypes)
        {
            if (supplierTypes == null || supplierTypes.Length == 0)
            {
                return string.Empty;
            }

            var result = new List<string>();

            foreach (var supplierType in supplierTypes)
            {
                var matchedType = AllowedSupplierTypes.FirstOrDefault(item =>
                    string.Equals(item, NormalizeOrEmpty(supplierType), StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrWhiteSpace(matchedType) && !result.Contains(matchedType))
                {
                    result.Add(matchedType);
                }
            }

            return string.Join(", ", result);
        }

        private async Task<string> ValidateSupplierInputAsync(
            int? currentSupplierId,
            string name,
            string taxCode,
            string contactPerson,
            string phone,
            string email,
            string address,
            string supplierTypeText,
            int paymentTermDays,
            string bankName,
            string bankAccountNumber,
            string bankAccountName)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length < 3)
            {
                return "Tên nhà cung cấp phải có ít nhất 3 ký tự.";
            }

            if (!IsValidVietnamTaxCode(taxCode))
            {
                return "Mã số thuế không hợp lệ. Mã số thuế phải gồm đúng 10 hoặc 13 chữ số.";
            }

            if (string.IsNullOrWhiteSpace(contactPerson) || contactPerson.Length < 2)
            {
                return "Người liên hệ phải có ít nhất 2 ký tự.";
            }

            if (!IsValidVietnamPhone(phone))
            {
                return "Số điện thoại không hợp lệ. Số điện thoại phải gồm đúng 10 chữ số và bắt đầu bằng số 0.";
            }

            if (!string.IsNullOrWhiteSpace(email) && !new EmailAddressAttribute().IsValid(email))
            {
                return "Email nhà cung cấp không hợp lệ.";
            }

            if (string.IsNullOrWhiteSpace(address) || address.Length < 5)
            {
                return "Địa chỉ nhà cung cấp phải có ít nhất 5 ký tự.";
            }

            if (string.IsNullOrWhiteSpace(supplierTypeText))
            {
                return "Vui lòng chọn ít nhất một nhóm cung ứng.";
            }

            if (!AllowedPaymentTerms.Contains(paymentTermDays))
            {
                return "Số ngày công nợ không hợp lệ. Vui lòng chọn theo danh sách có sẵn.";
            }

            var hasBankInfo =
                !string.IsNullOrWhiteSpace(bankName)
                || !string.IsNullOrWhiteSpace(bankAccountNumber)
                || !string.IsNullOrWhiteSpace(bankAccountName);

            if (hasBankInfo)
            {
                if (string.IsNullOrWhiteSpace(bankName))
                {
                    return "Vui lòng chọn ngân hàng nếu có nhập thông tin tài khoản.";
                }

                if (!AllowedBankNames.Any(item => string.Equals(item, bankName, StringComparison.OrdinalIgnoreCase)))
                {
                    return "Ngân hàng không hợp lệ. Vui lòng chọn ngân hàng trong danh sách.";
                }

                if (string.IsNullOrWhiteSpace(bankAccountNumber))
                {
                    return "Vui lòng nhập số tài khoản ngân hàng.";
                }

                if (!bankAccountNumber.All(char.IsDigit) || bankAccountNumber.Length < 6 || bankAccountNumber.Length > 20)
                {
                    return "Số tài khoản phải là số và có độ dài từ 6 đến 20 chữ số.";
                }

                if (string.IsNullOrWhiteSpace(bankAccountName) || bankAccountName.Length < 3)
                {
                    return "Vui lòng nhập chủ tài khoản ngân hàng hợp lệ.";
                }
            }

            var duplicatedName = await _context.Suppliers.AnyAsync(supplier =>
                supplier.Id != currentSupplierId
                && supplier.Name.ToLower() == name.ToLower());

            if (duplicatedName)
            {
                return "Tên nhà cung cấp này đã tồn tại.";
            }

            var duplicatedTaxCode = await _context.Suppliers.AnyAsync(supplier =>
                supplier.Id != currentSupplierId
                && supplier.TaxCode == taxCode);

            if (duplicatedTaxCode)
            {
                return "Mã số thuế này đã được dùng cho nhà cung cấp khác.";
            }

            var duplicatedPhone = await _context.Suppliers.AnyAsync(supplier =>
                supplier.Id != currentSupplierId
                && supplier.Phone == phone);

            if (duplicatedPhone)
            {
                return "Số điện thoại này đã được dùng cho nhà cung cấp khác.";
            }

            return string.Empty;
        }
        private async Task<BranchManagementViewModel> BuildBranchManagementViewModelAsync(BranchManagementViewModel source = null)
        {
            var branchUsers = await _userManager.Users
                .Where(user => user.BranchId != null)
                .OrderBy(user => user.FullName)
                .ToListAsync();

            var roleLookup = new Dictionary<string, IList<string>>(StringComparer.Ordinal);
            foreach (var branchUser in branchUsers)
            {
                roleLookup[branchUser.Id] = await _userManager.GetRolesAsync(branchUser);
            }

            var branches = await _context.Branches
                .OrderBy(branch => branch.BranchName)
                .Select(branch => new BranchManagementItemViewModel
                {
                    Id = branch.Id,
                    BranchName = branch.BranchName,
                    Address = branch.Address,
                    IsActive = branch.IsActive,
                    ProductCount = branch.Products.Count(),
                    OrderCount = branch.Orders.Count(),
                    ProductPriceInfo = branch.ProductPriceInfo,
                    SizeSelectionInfo = branch.SizeSelectionInfo,
                    WarrantyInfo = branch.WarrantyInfo,
                    TradeInPolicyInfo = branch.TradeInPolicyInfo,
                    OrderProcessInfo = branch.OrderProcessInfo
                })
                .ToListAsync();

            foreach (var branch in branches)
            {
                var members = branchUsers
                    .Where(user => user.BranchId == branch.Id)
                    .ToList();

                branch.OwnerSummary = BuildBranchRoleSummary(members, roleLookup, RoleCatalog.BranchOwner);
                branch.ManagerSummary = BuildBranchRoleSummary(members, roleLookup, RoleCatalog.Manager);
                branch.StaffCount = members.Count(user => roleLookup.TryGetValue(user.Id, out var roles) && roles.Contains(RoleCatalog.Staff));
                branch.CanManageMembers = true;
            }

            var managerOptions = await BuildRoleOptionsAsync(RoleCatalog.Manager, source?.ManagerUserId);
            var warehouseOptions = await _context.Warehouses.AsNoTracking()
                .Include(warehouse => warehouse.Branch)
                .Where(warehouse => warehouse.IsActive)
                .OrderBy(warehouse => warehouse.Branch.BranchName).ThenBy(warehouse => warehouse.Name)
                .Select(warehouse => new SelectListItem
                {
                    Value = warehouse.Id.ToString(),
                    Text = warehouse.Branch.BranchName + " · " + warehouse.Code + " - " + warehouse.Name,
                    Selected = source != null && source.WarehouseId == warehouse.Id
                }).ToListAsync();

            return new BranchManagementViewModel
            {
                BranchName = source?.BranchName ?? string.Empty,
                Address = source?.Address ?? string.Empty,
                ManagerUserId = source?.ManagerUserId ?? string.Empty,
                WarehouseId = source?.WarehouseId,
                ManagerOptions = managerOptions,
                WarehouseOptions = warehouseOptions,
                Branches = branches
            };
        }

        private async Task<BranchTeamViewModel> BuildBranchTeamViewModelAsync(
            Branch branch,
            ActorContext actor,
            IReadOnlyList<string> manageableRoles)
        {
            var users = await _userManager.Users
                .Where(user => user.BranchId == branch.Id)
                .OrderBy(user => user.FullName)
                .ToListAsync();

            var roleLookup = new Dictionary<string, IList<string>>(StringComparer.Ordinal);
            foreach (var user in users)
            {
                roleLookup[user.Id] = await _userManager.GetRolesAsync(user);
            }

            var members = users
                .Select(user =>
                {
                    var highestRole = RoleCatalog.GetHighestRole(roleLookup[user.Id]);
                    return new BranchTeamMemberViewModel
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email ?? string.Empty,
                        Role = highestRole,
                        IsActive = user.IsActive,
                        CanRemove = manageableRoles.Contains(highestRole)
                            && (actor.IsAdmin || !string.Equals(user.Id, actor.User?.Id, StringComparison.Ordinal))
                    };
                })
                .OrderByDescending(item => RoleCatalog.GetPriority(item.Role))
                .ThenBy(item => item.FullName)
                .ToList();

            return new BranchTeamViewModel
            {
                BranchId = branch.Id,
                BranchName = branch.BranchName,
                Address = branch.Address ?? string.Empty,
                IsActive = branch.IsActive,
                CanManageOwners = manageableRoles.Contains(RoleCatalog.BranchOwner),
                CanManageManagers = manageableRoles.Contains(RoleCatalog.Manager),
                CanManageStaff = manageableRoles.Contains(RoleCatalog.Staff),
                ExistingUserOptions = await BuildAssignableUserOptionsAsync(branch.Id, actor, manageableRoles),
                NewMemberRoleOptions = BuildAssignableRoleOptions(manageableRoles),
                NewMemberRole = manageableRoles.FirstOrDefault() ?? string.Empty,
                Members = members
            };
        }

        private async Task<IReadOnlyList<SelectListItem>> BuildRoleOptionsAsync(string roleName, string selectedUserId)
        {
            var users = await _userManager.GetUsersInRoleAsync(roleName);
            return new[]
                {
                    new SelectListItem
                    {
                        Value = string.Empty,
                        Text = "-- Chưa chọn --",
                        Selected = string.IsNullOrWhiteSpace(selectedUserId)
                    }
                }
                .Concat(users
                    .OrderBy(user => user.FullName)
                    .Select(user => new SelectListItem
                    {
                        Value = user.Id,
                        Text = $"{user.FullName} ({user.Email})",
                        Selected = string.Equals(user.Id, selectedUserId, StringComparison.Ordinal)
                    }))
                .ToList();
        }

        private async Task<IReadOnlyList<SelectListItem>> BuildAssignableUserOptionsAsync(
            int branchId,
            ActorContext actor,
            IReadOnlyList<string> manageableRoles)
        {
            var options = new Dictionary<string, SelectListItem>(StringComparer.Ordinal);

            foreach (var role in manageableRoles)
            {
                var users = await _userManager.GetUsersInRoleAsync(role);
                foreach (var user in users)
                {
                    if (!actor.IsAdmin && string.Equals(user.Id, actor.User?.Id, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (user.BranchId == branchId)
                    {
                        continue;
                    }

                    options[user.Id] = new SelectListItem
                    {
                        Value = user.Id,
                        Text = $"{user.FullName} - {RoleCatalog.GetVietnameseLabel(role)} ({user.Email})"
                    };
                }
            }

            return new[]
                {
                    new SelectListItem
                    {
                        Value = string.Empty,
                        Text = "-- Chọn tài khoản có sẵn --",
                        Selected = true
                    }
                }
                .Concat(options.Values.OrderBy(item => item.Text))
                .ToList();
        }

        private static IReadOnlyList<SelectListItem> BuildAssignableRoleOptions(IEnumerable<string> roles)
        {
            return roles
                .Select(role => new SelectListItem
                {
                    Value = role,
                    Text = RoleCatalog.GetVietnameseLabel(role)
                })
                .ToList();
        }

        private async Task<bool> IsEligibleBranchAssigneeAsync(string userId, string requiredRole)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return true;
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            return (await _userManager.GetRolesAsync(user)).Contains(requiredRole);
        }

        private async Task AssignBranchToUserIfSelectedAsync(string userId, int branchId, string requiredRole)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return;
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(requiredRole))
            {
                return;
            }

            user.BranchId = branchId;
            await _userManager.UpdateAsync(user);
        }

        private async Task<ActorContext> GetCurrentActorAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = user == null
                ? new List<string>()
                : await _userManager.GetRolesAsync(user);

            return new ActorContext
            {
                User = user,
                Roles = roles
            };
        }

        private static IReadOnlyList<string> GetManageableBranchRoles(ActorContext actor, int branchId)
        {
            if (actor.IsAdmin)
            {
                return new[] { RoleCatalog.BranchOwner, RoleCatalog.Manager, RoleCatalog.Staff };
            }

            if (actor.IsBranchOwner && actor.User?.BranchId == branchId)
            {
                return new[] { RoleCatalog.Manager, RoleCatalog.Staff };
            }

            if (actor.IsManager && actor.User?.BranchId == branchId)
            {
                return new[] { RoleCatalog.Staff };
            }

            return Array.Empty<string>();
        }

        private static bool CanAssignRole(ActorContext actor, string newRole)
        {
            if (actor.IsAdmin)
            {
                return true;
            }

            if (string.Equals(newRole, RoleCatalog.Admin, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (actor.IsBranchOwner)
            {
                return !string.Equals(newRole, RoleCatalog.BranchOwner, StringComparison.OrdinalIgnoreCase);
            }

            if (actor.IsManager)
            {
                return string.Equals(newRole, RoleCatalog.Staff, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(newRole, RoleCatalog.Accountant, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(newRole, RoleCatalog.Customer, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static bool CanConfirmOrder(ActorContext actor, Order order)
        {
            if (order.Status != Order.StatusPendingConfirmation)
            {
                return false;
            }

            if (actor.IsAdmin || actor.IsManager || actor.IsBranchOwner)
            {
                return true;
            }

            return actor.IsStaff && order.TotalAmount <= 50_000_000m;
        }

        private async Task CancelExpiredDepositOrdersAsync()
        {
            var now = DateTime.UtcNow;
            var expiredOrders = await _context.Orders
                .Include(order => order.OrderDetails)
                    .ThenInclude(detail => detail.Product)
                .Where(order =>
                    (order.Status == Order.StatusAwaitingDepositPayment || order.Status == Order.StatusUnpaidDeposit)
                    && order.DepositDueAt.HasValue
                    && order.DepositDueAt.Value <= now)
                .ToListAsync();

            if (expiredOrders.Count == 0)
            {
                return;
            }

            foreach (var order in expiredOrders)
            {
                order.Status = Order.StatusCancelled;
                order.CancelReason = "Đơn hàng tự hủy vì khách chưa thanh toán cọc trong 1 giờ 30 phút.";
                RestoreProductsForCancelledOrder(order);

                // Notify customer of auto-cancellation
                try
                {
                    var customer = await _userManager.FindByIdAsync(order.UserId);
                    var destination = customer != null && !string.IsNullOrWhiteSpace(customer.Email)
                        ? customer.Email
                        : order.CustomerPhone;

                    if (!string.IsNullOrWhiteSpace(destination))
                    {
                        await _notificationService.SendOrderCancelledDueToNoDepositNotificationAsync(
                            destination,
                            order.CustomerName ?? customer?.FullName ?? "Quý khách",
                            order.OrderNumber);
                    }
                }
                catch (Exception)
                {
                    // Ignore exceptions to avoid breaking background loops
                }
            }

            await _context.SaveChangesAsync();
        }

        private static void RestoreProductsForCancelledOrder(Order order)
        {
            foreach (var detail in order.OrderDetails ?? Enumerable.Empty<OrderDetail>())
            {
                if (detail.Product != null && detail.Product.Status == "Đã bán")
                {
                    detail.Product.Status = "Còn hàng";
                }
            }
        }

        private static string BuildBranchRoleSummary(
            IEnumerable<AppUser> members,
            IReadOnlyDictionary<string, IList<string>> roleLookup,
            string roleName)
        {
            var names = members
                .Where(user => roleLookup.TryGetValue(user.Id, out var roles) && roles.Contains(roleName))
                .Select(user => user.FullName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct()
                .ToList();

            return names.Count == 0 ? "--" : string.Join(", ", names);
        }

        private static string BuildIdentityErrorMessage(IdentityResult result, string fallbackMessage)
        {
            var errorMessages = result.Errors
                .Select(error => error.Description)
                .Where(description => !string.IsNullOrWhiteSpace(description))
                .ToList();

            return errorMessages.Count > 0
                ? string.Join(" ", errorMessages)
                : fallbackMessage;
        }

        private static string NormalizeOrEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private sealed class ActorContext
        {
            public AppUser User { get; set; }
            public IList<string> Roles { get; set; } = new List<string>();
            public bool IsAdmin => Roles.Contains(RoleCatalog.Admin);
            public bool IsBranchOwner => Roles.Contains(RoleCatalog.BranchOwner);
            public bool IsManager => Roles.Contains(RoleCatalog.Manager);
            public bool IsStaff => Roles.Contains(RoleCatalog.Staff);
        }

        private async Task NotifyAuthorizedStaffForPendingOrderAsync(Order order)
        {
            try
            {
                var admins = await _userManager.GetUsersInRoleAsync(RoleCatalog.Admin);
                var owners = await _userManager.GetUsersInRoleAsync(RoleCatalog.BranchOwner);
                var managers = await _userManager.GetUsersInRoleAsync(RoleCatalog.Manager);
                var staff = await _userManager.GetUsersInRoleAsync(RoleCatalog.Staff);

                var targetUsers = new List<AppUser>();
                targetUsers.AddRange(admins);
                targetUsers.AddRange(owners.Where(u => u.BranchId == order.BranchId));
                targetUsers.AddRange(managers.Where(u => u.BranchId == order.BranchId));
                if (order.TotalAmount <= 50_000_000m)
                {
                    targetUsers.AddRange(staff.Where(u => u.BranchId == order.BranchId));
                }

                var uniqueUsers = targetUsers.GroupBy(u => u.Id).Select(g => g.First()).ToList();

                foreach (var u in uniqueUsers)
                {
                    var destination = !string.IsNullOrWhiteSpace(u.Email) ? u.Email : u.PhoneNumber;
                    if (!string.IsNullOrWhiteSpace(destination))
                    {
                        await _notificationService.SendOrderPendingConfirmationNotificationAsync(destination, u.FullName, order);
                    }
                }
            }
            catch (Exception)
            {
                // Ignore errors
            }
        }
        private static string BuildSupplierGoodsReceiptCode()
        {
            var randomPart = Guid.NewGuid()
                .ToString("N")
                .Substring(0, 4)
                .ToUpperInvariant();

            return $"GR-{DateTime.Now:yyyyMMddHHmmssfff}-{randomPart}";
        }

        // ── Cài đặt AI Chatbot ──────────────────────────────────────────────
        [Authorize(Roles = RoleCatalog.Admin)]
        public async Task<IActionResult> ChatSettings()
        {
            var settings = await _context.ChatSettings.FirstOrDefaultAsync()
                           ?? new ChatSettings { Id = 1 };
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleCatalog.Admin)]
        public async Task<IActionResult> ChatSettings(ChatSettings model)
        {
            var existing = await _context.ChatSettings.FirstOrDefaultAsync();
            if (existing == null)
            {
                existing = new ChatSettings { Id = 1 };
                _context.ChatSettings.Add(existing);
            }

            existing.ShopName         = (model.ShopName ?? "KimTon Gold").Trim();
            existing.Hotline          = (model.Hotline ?? "1800 9999").Trim();
            existing.ShopAddress      = (model.ShopAddress ?? string.Empty).Trim();
            existing.ProductPriceInfo = (model.ProductPriceInfo ?? string.Empty).Trim();
            existing.SizeGuideInfo    = (model.SizeGuideInfo ?? string.Empty).Trim();
            existing.WarrantyInfo     = (model.WarrantyInfo ?? string.Empty).Trim();
            existing.ExchangePolicy   = (model.ExchangePolicy ?? string.Empty).Trim();
            existing.OrderProcess     = (model.OrderProcess ?? string.Empty).Trim();
            existing.UpdatedAt        = DateTime.UtcNow;
            existing.UpdatedBy        = User.Identity?.Name ?? "admin";

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã lưu cài đặt chatbot thành công.";
            return RedirectToAction(nameof(ChatSettings));
        }
    }
}
