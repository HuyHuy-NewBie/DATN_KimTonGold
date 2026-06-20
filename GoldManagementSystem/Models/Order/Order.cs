using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoldManagementSystem.Models
{
    public class Order
    {
        public const string StatusAwaitingDepositPayment = "Chờ thanh toán cọc";
        public const string StatusUnpaidDeposit = "Chưa thanh toán cọc";
        public const string StatusPendingConfirmation = "Chờ xác nhận";
        public const string StatusConfirmed = "Đã xác nhận";
        public const string StatusProcessing = "Đang xử lý";
        public const string StatusShipping = "Vận chuyển";
        public const string StatusCompleted = "Hoàn thành";
        public const string StatusCancelled = "Đã hủy";

        public const string PaymentMethodOnlineDeposit = "OnlineDeposit";
        public const string PaymentMethodCashDeposit = "CashDeposit";
        public const string PaymentMethodOnlineFull = "OnlineFull";

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

        [Column(TypeName = "decimal(18,2)")]
        public decimal DepositAmount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal DepositRate { get; set; } = 10m;

        [StringLength(30)]
        public string PaymentMethod { get; set; } = PaymentMethodOnlineDeposit;

        [StringLength(50)]
        public string Status { get; set; } = StatusPendingConfirmation;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public DateTime? DepositDueAt { get; set; }

        public DateTime? DepositPaidAt { get; set; }

        public DateTime? ConfirmedAt { get; set; }

        [StringLength(500)]
        public string CancelReason { get; set; }

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
