using GoldManagementSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GoldManagementSystem.Models.ViewModels
{
    public class InventoryStocktakeViewModel
    {
        public int? BranchId { get; set; }

        public int? WarehouseId { get; set; }

        public Warehouse? SelectedWarehouse { get; set; }

        public IReadOnlyList<SelectListItem> WarehouseOptions { get; set; }
            = Array.Empty<SelectListItem>();

        public IReadOnlyList<InventoryItem> InventoryItems { get; set; }
            = Array.Empty<InventoryItem>();

        public IReadOnlyList<InventoryStocktake> RecentStocktakes { get; set; }
            = Array.Empty<InventoryStocktake>();

        public int TotalLines => InventoryItems.Count;

        public int TotalQuantity => InventoryItems.Sum(x => x.QuantityOnHand);

        public decimal TotalWeight => InventoryItems.Sum(x => x.WeightOnHand);

        public decimal TotalCarat => InventoryItems.Sum(x => x.DiamondCarat ?? 0);
    }
}