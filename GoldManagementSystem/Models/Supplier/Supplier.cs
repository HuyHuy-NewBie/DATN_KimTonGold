using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GoldManagementSystem.Models
{
    public class Supplier
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(180)]
        public string Name { get; set; }

        [Required]
        [StringLength(13)]
        public string TaxCode { get; set; }

        [Required]
        [StringLength(120)]
        public string ContactPerson { get; set; }

        [Required]
        [StringLength(10)]
        public string Phone { get; set; }

        [StringLength(150)]
        public string Email { get; set; }

        [Required]
        [StringLength(300)]
        public string Address { get; set; }

        [Required]
        [StringLength(200)]
        public string SupplierType { get; set; } = "Vàng";

        public int PaymentTermDays { get; set; } = 0;

        [StringLength(120)]
        public string BankName { get; set; }

        [StringLength(20)]
        public string BankAccountNumber { get; set; }

        [StringLength(120)]
        public string BankAccountName { get; set; }

        [StringLength(1000)]
        public string Note { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<SupplierPurchaseOrder> PurchaseOrders { get; set; } = new List<SupplierPurchaseOrder>();

        public virtual ICollection<SupplierPayment> Payments { get; set; } = new List<SupplierPayment>();
    }
}