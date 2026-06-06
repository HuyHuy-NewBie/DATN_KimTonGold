using System.ComponentModel.DataAnnotations;

namespace MobileApi.Models;

public class Branch
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string BranchName { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Address { get; set; }

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<AppUser> AppUsers { get; set; } = new List<AppUser>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
