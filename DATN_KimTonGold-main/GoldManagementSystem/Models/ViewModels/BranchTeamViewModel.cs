using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace GoldManagementSystem.Models.ViewModels
{
    public class BranchTeamViewModel
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool CanManageOwners { get; set; }
        public bool CanManageManagers { get; set; }
        public bool CanManageStaff { get; set; }
        public string ExistingUserId { get; set; } = string.Empty;
        public string NewMemberFullName { get; set; } = string.Empty;
        public string NewMemberEmail { get; set; } = string.Empty;
        public string NewMemberPassword { get; set; } = string.Empty;
        public string NewMemberRole { get; set; } = string.Empty;
        public IReadOnlyList<SelectListItem> ExistingUserOptions { get; set; } =
            new List<SelectListItem>();
        public IReadOnlyList<SelectListItem> NewMemberRoleOptions { get; set; } =
            new List<SelectListItem>();
        public IReadOnlyList<BranchTeamMemberViewModel> Members { get; set; } =
            new List<BranchTeamMemberViewModel>();
    }

    public class BranchTeamMemberViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool CanRemove { get; set; }
    }
}
