using System.Collections.Generic;

namespace GoldManagementSystem.Models.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public bool IsActive { get; set; }
        public IList<string> Roles { get; set; }
    }
}
