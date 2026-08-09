using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoldManagementSystem.Models
{
    public class SupplierPayment
    {
        public const string MethodCash = "Tiền mặt";
        public const string MethodBankTransfer = "Chuyển khoản";

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public string PaymentCode { get; set; }

        public int SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }

        public int SupplierPurchaseOrderId { get; set; }
        public virtual SupplierPurchaseOrder SupplierPurchaseOrder { get; set; }

        [Required]
        public string CreatedByUserId { get; set; }
        public virtual AppUser CreatedByUser { get; set; }

        public DateTime PaidAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = MethodBankTransfer;

        [StringLength(100)]
        public string ReferenceNumber { get; set; }

        [StringLength(500)]
        public string Note { get; set; }
    }
}