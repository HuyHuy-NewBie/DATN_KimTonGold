using GoldManagementSystem.Data;
using GoldManagementSystem.Models;
using GoldManagementSystem.Models.ViewModels;
using GoldManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoldManagementSystem.Controllers
{
    public class ProductsController : Controller
    {
        private static readonly string[] GoldCategoriesList =
        {
            "Nhẫn",
            "Nhẫn Cưới",
            "Dây Chuyền",
            "Lắc Tay",
            "Bông Tai",
            "Kiềng cổ",
            "Bộ Sưu Tập"
        };

        private static readonly string[] SilverCategoriesList =
        {
            "Trang Sức Bạc",
            "Nhẫn",
            "Dây Chuyền",
            "Lắc Tay",
            "Bông Tai",
            "Kiềng cổ"
        };

        private static readonly string[] DiamondCategoriesList =
        {
            "Nhẫn Kim Cương",
            "Bông Tai Kim Cương",
            "Dây Chuyền Kim Cương",
            "Mặt Dây Kim Cương",
            "Lắc Tay Kim Cương",
            "Kim Cương Viên"
        };

        private static readonly string[] GoldMaterialsList =
        {
            "Vàng 24K",
            "Vàng 18K",
            "Vàng 9999",
            "Vàng Trắng",
            "Vàng Ý 750"
        };

        private static readonly string[] SilverMaterialsList =
        {
            "Bạc S925",
            "Bạc Ý 925",
            "Bạc Ta",
            "Bạc Thái"
        };

        private static readonly string[] DiamondMaterialList =
        {
            "Kim Cương Thiên Nhiên",
            "Kim Cương Lab Grown",
            "Moissanite"
        };

        private static readonly string[] DiamondShapeList =
        {
            "Round",
            "Princess",
            "Oval",
            "Emerald",
            "Cushion",
            "Pear",
            "Marquise",
            "Heart"
        };

        private static readonly string[] DiamondCutList = { "Excellent", "Very Good", "Good" };
        private static readonly string[] DiamondColorList = { "D", "E", "F", "G", "H", "I", "J" };
        private static readonly string[] DiamondClarityList = { "IF", "VVS1", "VVS2", "VS1", "VS2", "SI1", "SI2" };
        private static readonly string[] DiamondCertificateList = { "GIA", "IGI", "AGS", "Không chứng nhận" };

        private static readonly string[] ProductStatuses =
        {
            "Mới",
            "Còn hàng",
            "Bán chạy",
            "Hết hàng",
            "Đã bán"
        };

        private const string DuplicateActionCreateNew = "CreateNew";
        private const string DuplicateActionUpdateExistingPrefix = "UpdateExisting:";

        private readonly ApplicationDbContext _context;
        private readonly IManagementPermissionService _permissions;

        public ProductsController(ApplicationDbContext context, IManagementPermissionService permissions)
        {
            _context = context;
            _permissions = permissions;
        }

        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public IActionResult AdminIndex(string searchString, string category, string goldType, int? branchId)
        {
            return RedirectToAction(nameof(AdminGold), new { searchString, category, goldType, branchId });
        }

        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult> AdminGold(string searchString, string category, string goldType, int? branchId)
        {
            var query = ApplyCatalogFilters(GoldProducts(_context.Products.Include(product => product.Branch)), searchString, category, goldType, branchId);
            await PopulateAdminSelectionsAsync(ProductLineOptions.Gold, category, goldType, branchId);

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentBranchId = branchId;
            ViewBag.CatalogAdminTitle = "Quản lý sản phẩm vàng";
            ViewBag.CatalogAdminDescription = "Tập trung riêng trang sức vàng, vàng trắng và các mẫu có nền kim loại vàng để thao tác đồng bộ hơn.";
            ViewBag.CatalogAdminCreateLink = "/Products/Create?line=Gold";
            ViewBag.CatalogAdminBasePath = "/Products/AdminGold";
            ViewBag.CatalogAdminEyebrow = "Danh mục vàng";
            ViewBag.CatalogAdminAddLabel = "Thêm sản phẩm vàng";
            ViewBag.CatalogSearchPlaceholder = "Tên, loại vàng, trạng thái...";

            return View("AdminIndex", await OrderProducts(query).ToListAsync());
        }

        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult> AdminSilver(string searchString, string category, string goldType, int? branchId)
        {
            var query = ApplyCatalogFilters(SilverProducts(_context.Products.Include(product => product.Branch)), searchString, category, goldType, branchId);
            await PopulateAdminSelectionsAsync(ProductLineOptions.Silver, category, goldType, branchId);

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentBranchId = branchId;
            ViewBag.CatalogAdminTitle = "Quản lý sản phẩm bạc";
            ViewBag.CatalogAdminDescription = "Hiển thị riêng các mẫu bạc để lọc, tìm kiếm và thêm dữ liệu mà không lẫn với dòng vàng hoặc kim cương.";
            ViewBag.CatalogAdminCreateLink = "/Products/Create?line=Silver";
            ViewBag.CatalogAdminBasePath = "/Products/AdminSilver";
            ViewBag.CatalogAdminEyebrow = "Danh mục bạc";
            ViewBag.CatalogAdminAddLabel = "Thêm sản phẩm bạc";
            ViewBag.CatalogSearchPlaceholder = "Tên, chất liệu bạc, trạng thái...";

            return View("AdminIndex", await OrderProducts(query).ToListAsync());
        }

        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        public async Task<IActionResult> AdminDiamond(
            string searchString,
            string category,
            string goldType,
            string diamondShape,
            string diamondColor,
            string diamondClarity,
            string diamondCut,
            int? branchId)
        {
            var query = ApplyDiamondFilters(
                DiamondProducts(_context.Products.Include(product => product.Branch)),
                searchString,
                category,
                goldType,
                branchId,
                diamondShape,
                diamondColor,
                diamondClarity,
                diamondCut,
                null,
                null,
                null,
                null,
                null,
                null);

            await PopulateAdminSelectionsAsync(ProductLineOptions.Diamond, category, goldType, branchId);
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentBranchId = branchId;
            ViewBag.CurrentDiamondShape = diamondShape;
            ViewBag.CurrentDiamondColor = diamondColor;
            ViewBag.CurrentDiamondClarity = diamondClarity;
            ViewBag.CurrentDiamondCut = diamondCut;
            ViewBag.CatalogAdminTitle = "Quản lý sản phẩm kim cương";
            ViewBag.CatalogAdminDescription = "Trang quản trị kim cương nay dùng cùng khung dashboard để đồng bộ hoàn toàn với các trang quản lý khác.";
            ViewBag.CatalogAdminCreateLink = "/Products/Create?line=Diamond";
            ViewBag.CatalogAdminBasePath = "/Products/AdminDiamond";
            ViewBag.CatalogAdminEyebrow = "Danh mục kim cương";
            ViewBag.CatalogAdminAddLabel = "Thêm sản phẩm kim cương";
            ViewBag.CatalogSearchPlaceholder = "Tên, 4C, chứng nhận, trạng thái...";
            ViewBag.IsDiamondAdmin = true;
            ViewBag.DiamondShapes = new SelectList(DiamondShapeList, diamondShape);
            ViewBag.DiamondCuts = new SelectList(DiamondCutList, diamondCut);
            ViewBag.DiamondColors = new SelectList(DiamondColorList, diamondColor);
            ViewBag.DiamondClarities = new SelectList(DiamondClarityList, diamondClarity);

            return View("AdminIndex", await OrderProducts(query).ToListAsync());
        }

        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        [HttpGet]
        public async Task<IActionResult> Create(string line = null, int? branchId = null)
        {
            var primaryLine = ResolveRequestedLine(line);
            var model = new ProductFormViewModel
            {
                ProductLine = primaryLine,
                CatalogMode = ProductCatalogModeOptions.Single,
                BranchId = branchId ?? await GetDefaultBranchIdAsync(),
                Status = "Còn hàng",
                AssignedProductLines = new List<string> { primaryLine }
            };

            ApplyLineDefaults(model);
            await PopulateProductFormSelectionsAsync(model);
            ViewData["Title"] = BuildProductFormTitle(model, isEditMode: false);
            ViewData["ProductFormMode"] = "create";
            ViewBag.ProductAdminBackUrl = BuildAdminCatalogUrl(primaryLine);
            return View(model);
        }

        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductFormViewModel model)
        {
            if (!await _permissions.CanAsync(User, ManagementFeatureCatalog.ProductsEdit, model.BranchId)) return Forbid();
            NormalizeProductForm(model);
            model.ProductLine = ResolveRequestedLine(model.ProductLine);
            model.CatalogMode = ResolveCatalogMode(model.CatalogMode);
            var assignedLines = ResolveAssignedProductLines(model);
            var duplicateProducts = await FindDuplicateProductsByNamesAsync(new[] { model.Name });

            if (duplicateProducts.Count > 0 && TryParseDuplicateUpdateAction(model.DuplicateAction, out var duplicateProductId))
            {
                var targetProduct = duplicateProducts.FirstOrDefault(product => product.Id == duplicateProductId)
                    ?? duplicateProducts.FirstOrDefault();

                if (targetProduct != null)
                {
                    return RedirectToAction(nameof(Edit), new { id = targetProduct.Id });
                }

                ModelState.AddModelError(nameof(model.Name), "Không xác định được sản phẩm trùng để cập nhật.");
                return await RenderCreateViewAsync(model, assignedLines, duplicateProducts);
            }

            ValidateProductForm(model, assignedLines);
            await ValidateStandardProductFieldsAsync(model);

            if (!await BranchExistsAsync(model.BranchId))
            {
                ModelState.AddModelError(nameof(model.BranchId), "Chi nhánh không hợp lệ.");
            }

            if (!ModelState.IsValid)
            {
                return await RenderCreateViewAsync(model, assignedLines, duplicateProducts);
            }

            if (duplicateProducts.Count > 0)
            {
                if (!string.Equals(model.DuplicateAction, DuplicateActionCreateNew, StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(model.Name), $"Tên sản phẩm \"{model.Name}\" đã tồn tại. Chọn tạo mới hoặc cập nhật sản phẩm đang có.");
                    return await RenderCreateViewAsync(model, assignedLines, duplicateProducts);
                }
            }

            var product = new Product();
            ApplyProductForm(model, assignedLines, product);
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            await SyncCatalogEntriesAsync(product, assignedLines);
            await AddSpecificationVersionAsync(model, product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã thêm sản phẩm {product.Name}.";
            return RedirectToAction(ResolveAdminActionName(ResolveAdminLine(model, assignedLines)));
        }

        private async Task<IActionResult> RenderCreateViewAsync(
            ProductFormViewModel model,
            IReadOnlyCollection<string> assignedLines,
            IReadOnlyList<Product> duplicateProducts = null)
        {
            await PopulateProductFormSelectionsAsync(model);
            ViewData["Title"] = BuildProductFormTitle(model, isEditMode: false);
            ViewData["ProductFormMode"] = "create";
            ViewBag.ProductAdminBackUrl = BuildAdminCatalogUrl(ResolveAdminLine(model, assignedLines));
            ViewBag.DuplicateProducts = duplicateProducts ?? Array.Empty<Product>();
            model.DuplicateAction = string.Empty;
            return View(model);
        }

        private async Task<IReadOnlyList<Product>> FindDuplicateProductsByNamesAsync(IEnumerable<string> candidateNames)
        {
            var normalizedNames = (candidateNames ?? Enumerable.Empty<string>())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name.Trim().ToLower())
                .Distinct()
                .ToList();

            if (normalizedNames.Count == 0)
            {
                return Array.Empty<Product>();
            }

            return await _context.Products
                .Include(product => product.Branch)
                .Where(product => product.Name != null && normalizedNames.Contains(product.Name.ToLower()))
                .OrderByDescending(product => product.CreatedAt)
                .ThenByDescending(product => product.Id)
                .ToListAsync();
        }

        private static bool TryParseDuplicateUpdateAction(string action, out int productId)
        {
            productId = 0;
            if (string.IsNullOrWhiteSpace(action))
            {
                return false;
            }

            if (!action.StartsWith(DuplicateActionUpdateExistingPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var idPart = action.Substring(DuplicateActionUpdateExistingPrefix.Length);
            return int.TryParse(idPart, out productId) && productId > 0;
        }

        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products
                .Include(item => item.GoldCatalogEntry)
                .Include(item => item.SilverCatalogEntry)
                .Include(item => item.DiamondCatalogEntry)
                .Include(item => item.GoldSilverCatalogEntry)
                .Include(item => item.GoldDiamondCatalogEntry)
                .Include(item => item.SilverDiamondCatalogEntry)
                .Include(item => item.SpecificationVersions)
                .FirstOrDefaultAsync(item => item.Id == id);

            if (product == null)
            {
                return NotFound();
            }
            if (!await _permissions.CanAsync(User, ManagementFeatureCatalog.ProductsEdit, product.BranchId)) return Forbid();

            var model = BuildProductForm(product);
            await PopulateProductFormSelectionsAsync(model);
            ViewData["Title"] = BuildProductFormTitle(model, isEditMode: true);
            ViewData["ProductFormMode"] = "edit";
            ViewBag.ProductAdminBackUrl = BuildAdminCatalogUrl(ResolveAdminLine(model, ResolveAssignedProductLines(model)));
            return View(model);
        }

        [Authorize(Roles = RoleCatalog.ManagementRoles)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductFormViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            NormalizeProductForm(model);
            model.ProductLine = ResolveRequestedLine(model.ProductLine);
            model.CatalogMode = ResolveCatalogMode(model.CatalogMode);
            var assignedLines = ResolveAssignedProductLines(model);
            ValidateProductForm(model, assignedLines);
            await ValidateStandardProductFieldsAsync(model);

            var product = await _context.Products
                .Include(item => item.GoldCatalogEntry)
                .Include(item => item.SilverCatalogEntry)
                .Include(item => item.DiamondCatalogEntry)
                .Include(item => item.GoldSilverCatalogEntry)
                .Include(item => item.GoldDiamondCatalogEntry)
                .Include(item => item.SilverDiamondCatalogEntry)
                .FirstOrDefaultAsync(item => item.Id == id);

            if (product == null)
            {
                return NotFound();
            }
            if (!await _permissions.CanAsync(User, ManagementFeatureCatalog.ProductsEdit, product.BranchId)) return Forbid();

            if (!await BranchExistsAsync(model.BranchId))
            {
                ModelState.AddModelError(nameof(model.BranchId), "Chi nhánh không hợp lệ.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateProductFormSelectionsAsync(model);
                ViewData["Title"] = BuildProductFormTitle(model, isEditMode: true);
                ViewData["ProductFormMode"] = "edit";
                ViewBag.ProductAdminBackUrl = BuildAdminCatalogUrl(ResolveAdminLine(model, assignedLines));
                return View(model);
            }

            ApplyProductForm(model, assignedLines, product);
            await SyncCatalogEntriesAsync(product, assignedLines);
            await AddSpecificationVersionAsync(model, product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã cập nhật sản phẩm {product.Name}.";
            return RedirectToAction(ResolveAdminActionName(ResolveAdminLine(model, assignedLines)));
        }

        public async Task<IActionResult> Index(string searchString, string category, string goldType, int? branchId)
        {
            var query = ApplyCatalogFilters(_context.Products.Include(product => product.Branch), searchString, category, goldType, branchId);
            ViewBag.Categories = new SelectList(GoldCategoriesList.Concat(SilverCategoriesList).Concat(DiamondCategoriesList).Distinct().ToArray(), category);
            ViewBag.GoldTypes = new SelectList(GoldMaterialsList.Concat(SilverMaterialsList).Concat(DiamondMaterialList).Distinct().ToArray(), goldType);
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentBranchId = branchId;
            ViewBag.PageTitle = "Tất Cả Sản Phẩm";
            ViewBag.CollectionType = "All";
            ViewBag.SubmitAction = "Index";

            return View(await OrderProducts(query).ToListAsync());
        }

        public async Task<IActionResult> New(string searchString, string category, string goldType, int? branchId)
        {
            var query = ApplyCatalogFilters(_context.Products.Include(product => product.Branch), searchString, category, goldType, branchId);
            ViewBag.Categories = new SelectList(GoldCategoriesList.Concat(SilverCategoriesList).Concat(DiamondCategoriesList).Distinct().ToArray(), category);
            ViewBag.GoldTypes = new SelectList(GoldMaterialsList.Concat(SilverMaterialsList).Concat(DiamondMaterialList).Distinct().ToArray(), goldType);
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentBranchId = branchId;
            ViewBag.PageTitle = "Sản Phẩm Mới";
            ViewBag.CollectionType = "New";
            ViewBag.SubmitAction = "New";
            ViewBag.HighlightNewPriority = true;

            return View("Index", await query.OrderByDescending(product => product.Status == "Mới").ThenByDescending(product => product.CreatedAt).ThenByDescending(product => product.Id).ToListAsync());
        }

        public async Task<IActionResult> Gold(string searchString, string category, string goldType, int? branchId)
        {
            var query = ApplyCatalogFilters(GoldProducts(_context.Products.Include(product => product.Branch)), searchString, category, goldType, branchId);
            ViewBag.Categories = new SelectList(GoldCategoriesList, category);
            ViewBag.GoldTypes = new SelectList(GoldMaterialsList, goldType);
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentBranchId = branchId;
            ViewBag.PageTitle = "Trang Sức Vàng Cao Cấp";
            ViewBag.CollectionType = "Gold";
            ViewBag.SubmitAction = "Gold";

            return View("Index", await OrderProducts(query).ToListAsync());
        }

        public async Task<IActionResult> Silver(string searchString, string category, string goldType, int? branchId)
        {
            var query = ApplyCatalogFilters(SilverProducts(_context.Products.Include(product => product.Branch)), searchString, category, goldType, branchId);
            ViewBag.Categories = new SelectList(SilverCategoriesList, category);
            ViewBag.GoldTypes = new SelectList(SilverMaterialsList, goldType);
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentBranchId = branchId;
            ViewBag.PageTitle = "Trang Sức Bạc Tinh Tế";
            ViewBag.CollectionType = "Silver";
            ViewBag.SubmitAction = "Silver";

            return View("Index", await OrderProducts(query).ToListAsync());
        }

        public async Task<IActionResult> Diamond(
            string searchString,
            string category,
            string goldType,
            int? branchId,
            string diamondShape,
            string diamondCut,
            string diamondColor,
            string diamondClarity,
            decimal? minCarat,
            decimal? maxCarat,
            decimal? minPrice,
            decimal? maxPrice,
            decimal? minSize,
            decimal? maxSize)
        {
            var query = ApplyDiamondFilters(
                DiamondProducts(_context.Products.Include(product => product.Branch)),
                searchString,
                category,
                goldType,
                branchId,
                diamondShape,
                diamondColor,
                diamondClarity,
                diamondCut,
                minCarat,
                maxCarat,
                minPrice,
                maxPrice,
                minSize,
                maxSize);

            ViewBag.Categories = new SelectList(DiamondCategoriesList, category);
            ViewBag.GoldTypes = new SelectList(DiamondMaterialList, goldType);
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentBranchId = branchId;
            ViewBag.CurrentDiamondShape = diamondShape;
            ViewBag.CurrentDiamondCut = diamondCut;
            ViewBag.CurrentDiamondColor = diamondColor;
            ViewBag.CurrentDiamondClarity = diamondClarity;
            ViewBag.CurrentMinCarat = minCarat;
            ViewBag.CurrentMaxCarat = maxCarat;
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;
            ViewBag.CurrentMinSize = minSize;
            ViewBag.CurrentMaxSize = maxSize;
            ViewBag.PageTitle = "Kim Cương";
            ViewBag.CollectionType = "Diamond";
            ViewBag.SubmitAction = "Diamond";
            ViewBag.IsDiamondCollection = true;
            ViewBag.DiamondShapes = new SelectList(DiamondShapeList, diamondShape);
            ViewBag.DiamondCuts = new SelectList(DiamondCutList, diamondCut);
            ViewBag.DiamondColors = new SelectList(DiamondColorList, diamondColor);
            ViewBag.DiamondClarities = new SelectList(DiamondClarityList, diamondClarity);

            return View("Index", await OrderProducts(query).ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(item => item.Branch)
                .Include(item => item.GoldCatalogEntry)
                .Include(item => item.SilverCatalogEntry)
                .Include(item => item.DiamondCatalogEntry)
                .Include(item => item.GoldSilverCatalogEntry)
                .Include(item => item.GoldDiamondCatalogEntry)
                .Include(item => item.SilverDiamondCatalogEntry)
                .FirstOrDefaultAsync(item => item.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var assignedLines = ResolveAssignedProductLines(product);
            var isDiamond = assignedLines.Contains(ProductLineOptions.Diamond)
                || (!string.IsNullOrWhiteSpace(product.DiamondShape) && product.DiamondCarat.HasValue);

            var relatedQuery = isDiamond
                ? DiamondProducts(_context.Products.Where(item => item.Id != id))
                    .Where(item => item.Category == product.Category || item.DiamondShape == product.DiamondShape || item.GoldType == product.GoldType)
                : ApplyCatalogFilters(
                    _context.Products.Where(item => item.Id != id),
                    null,
                    product.Category,
                    product.GoldType,
                    null);

            ViewBag.RelatedProducts = await relatedQuery.OrderByDescending(item => item.CreatedAt).Take(4).ToListAsync();
            return View(product);
        }

        private IQueryable<Product> GoldProducts(IQueryable<Product> query)
        {
            var directIds = _context.GoldProductCatalogEntries.Select(item => item.ProductId);
            var comboIds = _context.GoldSilverProductCatalogEntries.Select(item => item.ProductId)
                .Concat(_context.GoldDiamondProductCatalogEntries.Select(item => item.ProductId));

            return query.Where(product =>
                directIds.Contains(product.Id)
                || comboIds.Contains(product.Id)
                || product.ProductLine == ProductLineOptions.Gold
                || ((product.ProductLine == null || product.ProductLine == string.Empty)
                    && product.Category != "Trang Sức Bạc"
                    && (product.GoldType == null || (!product.GoldType.Contains("Bạc") && !product.GoldType.Contains("Kim Cương") && !product.GoldType.Contains("Moissanite")))
                    && (product.Name == null || (!product.Name.Contains("Kim Cương") && !product.Name.Contains("Moissanite")))));
        }

        private IQueryable<Product> SilverProducts(IQueryable<Product> query)
        {
            var directIds = _context.SilverProductCatalogEntries.Select(item => item.ProductId);
            var comboIds = _context.GoldSilverProductCatalogEntries.Select(item => item.ProductId)
                .Concat(_context.SilverDiamondProductCatalogEntries.Select(item => item.ProductId));

            return query.Where(product =>
                directIds.Contains(product.Id)
                || comboIds.Contains(product.Id)
                || product.ProductLine == ProductLineOptions.Silver
                || ((product.ProductLine == null || product.ProductLine == string.Empty)
                    && ((product.GoldType != null && product.GoldType.Contains("Bạc")) || product.Category == "Trang Sức Bạc")));
        }

        private IQueryable<Product> DiamondProducts(IQueryable<Product> query)
        {
            var directIds = _context.DiamondProductCatalogEntries.Select(item => item.ProductId);
            var comboIds = _context.GoldDiamondProductCatalogEntries.Select(item => item.ProductId)
                .Concat(_context.SilverDiamondProductCatalogEntries.Select(item => item.ProductId));

            return query.Where(product =>
                directIds.Contains(product.Id)
                || comboIds.Contains(product.Id)
                || product.ProductLine == ProductLineOptions.Diamond
                || product.Category == "Kim Cương"
                || product.DiamondCarat.HasValue
                || product.DiamondSize.HasValue
                || !string.IsNullOrWhiteSpace(product.DiamondShape)
                || (product.GoldType != null && (product.GoldType.Contains("Kim Cương") || product.GoldType.Contains("Moissanite") || product.GoldType.Contains("Cubic")))
                || (product.Name != null && (product.Name.Contains("Kim Cương") || product.Name.Contains("Moissanite") || product.Name.Contains("Cubic"))));
        }

        private static IQueryable<Product> ApplyCatalogFilters(IQueryable<Product> query, string searchString, string category, string goldType, int? branchId)
        {
            query = query.Where(product => product.Status != "Đã xóa");
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(product =>
                    product.Name.Contains(searchString)
                    || product.Category.Contains(searchString)
                    || product.GoldType.Contains(searchString)
                    || product.Status.Contains(searchString)
                    || (product.Description != null && product.Description.Contains(searchString))
                    || (product.DiamondShape != null && product.DiamondShape.Contains(searchString))
                    || (product.DiamondCut != null && product.DiamondCut.Contains(searchString))
                    || (product.DiamondColor != null && product.DiamondColor.Contains(searchString))
                    || (product.DiamondClarity != null && product.DiamondClarity.Contains(searchString))
                    || (product.DiamondCertificate != null && product.DiamondCertificate.Contains(searchString))
                    || (product.Branch != null && product.Branch.BranchName.Contains(searchString)));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(product => product.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(goldType))
            {
                query = query.Where(product => product.GoldType == goldType);
            }

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(product => product.BranchId == branchId.Value);
            }

            return query;
        }

        private static IOrderedQueryable<Product> OrderProducts(IQueryable<Product> query) => query
            .OrderByDescending(product => product.IsPriority)
            .ThenBy(product => product.PriorityOrder)
            .ThenByDescending(product => product.CreatedAt)
            .ThenByDescending(product => product.Id);

        private static IQueryable<Product> ApplyDiamondFilters(
            IQueryable<Product> query,
            string searchString,
            string category,
            string goldType,
            int? branchId,
            string diamondShape,
            string diamondColor,
            string diamondClarity,
            string diamondCut,
            decimal? minCarat,
            decimal? maxCarat,
            decimal? minPrice,
            decimal? maxPrice,
            decimal? minSize,
            decimal? maxSize)
        {
            query = ApplyCatalogFilters(query, searchString, category, goldType, branchId);

            if (!string.IsNullOrWhiteSpace(diamondShape))
            {
                query = query.Where(product => product.DiamondShape == diamondShape);
            }

            if (!string.IsNullOrWhiteSpace(diamondColor))
            {
                query = query.Where(product => product.DiamondColor == diamondColor);
            }

            if (!string.IsNullOrWhiteSpace(diamondClarity))
            {
                query = query.Where(product => product.DiamondClarity == diamondClarity);
            }

            if (!string.IsNullOrWhiteSpace(diamondCut))
            {
                query = query.Where(product => product.DiamondCut == diamondCut);
            }

            if (minCarat.HasValue)
            {
                query = query.Where(product => product.DiamondCarat >= minCarat.Value);
            }

            if (maxCarat.HasValue)
            {
                query = query.Where(product => product.DiamondCarat <= maxCarat.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(product => product.SellPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(product => product.SellPrice <= maxPrice.Value);
            }

            if (minSize.HasValue)
            {
                query = query.Where(product => product.DiamondSize >= minSize.Value);
            }

            if (maxSize.HasValue)
            {
                query = query.Where(product => product.DiamondSize <= maxSize.Value);
            }

            return query;
        }

        private static ProductFormViewModel BuildProductForm(Product product)
        {
            var images = product.GalleryImages.Take(3).ToList();
            var assignedLines = ResolveAssignedProductLines(product);
            var primaryLine = ResolvePrimaryLine(product.ProductLine, assignedLines);
            var latestSpecification = product.SpecificationVersions?
                .OrderByDescending(version => version.EffectiveFrom)
                .ThenByDescending(version => version.Id)
                .FirstOrDefault();

            return new ProductFormViewModel
            {
                Id = product.Id,
                ProductLine = primaryLine,
                CatalogMode = assignedLines.Count > 1 ? ProductCatalogModeOptions.Multi : ProductCatalogModeOptions.Single,
                AssignedProductLines = assignedLines.ToList(),
                Name = product.Name,
                Category = product.Category,
                GoldType = product.GoldType,
                Material = latestSpecification?.Material ?? (string.IsNullOrWhiteSpace(product.Material) ? product.ProductLine : product.Material),
                ProductForm = latestSpecification?.ProductForm ?? (string.IsNullOrWhiteSpace(product.ProductForm) ? ProductFormOptions.Jewelry : product.ProductForm),
                ProductLegalClass = latestSpecification?.ProductLegalClass ?? (string.IsNullOrWhiteSpace(product.ProductLegalClass) ? ProductLegalClassOptions.GoldJewelry : product.ProductLegalClass),
                PurityDefinitionId = latestSpecification?.PurityDefinitionId ?? product.PurityDefinitionId,
                PurityRate = latestSpecification?.PurityRate ?? product.PurityRate,
                UnitOfMeasure = latestSpecification?.UnitOfMeasure ?? (string.IsNullOrWhiteSpace(product.UnitOfMeasure) ? ProductUnitOfMeasureOptions.Piece : product.UnitOfMeasure),
                SpecificationVersion = latestSpecification?.Version ?? "1.0",
                Weight = product.Weight,
                ProcessingFee = product.ProcessingFee,
                SellPrice = product.SellPrice,
                BuyPrice = product.BuyPrice,
                BranchId = product.BranchId,
                Status = product.Status,
                Description = product.Description,
                DiamondShape = product.DiamondShape,
                DiamondCut = product.DiamondCut,
                DiamondColor = product.DiamondColor,
                DiamondClarity = product.DiamondClarity,
                DiamondCarat = product.DiamondCarat,
                DiamondSize = product.DiamondSize,
                DiamondCertificate = product.DiamondCertificate,
                ImageUrl1 = images.ElementAtOrDefault(0) ?? string.Empty,
                ImageUrl2 = images.ElementAtOrDefault(1) ?? string.Empty,
                ImageUrl3 = images.ElementAtOrDefault(2) ?? string.Empty
            };
        }

        private static void ApplyProductForm(ProductFormViewModel model, IReadOnlyCollection<string> assignedLines, Product product)
        {
            product.ProductLine = ResolvePrimaryLine(model.ProductLine, assignedLines);
            product.Name = model.Name;
            product.Category = model.Category;
            product.GoldType = model.GoldType;
            product.Material = model.Material;
            product.ProductForm = model.ProductForm;
            product.ProductLegalClass = model.ProductLegalClass;
            product.PurityDefinitionId = model.PurityDefinitionId;
            product.PurityRate = model.PurityRate;
            product.UnitOfMeasure = model.UnitOfMeasure;
            product.Weight = model.Weight;
            product.ProcessingFee = model.ProcessingFee;
            product.SellPrice = model.SellPrice;
            product.BuyPrice = model.BuyPrice;
            product.BranchId = model.BranchId;
            product.ImagesUrl = ComposeImagesUrl(model);
            product.Status = string.IsNullOrWhiteSpace(model.Status) ? "Còn hàng" : model.Status;
            product.Description = NullIfWhiteSpace(model.Description);

            if (assignedLines.Contains(ProductLineOptions.Diamond))
            {
                product.DiamondShape = NullIfWhiteSpace(model.DiamondShape);
                product.DiamondCut = NullIfWhiteSpace(model.DiamondCut);
                product.DiamondColor = NullIfWhiteSpace(model.DiamondColor);
                product.DiamondClarity = NullIfWhiteSpace(model.DiamondClarity);
                product.DiamondCarat = model.DiamondCarat;
                product.DiamondSize = model.DiamondSize;
                product.DiamondCertificate = NullIfWhiteSpace(model.DiamondCertificate);
            }
            else
            {
                product.DiamondShape = null;
                product.DiamondCut = null;
                product.DiamondColor = null;
                product.DiamondClarity = null;
                product.DiamondCarat = null;
                product.DiamondSize = null;
                product.DiamondCertificate = null;
            }
        }

        private static void NormalizeProductForm(ProductFormViewModel model)
        {
            model.Name = model.Name?.Trim() ?? string.Empty;
            model.Category = model.Category?.Trim() ?? string.Empty;
            model.GoldType = model.GoldType?.Trim() ?? string.Empty;
            model.Material = model.Material?.Trim() ?? string.Empty;
            model.ProductForm = model.ProductForm?.Trim() ?? string.Empty;
            model.ProductLegalClass = model.ProductLegalClass?.Trim() ?? string.Empty;
            model.UnitOfMeasure = model.UnitOfMeasure?.Trim() ?? string.Empty;
            model.SpecificationVersion = model.SpecificationVersion?.Trim() ?? string.Empty;
            model.SpecificationChangeReason = model.SpecificationChangeReason?.Trim() ?? string.Empty;
            model.Status = model.Status?.Trim() ?? "Còn hàng";
            model.Description = model.Description?.Trim() ?? string.Empty;
            model.DiamondShape = model.DiamondShape?.Trim() ?? string.Empty;
            model.DiamondCut = model.DiamondCut?.Trim() ?? string.Empty;
            model.DiamondColor = model.DiamondColor?.Trim() ?? string.Empty;
            model.DiamondClarity = model.DiamondClarity?.Trim() ?? string.Empty;
            model.DiamondCertificate = model.DiamondCertificate?.Trim() ?? string.Empty;
            model.ImageUrl1 = model.ImageUrl1?.Trim() ?? string.Empty;
            model.ImageUrl2 = model.ImageUrl2?.Trim() ?? string.Empty;
            model.ImageUrl3 = model.ImageUrl3?.Trim() ?? string.Empty;
            model.DuplicateAction = model.DuplicateAction?.Trim() ?? string.Empty;
            model.AssignedProductLines = (model.AssignedProductLines ?? new List<string>())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(ResolveRequestedLine)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private void ValidateProductForm(ProductFormViewModel model, IReadOnlyCollection<string> assignedLines)
        {
            if (ResolveCatalogMode(model.CatalogMode) == ProductCatalogModeOptions.Multi && assignedLines.Count < 2)
            {
                ModelState.AddModelError(nameof(model.AssignedProductLines), "Sản phẩm kết hợp cần chọn đúng 2 thể loại hiển thị.");
            }

            if (assignedLines.Count > 2)
            {
                ModelState.AddModelError(nameof(model.AssignedProductLines), "Mỗi sản phẩm chỉ được thuộc tối đa 2 thể loại để khớp cấu trúc bảng catalog.");
            }

            if (assignedLines.Count == 0)
            {
                ModelState.AddModelError(nameof(model.ProductLine), "Vui lòng chọn ít nhất một thể loại sản phẩm.");
            }

            if (!new[] { ProductMaterialOptions.Gold, ProductMaterialOptions.Silver, ProductMaterialOptions.Diamond }.Contains(model.Material, StringComparer.OrdinalIgnoreCase)) ModelState.AddModelError(nameof(model.Material), "Vật liệu không hợp lệ.");
            if (!new[] { ProductFormOptions.Bar, ProductFormOptions.Jewelry, ProductFormOptions.RawMaterial, ProductFormOptions.FinishedGood }.Contains(model.ProductForm, StringComparer.OrdinalIgnoreCase)) ModelState.AddModelError(nameof(model.ProductForm), "Dạng hàng không hợp lệ.");
            if (!new[] { ProductUnitOfMeasureOptions.Piece, ProductUnitOfMeasureOptions.Gram, ProductUnitOfMeasureOptions.Tael }.Contains(model.UnitOfMeasure, StringComparer.OrdinalIgnoreCase)) ModelState.AddModelError(nameof(model.UnitOfMeasure), "Đơn vị tính không hợp lệ.");
            if (string.IsNullOrWhiteSpace(model.SpecificationVersion)) ModelState.AddModelError(nameof(model.SpecificationVersion), "Phiên bản thông số là bắt buộc.");

            if (assignedLines.Contains(ProductLineOptions.Diamond))
            {
                if (string.IsNullOrWhiteSpace(model.DiamondShape))
                {
                    ModelState.AddModelError(nameof(model.DiamondShape), "Vui lòng chọn hình dạng kim cương.");
                }

                if (!model.DiamondCarat.HasValue && !model.DiamondSize.HasValue)
                {
                    ModelState.AddModelError(nameof(model.DiamondCarat), "Vui lòng nhập carat hoặc kích thước ly.");
                }
            }
        }

        private async Task SyncCatalogEntriesAsync(Product product, IReadOnlyCollection<string> assignedLines)
        {
            var normalizedLines = assignedLines
                .Select(ResolveRequestedLine)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            await SyncSingleCatalogEntryAsync(product.Id, normalizedLines.Count == 1 && normalizedLines.Contains(ProductLineOptions.Gold), _context.GoldProductCatalogEntries);
            await SyncSingleCatalogEntryAsync(product.Id, normalizedLines.Count == 1 && normalizedLines.Contains(ProductLineOptions.Silver), _context.SilverProductCatalogEntries);
            await SyncSingleCatalogEntryAsync(product.Id, normalizedLines.Count == 1 && normalizedLines.Contains(ProductLineOptions.Diamond), _context.DiamondProductCatalogEntries);
            await SyncSingleCatalogEntryAsync(product.Id, IsCatalogPair(normalizedLines, ProductLineOptions.Gold, ProductLineOptions.Silver), _context.GoldSilverProductCatalogEntries);
            await SyncSingleCatalogEntryAsync(product.Id, IsCatalogPair(normalizedLines, ProductLineOptions.Gold, ProductLineOptions.Diamond), _context.GoldDiamondProductCatalogEntries);
            await SyncSingleCatalogEntryAsync(product.Id, IsCatalogPair(normalizedLines, ProductLineOptions.Silver, ProductLineOptions.Diamond), _context.SilverDiamondProductCatalogEntries);
        }

        private async Task SyncSingleCatalogEntryAsync<TEntry>(int productId, bool shouldExist, DbSet<TEntry> dbSet)
            where TEntry : class, new()
        {
            var existing = await dbSet.FindAsync(productId);
            if (shouldExist && existing == null)
            {
                var entry = new TEntry();
                typeof(TEntry).GetProperty("ProductId")?.SetValue(entry, productId);
                dbSet.Add(entry);
            }
            else if (!shouldExist && existing != null)
            {
                dbSet.Remove(existing);
            }
        }

        private async Task PopulateProductFormSelectionsAsync(ProductFormViewModel model)
        {
            var primaryLine = ResolveRequestedLine(model.ProductLine);
            var branches = await _context.Branches
                .Where(branch => branch.IsActive)
                .OrderBy(branch => branch.BranchName)
                .ToListAsync();

            ViewBag.ProductBranches = new SelectList(branches, nameof(Branch.Id), nameof(Branch.BranchName), model.BranchId);
            ViewBag.ProductLines = new SelectList(new[]
            {
                new SelectListItem { Value = ProductLineOptions.Gold, Text = "Trang sức vàng" },
                new SelectListItem { Value = ProductLineOptions.Silver, Text = "Trang sức bạc" },
                new SelectListItem { Value = ProductLineOptions.Diamond, Text = "Kim cương" }
            }, nameof(SelectListItem.Value), nameof(SelectListItem.Text), primaryLine);
            ViewBag.ProductCatalogModes = new SelectList(new[]
            {
                new SelectListItem { Value = ProductCatalogModeOptions.Single, Text = "Một thể loại" },
                new SelectListItem { Value = ProductCatalogModeOptions.Multi, Text = "Hai thể loại kết hợp" }
            }, nameof(SelectListItem.Value), nameof(SelectListItem.Text), ResolveCatalogMode(model.CatalogMode));
            ViewBag.ProductAssignedLines = ProductLineOptions.All
                .Select(line => new SelectListItem
                {
                    Value = line,
                    Text = GetProductLineLabel(line),
                    Selected = model.AssignedProductLines.Any(item => string.Equals(item, line, StringComparison.OrdinalIgnoreCase))
                })
                .ToList();
            ViewBag.ProductCategories = new SelectList(GetCategoriesForLine(primaryLine), model.Category);
            ViewBag.ProductGoldTypes = new SelectList(GetMaterialsForLine(primaryLine), model.GoldType);
            ViewBag.ProductMaterials = new SelectList(new[] { new SelectListItem("Vàng", ProductMaterialOptions.Gold), new SelectListItem("Bạc", ProductMaterialOptions.Silver), new SelectListItem("Kim cương (ngoài phạm vi vàng bạc)", ProductMaterialOptions.Diamond) }, nameof(SelectListItem.Value), nameof(SelectListItem.Text), model.Material);
            ViewBag.ProductForms = new SelectList(new[] { new SelectListItem("Miếng", ProductFormOptions.Bar), new SelectListItem("Trang sức", ProductFormOptions.Jewelry), new SelectListItem("Nguyên liệu", ProductFormOptions.RawMaterial), new SelectListItem("Thành phẩm", ProductFormOptions.FinishedGood) }, nameof(SelectListItem.Value), nameof(SelectListItem.Text), model.ProductForm);
            ViewBag.ProductLegalClasses = new SelectList(new[] { ProductLegalClassOptions.GoldBarRegulated, ProductLegalClassOptions.GoldJewelry, ProductLegalClassOptions.GoldRawMaterial, ProductLegalClassOptions.SilverCommodity, ProductLegalClassOptions.SilverJewelry, ProductLegalClassOptions.SilverRawMaterial, ProductLegalClassOptions.DiamondExcluded }, model.ProductLegalClass);
            ViewBag.ProductUnits = new SelectList(new[] { new SelectListItem("Món", ProductUnitOfMeasureOptions.Piece), new SelectListItem("Gram", ProductUnitOfMeasureOptions.Gram), new SelectListItem("Chỉ/lượng", ProductUnitOfMeasureOptions.Tael) }, nameof(SelectListItem.Value), nameof(SelectListItem.Text), model.UnitOfMeasure);
            ViewBag.ProductPurities = new SelectList(await _context.PurityDefinitions.Where(item => item.IsActive && (item.Material == model.Material || item.Material == ProductMaterialOptions.Diamond)).OrderBy(item => item.Rate).ToListAsync(), nameof(PurityDefinition.Id), nameof(PurityDefinition.DisplayName), model.PurityDefinitionId);
            ViewBag.ProductStatuses = new SelectList(ProductStatuses, model.Status);
            ViewBag.ProductDiamondShapes = new SelectList(DiamondShapeList, model.DiamondShape);
            ViewBag.ProductDiamondCuts = new SelectList(DiamondCutList, model.DiamondCut);
            ViewBag.ProductDiamondColors = new SelectList(DiamondColorList, model.DiamondColor);
            ViewBag.ProductDiamondClarities = new SelectList(DiamondClarityList, model.DiamondClarity);
            ViewBag.ProductDiamondCertificates = new SelectList(DiamondCertificateList, model.DiamondCertificate);
        }

        private async Task PopulateAdminSelectionsAsync(string line, string category, string goldType, int? branchId)
        {
            ViewBag.Categories = new SelectList(GetCategoriesForLine(line), category);
            ViewBag.GoldTypes = new SelectList(GetMaterialsForLine(line), goldType);
            ViewBag.Branches = new SelectList(
                await _context.Branches.OrderBy(branch => branch.BranchName).ToListAsync(),
                nameof(Branch.Id),
                nameof(Branch.BranchName),
                branchId);
        }

        private async Task<int> GetDefaultBranchIdAsync()
        {
            return await _context.Branches
                .Where(branch => branch.IsActive)
                .OrderBy(branch => branch.BranchName)
                .Select(branch => branch.Id)
                .FirstOrDefaultAsync();
        }

        private async Task<bool> BranchExistsAsync(int branchId)
        {
            return await _context.Branches.AnyAsync(branch => branch.Id == branchId && branch.IsActive);
        }

        private static string ResolveRequestedLine(string line)
        {
            if (string.Equals(line, ProductLineOptions.Diamond, StringComparison.OrdinalIgnoreCase))
            {
                return ProductLineOptions.Diamond;
            }

            if (string.Equals(line, ProductLineOptions.Silver, StringComparison.OrdinalIgnoreCase))
            {
                return ProductLineOptions.Silver;
            }

            return ProductLineOptions.Gold;
        }

        private static string ResolveCatalogMode(string mode)
        {
            return string.Equals(mode, ProductCatalogModeOptions.Multi, StringComparison.OrdinalIgnoreCase)
                ? ProductCatalogModeOptions.Multi
                : ProductCatalogModeOptions.Single;
        }

        private static IReadOnlyList<string> ResolveAssignedProductLines(ProductFormViewModel model)
        {
            var primaryLine = ResolveRequestedLine(model.ProductLine);
            if (ResolveCatalogMode(model.CatalogMode) == ProductCatalogModeOptions.Single)
            {
                return new[] { primaryLine };
            }

            var selectedLines = (model.AssignedProductLines ?? new List<string>())
                .Select(ResolveRequestedLine)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!selectedLines.Contains(primaryLine))
            {
                selectedLines.Insert(0, primaryLine);
            }

            return selectedLines;
        }

        private static IReadOnlyList<string> ResolveAssignedProductLines(Product product)
        {
            if (product.GoldSilverCatalogEntry != null)
            {
                return new[] { ProductLineOptions.Gold, ProductLineOptions.Silver };
            }

            if (product.GoldDiamondCatalogEntry != null)
            {
                return new[] { ProductLineOptions.Gold, ProductLineOptions.Diamond };
            }

            if (product.SilverDiamondCatalogEntry != null)
            {
                return new[] { ProductLineOptions.Silver, ProductLineOptions.Diamond };
            }

            if (product.GoldCatalogEntry != null)
            {
                return new[] { ProductLineOptions.Gold };
            }

            if (product.SilverCatalogEntry != null)
            {
                return new[] { ProductLineOptions.Silver };
            }

            if (product.DiamondCatalogEntry != null)
            {
                return new[] { ProductLineOptions.Diamond };
            }

            return new[] { ResolveRequestedLine(product.ProductLine) };
        }

        private static string ResolvePrimaryLine(string primaryLine, IReadOnlyCollection<string> assignedLines)
        {
            var normalizedPrimary = ResolveRequestedLine(primaryLine);
            if (assignedLines.Contains(normalizedPrimary))
            {
                return normalizedPrimary;
            }

            return assignedLines.FirstOrDefault() ?? ProductLineOptions.Gold;
        }

        private static string ResolveAdminLine(ProductFormViewModel model, IReadOnlyCollection<string> assignedLines)
        {
            return ResolvePrimaryLine(model.ProductLine, assignedLines);
        }

        private static string ResolveAdminActionName(string line)
        {
            return line switch
            {
                ProductLineOptions.Silver => nameof(AdminSilver),
                ProductLineOptions.Diamond => nameof(AdminDiamond),
                _ => nameof(AdminGold)
            };
        }

        private static string BuildAdminCatalogUrl(string line)
        {
            return line switch
            {
                ProductLineOptions.Silver => "/Products/AdminSilver",
                ProductLineOptions.Diamond => "/Products/AdminDiamond",
                _ => "/Products/AdminGold"
            };
        }

        private static string BuildProductFormTitle(ProductFormViewModel model, bool isEditMode)
        {
            var verb = isEditMode ? "Cập nhật" : "Thêm";
            return ResolveCatalogMode(model.CatalogMode) == ProductCatalogModeOptions.Multi
                ? $"{verb} sản phẩm kết hợp"
                : model.ProductLine switch
                {
                    ProductLineOptions.Silver => $"{verb} sản phẩm bạc",
                    ProductLineOptions.Diamond => $"{verb} sản phẩm kim cương",
                    _ => $"{verb} sản phẩm vàng"
                };
        }

        private static string[] GetCategoriesForLine(string line)
        {
            return ResolveRequestedLine(line) switch
            {
                ProductLineOptions.Silver => SilverCategoriesList,
                ProductLineOptions.Diamond => DiamondCategoriesList,
                _ => GoldCategoriesList
            };
        }

        private static string[] GetMaterialsForLine(string line)
        {
            return ResolveRequestedLine(line) switch
            {
                ProductLineOptions.Silver => SilverMaterialsList,
                ProductLineOptions.Diamond => DiamondMaterialList,
                _ => GoldMaterialsList
            };
        }

        private static string GetProductLineLabel(string line)
        {
            return ResolveRequestedLine(line) switch
            {
                ProductLineOptions.Silver => "Trang sức bạc",
                ProductLineOptions.Diamond => "Kim cương",
                _ => "Trang sức vàng"
            };
        }

        private static void ApplyLineDefaults(ProductFormViewModel model)
        {
            var primaryLine = ResolveRequestedLine(model.ProductLine);
            model.ProductLine = primaryLine;

            if (primaryLine == ProductLineOptions.Diamond)
            {
                model.Material = ProductMaterialOptions.Diamond;
                model.ProductLegalClass = ProductLegalClassOptions.DiamondExcluded;
                model.PurityDefinitionId = 6;
                model.Category = DiamondCategoriesList.First();
                model.GoldType = DiamondMaterialList.First();
                model.DiamondShape = DiamondShapeList.First();
                model.DiamondCut = DiamondCutList.First();
                model.DiamondColor = DiamondColorList[2];
                model.DiamondClarity = DiamondClarityList[3];
                model.DiamondCertificate = DiamondCertificateList.First();
                return;
            }

            model.Category = primaryLine == ProductLineOptions.Silver ? SilverCategoriesList.First() : GoldCategoriesList.First();
            model.GoldType = primaryLine == ProductLineOptions.Silver ? SilverMaterialsList.First() : GoldMaterialsList[1];
            model.Material = primaryLine == ProductLineOptions.Silver ? ProductMaterialOptions.Silver : ProductMaterialOptions.Gold;
            model.ProductLegalClass = primaryLine == ProductLineOptions.Silver ? ProductLegalClassOptions.SilverJewelry : ProductLegalClassOptions.GoldJewelry;
            model.PurityDefinitionId = model.Material == ProductMaterialOptions.Silver ? 5 : 1;
        }

        private async Task ValidateStandardProductFieldsAsync(ProductFormViewModel model)
        {
            var legalClasses = new[] { ProductLegalClassOptions.GoldBarRegulated, ProductLegalClassOptions.GoldJewelry, ProductLegalClassOptions.GoldRawMaterial, ProductLegalClassOptions.SilverCommodity, ProductLegalClassOptions.SilverJewelry, ProductLegalClassOptions.SilverRawMaterial, ProductLegalClassOptions.DiamondExcluded };
            if (!legalClasses.Contains(model.ProductLegalClass, StringComparer.OrdinalIgnoreCase)) ModelState.AddModelError(nameof(model.ProductLegalClass), "Nhóm pháp lý không hợp lệ.");
            if (model.PurityDefinitionId is not int purityId) { ModelState.AddModelError(nameof(model.PurityDefinitionId), "Phải chọn hàm lượng chuẩn."); return; }
            var purity = await _context.PurityDefinitions.FirstOrDefaultAsync(item => item.Id == purityId && item.IsActive);
            if (purity == null) { ModelState.AddModelError(nameof(model.PurityDefinitionId), "Hàm lượng chuẩn không tồn tại hoặc đã ngừng dùng."); return; }
            if (!string.Equals(purity.Material, model.Material, StringComparison.OrdinalIgnoreCase)) ModelState.AddModelError(nameof(model.PurityDefinitionId), "Hàm lượng không thuộc vật liệu đã chọn.");
            model.PurityRate = purity.Rate;
            model.GoldType = purity.DisplayName;
            var validLegalClass = model.Material switch
            {
                ProductMaterialOptions.Gold => new[] { ProductLegalClassOptions.GoldBarRegulated, ProductLegalClassOptions.GoldJewelry, ProductLegalClassOptions.GoldRawMaterial },
                ProductMaterialOptions.Silver => new[] { ProductLegalClassOptions.SilverCommodity, ProductLegalClassOptions.SilverJewelry, ProductLegalClassOptions.SilverRawMaterial },
                _ => new[] { ProductLegalClassOptions.DiamondExcluded }
            };
            if (!validLegalClass.Contains(model.ProductLegalClass, StringComparer.OrdinalIgnoreCase)) ModelState.AddModelError(nameof(model.ProductLegalClass), "Nhóm pháp lý không khớp vật liệu.");
        }

        private async Task AddSpecificationVersionAsync(ProductFormViewModel model, Product product)
        {
            var version = model.SpecificationVersion;
            if (await _context.ProductSpecVersions.AnyAsync(item => item.ProductId == product.Id && item.Version == version)) version = $"{version}-{DateTime.UtcNow:yyyyMMddHHmmss}";
            _context.ProductSpecVersions.Add(new ProductSpecVersion { ProductId = product.Id, Version = version, Material = product.Material, ProductForm = product.ProductForm, ProductLegalClass = product.ProductLegalClass, PurityDefinitionId = product.PurityDefinitionId, PurityRate = product.PurityRate, UnitOfMeasure = product.UnitOfMeasure, GrossWeight = product.Weight, FineWeight = product.Weight * product.PurityRate, CreatedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty, EffectiveFrom = DateTime.UtcNow, ChangeReason = string.IsNullOrWhiteSpace(model.SpecificationChangeReason) ? "Khởi tạo/cập nhật thông số sản phẩm" : model.SpecificationChangeReason });
        }

        private static string ComposeImagesUrl(ProductFormViewModel model)
        {
            return string.Join(
                ";",
                new[] { model.ImageUrl1, model.ImageUrl2, model.ImageUrl3 }
                    .Where(image => !string.IsNullOrWhiteSpace(image))
                    .Select(image => image.Trim()));
        }

        private static string NullIfWhiteSpace(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static bool IsCatalogPair(IReadOnlyCollection<string> lines, string first, string second)
        {
            return lines.Count == 2
                && lines.Contains(first)
                && lines.Contains(second);
        }
    }
}
