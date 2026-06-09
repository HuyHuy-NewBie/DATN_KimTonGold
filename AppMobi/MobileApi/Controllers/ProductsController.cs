using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobileApi.Contracts;
using MobileApi.Data;
using MobileApi.Models;
using MobileApi.Services;

namespace MobileApi.Controllers;

[ApiController]
[Route("api/products")]
[Authorize(Policy = Policies.ProductWrite)]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly PermissionService _permissionService;

    public ProductsController(ApplicationDbContext context, PermissionService permissionService)
    {
        _context = context;
        _permissionService = permissionService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetProducts(
        string? search = null,
        string? line = null,
        string? category = null,
        string? status = null,
        int? branchId = null,
        bool includeDeleted = false)
    {
        var actor = await _permissionService.GetActorAsync(User);
        if (actor == null)
        {
            return Unauthorized(new ApiError("Tài khoản không còn hợp lệ."));
        }

        var query = IncludeProductGraph(_context.Products.AsNoTracking());
        query = _permissionService.ApplyProductScope(query, actor);

        if (!includeDeleted)
        {
            query = query.Where(product => product.Status != "Đã xóa");
        }

        if (!string.IsNullOrWhiteSpace(line))
        {
            query = ApplyLineFilter(query, DtoMapper.ResolveRequestedLine(line));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(product => product.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(product => product.Status == status);
        }

        if (branchId.HasValue)
        {
            query = query.Where(product => product.BranchId == branchId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(product =>
                product.Name.Contains(keyword)
                || product.Category.Contains(keyword)
                || product.GoldType.Contains(keyword)
                || product.Status.Contains(keyword)
                || (product.Description != null && product.Description.Contains(keyword))
                || (product.DiamondShape != null && product.DiamondShape.Contains(keyword))
                || (product.DiamondColor != null && product.DiamondColor.Contains(keyword))
                || (product.DiamondClarity != null && product.DiamondClarity.Contains(keyword))
                || (product.Branch != null && product.Branch.BranchName.Contains(keyword)));
        }

        var products = await query
            .OrderByDescending(product => product.CreatedAt)
            .ThenByDescending(product => product.Id)
            .Take(300)
            .ToListAsync();

        return products.Select(DtoMapper.ToProductDto).ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var actor = await _permissionService.GetActorAsync(User);
        if (actor == null)
        {
            return Unauthorized(new ApiError("Tài khoản không còn hợp lệ."));
        }

        var product = await _permissionService
            .ApplyProductScope(IncludeProductGraph(_context.Products), actor)
            .FirstOrDefaultAsync(item => item.Id == id);

        return product == null ? NotFound(new ApiError("Không tìm thấy sản phẩm.")) : DtoMapper.ToProductDto(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(ProductUpsertRequest request)
    {
        var actor = await _permissionService.GetActorAsync(User);
        if (actor == null)
        {
            return Unauthorized(new ApiError("Tài khoản không còn hợp lệ."));
        }

        var validationError = await ValidateProductRequestAsync(request, actor);
        if (validationError != null)
        {
            return BadRequest(validationError);
        }

        var assignedLines = ResolveAssignedLines(request);
        var product = new Product
        {
            CreatedAt = DateTime.UtcNow
        };

        ApplyRequest(product, request, assignedLines);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        await SyncCatalogEntriesAsync(product.Id, assignedLines);
        await _context.SaveChangesAsync();

        product = await IncludeProductGraph(_context.Products)
            .FirstAsync(item => item.Id == product.Id);

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, DtoMapper.ToProductDto(product));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductDto>> Update(int id, ProductUpsertRequest request)
    {
        var actor = await _permissionService.GetActorAsync(User);
        if (actor == null)
        {
            return Unauthorized(new ApiError("Tài khoản không còn hợp lệ."));
        }

        var product = await _permissionService
            .ApplyProductScope(IncludeProductGraph(_context.Products), actor)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (product == null)
        {
            return NotFound(new ApiError("Không tìm thấy sản phẩm."));
        }

        var validationError = await ValidateProductRequestAsync(request, actor);
        if (validationError != null)
        {
            return BadRequest(validationError);
        }

        var assignedLines = ResolveAssignedLines(request);
        ApplyRequest(product, request, assignedLines);
        await SyncCatalogEntriesAsync(product.Id, assignedLines);
        await _context.SaveChangesAsync();

        product = await IncludeProductGraph(_context.Products)
            .FirstAsync(item => item.Id == product.Id);
        return DtoMapper.ToProductDto(product);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var actor = await _permissionService.GetActorAsync(User);
        if (actor == null)
        {
            return Unauthorized(new ApiError("Tài khoản không còn hợp lệ."));
        }

        var product = await _permissionService
            .ApplyProductScope(IncludeProductGraph(_context.Products), actor)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (product == null)
        {
            return NotFound(new ApiError("Không tìm thấy sản phẩm."));
        }

        await RemoveCatalogEntriesAsync(product.Id);
        var hasOrderHistory = await _context.OrderDetails.AnyAsync(detail => detail.ProductId == product.Id);
        if (hasOrderHistory)
        {
            product.Status = "Đã xóa";
            product.ProductLine = "Deleted";
            await _context.SaveChangesAsync();
            return Ok(new { deleted = false, archived = true, message = "Sản phẩm đã nằm trong đơn hàng nên được ẩn khỏi danh mục thay vì xóa cứng." });
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return Ok(new { deleted = true, archived = false });
    }

    private static IQueryable<Product> IncludeProductGraph(IQueryable<Product> query)
    {
        return query
            .Include(product => product.Branch)
            .Include(product => product.GoldCatalogEntry)
            .Include(product => product.SilverCatalogEntry)
            .Include(product => product.DiamondCatalogEntry)
            .Include(product => product.GoldSilverCatalogEntry)
            .Include(product => product.GoldDiamondCatalogEntry)
            .Include(product => product.SilverDiamondCatalogEntry);
    }

    private IQueryable<Product> ApplyLineFilter(IQueryable<Product> query, string line)
    {
        var goldIds = _context.GoldProductCatalogEntries.Select(item => item.ProductId)
            .Concat(_context.GoldSilverProductCatalogEntries.Select(item => item.ProductId))
            .Concat(_context.GoldDiamondProductCatalogEntries.Select(item => item.ProductId));
        var silverIds = _context.SilverProductCatalogEntries.Select(item => item.ProductId)
            .Concat(_context.GoldSilverProductCatalogEntries.Select(item => item.ProductId))
            .Concat(_context.SilverDiamondProductCatalogEntries.Select(item => item.ProductId));
        var diamondIds = _context.DiamondProductCatalogEntries.Select(item => item.ProductId)
            .Concat(_context.GoldDiamondProductCatalogEntries.Select(item => item.ProductId))
            .Concat(_context.SilverDiamondProductCatalogEntries.Select(item => item.ProductId));

        return line switch
        {
            ProductLineOptions.Silver => query.Where(product => product.ProductLine == ProductLineOptions.Silver || silverIds.Contains(product.Id)),
            ProductLineOptions.Diamond => query.Where(product => product.ProductLine == ProductLineOptions.Diamond || diamondIds.Contains(product.Id)),
            _ => query.Where(product => product.ProductLine == ProductLineOptions.Gold || goldIds.Contains(product.Id))
        };
    }

    private async Task<ApiError?> ValidateProductRequestAsync(ProductUpsertRequest request, ActorContext actor)
    {
        if (string.IsNullOrWhiteSpace(request.Name)
            || string.IsNullOrWhiteSpace(request.Category)
            || string.IsNullOrWhiteSpace(request.GoldType))
        {
            return new ApiError("Tên, danh mục và chất liệu sản phẩm không được để trống.");
        }

        if (request.SellPrice < 0 || request.BuyPrice < 0 || request.ProcessingFee < 0 || request.Weight < 0)
        {
            return new ApiError("Giá, phí và khối lượng không được âm.");
        }

        if (!actor.IsAdmin && (!actor.User.BranchId.HasValue || actor.User.BranchId.Value != request.BranchId))
        {
            return new ApiError("Bạn không có quyền thao tác sản phẩm ở chi nhánh khác.");
        }

        var branchExists = await _context.Branches.AnyAsync(branch => branch.Id == request.BranchId && branch.IsActive);
        if (!branchExists)
        {
            return new ApiError("Chi nhánh không hợp lệ hoặc đang bị khóa.");
        }

        var assignedLines = ResolveAssignedLines(request);
        if (assignedLines.Count == 0 || assignedLines.Count > 2)
        {
            return new ApiError("Mỗi sản phẩm cần thuộc 1 hoặc 2 dòng danh mục.");
        }

        if (string.Equals(request.CatalogMode, ProductCatalogModeOptions.Multi, StringComparison.OrdinalIgnoreCase) && assignedLines.Count < 2)
        {
            return new ApiError("Sản phẩm kết hợp cần chọn đúng 2 dòng danh mục.");
        }

        if (assignedLines.Contains(ProductLineOptions.Diamond))
        {
            if (string.IsNullOrWhiteSpace(request.DiamondShape))
            {
                return new ApiError("Sản phẩm có kim cương cần chọn hình dạng kim cương.");
            }

            if (!request.DiamondCarat.HasValue && !request.DiamondSize.HasValue)
            {
                return new ApiError("Sản phẩm có kim cương cần nhập carat hoặc kích thước ly.");
            }
        }

        return null;
    }

    private static IReadOnlyList<string> ResolveAssignedLines(ProductUpsertRequest request)
    {
        var primaryLine = DtoMapper.ResolveRequestedLine(request.ProductLine);
        if (!string.Equals(request.CatalogMode, ProductCatalogModeOptions.Multi, StringComparison.OrdinalIgnoreCase))
        {
            return new[] { primaryLine };
        }

        var lines = (request.AssignedProductLines ?? Array.Empty<string>())
            .Select(DtoMapper.ResolveRequestedLine)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (!lines.Contains(primaryLine))
        {
            lines.Insert(0, primaryLine);
        }

        return lines;
    }

    private static void ApplyRequest(Product product, ProductUpsertRequest request, IReadOnlyCollection<string> assignedLines)
    {
        product.ProductLine = assignedLines.FirstOrDefault() ?? DtoMapper.ResolveRequestedLine(request.ProductLine);
        product.Name = Normalize(request.Name);
        product.Category = Normalize(request.Category);
        product.GoldType = Normalize(request.GoldType);
        product.Weight = request.Weight;
        product.ProcessingFee = request.ProcessingFee;
        product.SellPrice = request.SellPrice;
        product.BuyPrice = request.BuyPrice;
        product.BranchId = request.BranchId;
        product.ImagesUrl = ComposeImages(request.Images);
        product.Description = NormalizeOrNull(request.Description);
        product.Status = string.IsNullOrWhiteSpace(request.Status) ? "Còn hàng" : Normalize(request.Status);

        if (assignedLines.Contains(ProductLineOptions.Diamond))
        {
            product.DiamondShape = NormalizeOrNull(request.DiamondShape);
            product.DiamondCut = NormalizeOrNull(request.DiamondCut);
            product.DiamondColor = NormalizeOrNull(request.DiamondColor);
            product.DiamondClarity = NormalizeOrNull(request.DiamondClarity);
            product.DiamondCarat = request.DiamondCarat;
            product.DiamondSize = request.DiamondSize;
            product.DiamondCertificate = NormalizeOrNull(request.DiamondCertificate);
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

    private async Task SyncCatalogEntriesAsync(int productId, IReadOnlyCollection<string> assignedLines)
    {
        await SyncSingleAsync(productId, assignedLines.Count == 1 && assignedLines.Contains(ProductLineOptions.Gold), _context.GoldProductCatalogEntries);
        await SyncSingleAsync(productId, assignedLines.Count == 1 && assignedLines.Contains(ProductLineOptions.Silver), _context.SilverProductCatalogEntries);
        await SyncSingleAsync(productId, assignedLines.Count == 1 && assignedLines.Contains(ProductLineOptions.Diamond), _context.DiamondProductCatalogEntries);
        await SyncSingleAsync(productId, IsPair(assignedLines, ProductLineOptions.Gold, ProductLineOptions.Silver), _context.GoldSilverProductCatalogEntries);
        await SyncSingleAsync(productId, IsPair(assignedLines, ProductLineOptions.Gold, ProductLineOptions.Diamond), _context.GoldDiamondProductCatalogEntries);
        await SyncSingleAsync(productId, IsPair(assignedLines, ProductLineOptions.Silver, ProductLineOptions.Diamond), _context.SilverDiamondProductCatalogEntries);
    }

    private async Task SyncSingleAsync<TEntry>(int productId, bool shouldExist, DbSet<TEntry> dbSet)
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

    private async Task RemoveCatalogEntriesAsync(int productId)
    {
        await SyncCatalogEntriesAsync(productId, Array.Empty<string>());
    }

    private static bool IsPair(IReadOnlyCollection<string> lines, string first, string second)
    {
        return lines.Count == 2 && lines.Contains(first) && lines.Contains(second);
    }

    private static string ComposeImages(IReadOnlyList<string>? images)
    {
        return string.Join(
            ";",
            (images ?? Array.Empty<string>())
                .Where(image => !string.IsNullOrWhiteSpace(image))
                .Select(image => image.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(3));
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    private static string? NormalizeOrNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
