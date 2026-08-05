using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoldManagementSystem.Models
{
    public class SupplierPurchaseOrder
    {
        public const string StatusOrdered = "Đã đặt hàng";
        public const string StatusShipping = "Đang giao";
        public const string StatusPartiallyReceived = "Nhận một phần";
        public const string StatusAwaitingReplacement = "Chờ giao bù";
        public const string StatusReceived = "Đã nhận đủ";
        public const string StatusCancelled = "Đã hủy";

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public string OrderCode { get; set; }

        public int SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }

        public int BranchId { get; set; }
        public virtual Branch Branch { get; set; }

        [Required]
        public string CreatedByUserId { get; set; }
        public virtual AppUser CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpectedDeliveryDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = StatusOrdered;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [StringLength(1000)]
        public string Note { get; set; }

        public virtual ICollection<SupplierPurchaseOrderDetail> Details { get; set; } = new List<SupplierPurchaseOrderDetail>();

        public virtual ICollection<SupplierGoodsReceipt> Receipts { get; set; } = new List<SupplierGoodsReceipt>();

        public virtual ICollection<SupplierPayment> Payments { get; set; } = new List<SupplierPayment>();
    }

    public class SupplierPurchaseOrderDetail
    {
        [Key]
        public int Id { get; set; }

        public int SupplierPurchaseOrderId { get; set; }
        public virtual SupplierPurchaseOrder SupplierPurchaseOrder { get; set; }

        [Required]
        [StringLength(30)]
        public string ProductLine { get; set; } = ProductLineOptions.Gold;

        [Required]
        [StringLength(120)]
        public string Category { get; set; }

        [Required]
        [StringLength(220)]
        public string ProductName { get; set; }

        [Required]
        [StringLength(120)]
        public string GoldType { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Weight { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiamondCarat { get; set; }

        [StringLength(120)]
        public string DiamondCertificate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; }

        public int ReceivedQuantity { get; set; }

        public int AcceptedQuantity { get; set; }

        public int RejectedQuantity { get; set; }

        [StringLength(500)]
        public string Note { get; set; }
    }
}