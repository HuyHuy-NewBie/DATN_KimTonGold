using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace GoldManagementSystem.Models.ViewModels
{
    public class InventoryManagementViewModel
    {
        public string SearchTerm { get; set; }

        public int? BranchId { get; set; }

        public int? WarehouseId { get; set; }

        public string StatusFilter { get; set; }

        public int TotalWarehouses { get; set; }

        public int ActiveWarehouses { get; set; }

        public int TotalItemLines { get; set; }

        public int TotalQuantity { get; set; }

        public decimal TotalWeight { get; set; }

        public decimal TotalInventoryValue { get; set; }

        public IReadOnlyList<Warehouse> Warehouses { get; set; }
            = new List<Warehouse>();

        public IReadOnlyList<InventoryItem> InventoryItems { get; set; }
            = new List<InventoryItem>();

        public IReadOnlyList<SelectListItem> BranchOptions { get; set; }
            = new List<SelectListItem>();

        public IReadOnlyList<SelectListItem> WarehouseOptions { get; set; }
            = new List<SelectListItem>();

        public IReadOnlyList<SelectListItem> StatusOptions { get; set; }
            = new List<SelectListItem>();
    }
}