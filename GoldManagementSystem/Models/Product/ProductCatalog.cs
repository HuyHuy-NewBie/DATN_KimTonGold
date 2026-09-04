namespace GoldManagementSystem.Models
{
    public static class ProductMaterialOptions
    {
        public const string Gold = "Gold";
        public const string Silver = "Silver";
        public const string Diamond = "Diamond";
    }

    public static class ProductFormOptions
    {
        public const string Bar = "Bar";
        public const string Jewelry = "Jewelry";
        public const string RawMaterial = "RawMaterial";
        public const string FinishedGood = "FinishedGood";
    }

    public static class ProductLegalClassOptions
    {
        public const string GoldBarRegulated = "GoldBarRegulated";
        public const string GoldJewelry = "GoldJewelry";
        public const string GoldRawMaterial = "GoldRawMaterial";
        public const string SilverCommodity = "SilverCommodity";
        public const string SilverJewelry = "SilverJewelry";
        public const string SilverRawMaterial = "SilverRawMaterial";
        public const string DiamondExcluded = "DiamondExcluded";
    }

    public static class ProductUnitOfMeasureOptions
    {
        public const string Piece = "Piece";
        public const string Gram = "Gram";
        public const string Tael = "Tael";
    }

    public static class ProductLineOptions
    {
        public const string Gold = "Gold";
        public const string Silver = "Silver";
        public const string Diamond = "Diamond";

        public static readonly string[] All =
        {
            Gold,
            Silver,
            Diamond
        };
    }
}
