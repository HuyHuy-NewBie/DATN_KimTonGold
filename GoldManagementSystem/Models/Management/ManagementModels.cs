using System;
using System.ComponentModel.DataAnnotations;

namespace GoldManagementSystem.Models
{
    public class SystemNotification
    {
        public int Id { get; set; }
        [Required, MaxLength(450)] public string UserId { get; set; }
        public AppUser User { get; set; }
        [Required, MaxLength(160)] public string Title { get; set; }
        [Required, MaxLength(1000)] public string Message { get; set; }
        [MaxLength(500)] public string Link { get; set; }
        [MaxLength(40)] public string Type { get; set; } = "Info";
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ManagementAuditLog
    {
        public long Id { get; set; }
        [MaxLength(450)] public string UserId { get; set; }
        [MaxLength(120)] public string UserName { get; set; }
        [Required, MaxLength(30)] public string Area { get; set; }
        [Required, MaxLength(20)] public string HttpMethod { get; set; }
        [Required, MaxLength(120)] public string Action { get; set; }
        [MaxLength(120)] public string EntityType { get; set; }
        [MaxLength(120)] public string EntityId { get; set; }
        public int? BranchId { get; set; }
        [MaxLength(1000)] public string Details { get; set; }
        [MaxLength(64)] public string IpAddress { get; set; }
        public bool Succeeded { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Một kho có thể được chia sẻ cho nhiều chi nhánh nhưng vẫn giữ chi nhánh sở hữu.
    public class BranchWarehouseAccess
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public Branch Branch { get; set; }
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
