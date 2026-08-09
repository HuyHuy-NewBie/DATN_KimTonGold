using GoldManagementSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace GoldManagementSystem.Models.ViewModels
{
    public class SupplierManagementViewModel
    {
        public string SearchTerm { get; set; } = string.Empty;

        public string StatusFilter { get; set; } = string.Empty;

        public int TotalSuppliers { get; set; }

        public int ActivePurchaseOrders { get; set; }

        public int PendingReceiptCount { get; set; }

        public decimal TotalSupplierDebt { get; set; }

        public IReadOnlyList<Supplier> Suppliers { get; set; } = new List<Supplier>();

        public IReadOnlyList<SupplierPurchaseOrder> PurchaseOrders { get; set; } = new List<SupplierPurchaseOrder>();

        public IReadOnlyList<SupplierGoodsReceipt> RecentReceipts { get; set; } = new List<SupplierGoodsReceipt>();

        public IReadOnlyList<SupplierPayment> RecentPayments { get; set; } = new List<SupplierPayment>();

        public IReadOnlyList<SelectListItem> SupplierOptions { get; set; } = new List<SelectListItem>();

        public IReadOnlyList<SelectListItem> BranchOptions { get; set; } = new List<SelectListItem>();

        public IReadOnlyList<SelectListItem> ProductLineOptions { get; set; } = new List<SelectListItem>();

        public IReadOnlyList<SelectListItem> PaymentMethodOptions { get; set; } = new List<SelectListItem>();
    }
}