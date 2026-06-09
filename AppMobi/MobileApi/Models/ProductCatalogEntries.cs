using System.ComponentModel.DataAnnotations;

namespace MobileApi.Models;

public class GoldProductCatalogEntry
{
    [Key]
    public int ProductId { get; set; }
    public Product? Product { get; set; }
}

public class SilverProductCatalogEntry
{
    [Key]
    public int ProductId { get; set; }
    public Product? Product { get; set; }
}

public class DiamondProductCatalogEntry
{
    [Key]
    public int ProductId { get; set; }
    public Product? Product { get; set; }
}

public class GoldSilverProductCatalogEntry
{
    [Key]
    public int ProductId { get; set; }
    public Product? Product { get; set; }
}

public class GoldDiamondProductCatalogEntry
{
    [Key]
    public int ProductId { get; set; }
    public Product? Product { get; set; }
}

public class SilverDiamondProductCatalogEntry
{
    [Key]
    public int ProductId { get; set; }
    public Product? Product { get; set; }
}
