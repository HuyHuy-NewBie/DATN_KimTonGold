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

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ chi nhánh.")]
        [StringLength(300, ErrorMessage = "Địa chỉ tối đa 300 ký tự.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn chủ quản lí chi nhánh.")]
        public string ManagerUserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn kho cho chi nhánh.")]
        public int? WarehouseId { get; set; }

        public IReadOnlyList<SelectListItem> ManagerOptions { get; set; } =
            new List<SelectListItem>();
        public IReadOnlyList<SelectListItem> WarehouseOptions { get; set; } =
            new List<SelectListItem>();

        public IReadOnlyList<BranchManagementItemViewModel> Branches { get; set; } =
            new List<BranchManagementItemViewModel>();
    }
}
