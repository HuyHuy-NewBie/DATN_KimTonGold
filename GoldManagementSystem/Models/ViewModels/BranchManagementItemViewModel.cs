namespace GoldManagementSystem.Models.ViewModels
{
    public class BranchManagementItemViewModel
    {
        public int Id { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string OwnerSummary { get; set; } = string.Empty;
        public string ManagerSummary { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int ProductCount { get; set; }
        public int OrderCount { get; set; }
        public int StaffCount { get; set; }
        public bool CanManageMembers { get; set; }
    }
}
