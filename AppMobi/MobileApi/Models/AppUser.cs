using Microsoft.AspNetCore.Identity;

namespace MobileApi.Models;

public class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
