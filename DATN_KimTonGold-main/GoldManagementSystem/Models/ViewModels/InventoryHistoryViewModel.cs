using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace GoldManagementSystem.Models.ViewModels
{
    public class InventoryHistoryViewModel
    {
        public string SearchTerm { get; set; }

        public int? BranchId { get; set; }

        public int? WarehouseId { get; set; }

        public string TransactionType { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public int TotalTransactions { get; set; }

        public int TotalQuantityReceived { get; set; }

        public int TotalQuantityIssued { get; set; }

        public decimal TotalWeightMoved { get; set; }

        public IReadOnlyList<InventoryTransaction> Transactions { get; set; }
            = new List<InventoryTransaction>();

        public IReadOnlyList<SelectListItem> BranchOptions { get; set; }
            = new List<SelectListItem>();

        public IReadOnlyList<SelectListItem> WarehouseOptions { get; set; }
            = new List<SelectListItem>();

        public IReadOnlyList<SelectListItem> TransactionTypeOptions { get; set; }
            = new List<SelectListItem>();
    }
}