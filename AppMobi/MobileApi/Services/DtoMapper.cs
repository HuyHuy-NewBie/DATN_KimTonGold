using MobileApi.Contracts;
using MobileApi.Models;

namespace MobileApi.Services;

public static class DtoMapper
{
    public static UserProfileDto ToUserProfile(AppUser user, IReadOnlyList<string> roles, IReadOnlyList<string> permissions)
    {
        return new UserProfileDto(
            user.Id,
            user.FullName,
            user.Email,
            user.PhoneNumber,
            user.BranchId,
            user.Branch?.BranchName,
            user.IsActive,
            roles,
            RoleCatalog.GetHighestRole(roles),
            permissions);
    }

    public static BranchDto ToBranchDto(Branch branch)
    {
        return new BranchDto(branch.Id, branch.BranchName, branch.Address, branch.PhoneNumber, branch.IsActive);
    }

    public static ProductDto ToProductDto(Product product)
    {
        var assignedLines = ResolveAssignedProductLines(product);
        return new ProductDto(
            product.Id,
            product.Name,
            product.Category,
            product.GoldType,
            product.ProductLine,
            assignedLines.Count > 1 ? ProductCatalogModeOptions.Multi : ProductCatalogModeOptions.Single,
            assignedLines,
            product.Weight,
            product.ProcessingFee,
            product.SellPrice,
            product.BuyPrice,
            product.BranchId,
            product.Branch?.BranchName,
            SplitImages(product.ImagesUrl),
            product.Description,
            product.Status,
            product.CreatedAt,
            product.DiamondShape,
            product.DiamondCut,
            product.DiamondColor,
            product.DiamondClarity,
            product.DiamondCarat,
            product.DiamondSize,
            product.DiamondCertificate);
    }

    public static OrderDto ToOrderDto(Order order)
    {
        return new OrderDto(
            order.Id,
            order.OrderNumber,
            order.CustomerName,
            order.CustomerPhone,
            order.TotalAmount,
            order.Status,
            order.OrderDate,
            order.BranchId,
            order.Branch?.BranchName,
            order.User?.FullName,
            order.OrderDetails
                .OrderBy(detail => detail.Id)
                .Select(detail => new OrderDetailDto(
                    detail.Id,
                    detail.ProductId,
                    detail.Product?.Name,
                    detail.UnitPrice,
                    detail.Quantity))
                .ToList());
    }

    public static IReadOnlyList<string> SplitImages(string? imagesUrl)
    {
        return string.IsNullOrWhiteSpace(imagesUrl)
            ? Array.Empty<string>()
            : imagesUrl
                .Split(new[] { ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(image => image.Trim())
                .Where(image => !string.IsNullOrWhiteSpace(image))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
    }

    public static IReadOnlyList<string> ResolveAssignedProductLines(Product product)
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

    public static string ResolveRequestedLine(string? line)
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
}
