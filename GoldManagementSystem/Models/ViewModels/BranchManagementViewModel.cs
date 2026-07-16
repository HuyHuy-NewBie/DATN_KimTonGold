using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GoldManagementSystem.Models.ViewModels
{
    public class BranchManagementViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên chi nhánh.")]
        [StringLength(150, ErrorMessage = "Tên chi nhánh tối đa 150 ký tự.")]
        public string BranchName { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "Địa chỉ tối đa 300 ký tự.")]
        public string Address { get; set; } = string.Empty;

        public string ProductPriceInfo { get; set; } = string.Empty;
        public string SizeSelectionInfo { get; set; } = string.Empty;
        public string WarrantyInfo { get; set; } = string.Empty;
        public string TradeInPolicyInfo { get; set; } = string.Empty;
        public string OrderProcessInfo { get; set; } = string.Empty;

        public string OwnerUserId { get; set; } = string.Empty;
        public string ManagerUserId { get; set; } = string.Empty;
        public IReadOnlyList<SelectListItem> OwnerOptions { get; set; } =
            new List<SelectListItem>();
        public IReadOnlyList<SelectListItem> ManagerOptions { get; set; } =
            new List<SelectListItem>();

        public IReadOnlyList<BranchManagementItemViewModel> Branches { get; set; } =
            new List<BranchManagementItemViewModel>();
    }
}
