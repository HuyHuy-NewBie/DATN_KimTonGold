using GoldManagementSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace GoldManagementSystem.Models.ViewModels
{
    public class InventoryIssueManagementViewModel
    {
        public string SearchTerm { get; set; }

        public int? BranchId { get; set; }

        public int? WarehouseId { get; set; }

        public string IssueType { get; set; }

        public string Status { get; set; }

        public int TotalIssues { get; set; }

        public int PendingIssues { get; set; }

        public int IssuedIssues { get; set; }

        public int TotalIssuedQuantity { get; set; }

        public IReadOnlyList<InventoryIssue> Issues
        {
            get;
            set;
        } = new List<InventoryIssue>();

        /*
         * Sẽ dùng cho popup tạo phiếu ở bước tiếp theo.
         */
        public IReadOnlyList<InventoryItem> AvailableItems
        {
            get;
            set;
        } = new List<InventoryItem>();

        public IReadOnlyList<SelectListItem> BranchOptions
        {
            get;
            set;
        } = new List<SelectListItem>();

        public IReadOnlyList<SelectListItem> WarehouseOptions
        {
            get;
            set;
        } = new List<SelectListItem>();

        public IReadOnlyList<SelectListItem> IssueTypeOptions
        {
            get;
            set;
        } = new List<SelectListItem>();

        public IReadOnlyList<SelectListItem> StatusOptions
        {
            get;
            set;
        } = new List<SelectListItem>();
    }
}