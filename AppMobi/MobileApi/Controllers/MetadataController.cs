using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileApi.Contracts;
using MobileApi.Models;

namespace MobileApi.Controllers;

[ApiController]
[Route("api/metadata")]
[Authorize(Policy = Policies.BackOffice)]
public class MetadataController : ControllerBase
{
    private static readonly string[] GoldCategories =
    {
        "Nhẫn",
        "Nhẫn Cưới",
        "Dây Chuyền",
        "Lắc Tay",
        "Bông Tai",
        "Kiềng cổ",
        "Bộ Sưu Tập"
    };

    private static readonly string[] SilverCategories =
    {
        "Trang Sức Bạc",
        "Nhẫn",
        "Dây Chuyền",
        "Lắc Tay",
        "Bông Tai",
        "Kiềng cổ"
    };

    private static readonly string[] DiamondCategories =
    {
        "Nhẫn Kim Cương",
        "Bông Tai Kim Cương",
        "Dây Chuyền Kim Cương",
        "Mặt Dây Kim Cương",
        "Lắc Tay Kim Cương",
        "Kim Cương Viên"
    };

    private static readonly string[] GoldMaterials = { "Vàng 24K", "Vàng 18K", "Vàng 9999", "Vàng Trắng", "Vàng Ý 750" };
    private static readonly string[] SilverMaterials = { "Bạc S925", "Bạc Ý 925", "Bạc Ta", "Bạc Thái" };
    private static readonly string[] DiamondMaterials = { "Kim Cương Thiên Nhiên", "Kim Cương Lab Grown", "Moissanite" };
    private static readonly string[] ProductStatuses = { "Mới", "Còn hàng", "Bán chạy", "Hết hàng", "Đã bán", "Đã xóa" };
    private static readonly string[] DiamondShapes = { "Round", "Princess", "Oval", "Emerald", "Cushion", "Pear", "Marquise", "Heart" };
    private static readonly string[] DiamondCuts = { "Excellent", "Very Good", "Good" };
    private static readonly string[] DiamondColors = { "D", "E", "F", "G", "H", "I", "J" };
    private static readonly string[] DiamondClarities = { "IF", "VVS1", "VVS2", "VS1", "VS2", "SI1", "SI2" };
    private static readonly string[] DiamondCertificates = { "GIA", "IGI", "AGS", "Không chứng nhận" };

    [HttpGet]
    public ActionResult<CatalogMetadataDto> Get()
    {
        return new CatalogMetadataDto(
            ProductLineOptions.All,
            new[] { ProductCatalogModeOptions.Single, ProductCatalogModeOptions.Multi },
            ProductStatuses,
            OrderStatusOptions.All,
            RoleCatalog.BackOfficeRoles.Concat(new[] { RoleCatalog.Customer }).ToArray(),
            GoldCategories,
            SilverCategories,
            DiamondCategories,
            GoldMaterials,
            SilverMaterials,
            DiamondMaterials,
            DiamondShapes,
            DiamondCuts,
            DiamondColors,
            DiamondClarities,
            DiamondCertificates);
    }
}
