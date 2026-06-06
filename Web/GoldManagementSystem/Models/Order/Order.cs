using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoldManagementSystem.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public string OrderNumber { get; set; } = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

        [Required]
        public string UserId { get; set; } // Nhân viên thực hiện giao dịch
        public virtual AppUser User { get; set; }

        public int BranchId { get; set; }
        public virtual Branch Branch { get; set; }

        [StringLength(100)]
        public string CustomerName { get; set; }

        [StringLength(20)]
        public string CustomerPhone { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Hoàn thành"; // Hoàn thành, Hủy

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }

    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } // Giá thực tế lúc bán

        public int Quantity { get; set; }
    }
}
