using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GoldManagementSystem.Models
{
    public class AppUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        // Admin có thể không thuộc chi nhánh cụ thể nào (hoặc null = đại diện cho quản trị hệ thống)
        public int? BranchId { get; set; }
        public Branch Branch { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
