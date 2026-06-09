using System.ComponentModel.DataAnnotations;

namespace MobileApi.Models;

public class MobileRefreshToken
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(450)]
    public string UserId { get; set; } = string.Empty;

    public AppUser? User { get; set; }

    [Required]
    [StringLength(100)]
    public string DeviceId { get; set; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string TokenHash { get; set; } = string.Empty;

    [StringLength(300)]
    public string? UserAgent { get; set; }

    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}

public class MobileDeviceToken
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(450)]
    public string UserId { get; set; } = string.Empty;

    public AppUser? User { get; set; }

    [Required]
    [StringLength(100)]
    public string DeviceId { get; set; } = string.Empty;

    [Required]
    [StringLength(256)]
    public string ExpoPushToken { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Platform { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;
}

public class MobileOrderNotificationLog
{
    [Key]
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = OrderStatusOptions.PendingApproval;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
