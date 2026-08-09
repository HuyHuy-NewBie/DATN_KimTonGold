using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoldManagementSystem.Models
{
    public class CustomerFeedback
    {
        [Key]
        public int Id { get; set; }

        [StringLength(450)]
        public string CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual AppUser Customer { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [StringLength(20)]
        public string CustomerPhone { get; set; }

        [StringLength(256)]
        public string CustomerEmail { get; set; }

        public int? BranchId { get; set; }

        public int Rating { get; set; } = 5; // 1 to 5 stars

        [StringLength(100)]
        public string Category { get; set; } = "Sản phẩm"; // Sản phẩm, Dịch vụ CSKH, Giao hàng, Giá cả, Khác

        public int? ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [StringLength(200)]
        public string ProductName { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Chờ xử lý"; // Chờ xử lý, Đã xử lý

        [StringLength(2000)]
        public string AdminResponse { get; set; }

        public DateTime? RespondedAt { get; set; }

        [StringLength(100)]
        public string RespondedByName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
