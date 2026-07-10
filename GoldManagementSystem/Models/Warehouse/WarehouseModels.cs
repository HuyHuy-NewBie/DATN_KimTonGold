using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoldManagementSystem.Models
{
    public class Warehouse
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public string Code { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        public int BranchId { get; set; }

        public virtual Branch Branch { get; set; }

        [StringLength(300)]
        public string Location { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<InventoryItem> InventoryItems { get; set; }
            = new List<InventoryItem>();

        public virtual ICollection<InventoryTransaction> Transactions { get; set; }
            = new List<InventoryTransaction>();
    }

    public class InventoryItem
    {
        public const string StatusAvailable = "Sẵn sàng";
        public const string StatusReserved = "Đang giữ";
        public const string StatusQuarantined = "Cách ly";
        public const string StatusOutOfStock = "Hết tồn";

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        public string StockCode { get; set; }

        public int WarehouseId { get; set; }

        public virtual Warehouse Warehouse { get; set; }

        // Nhà cung cấp của lô hàng
        public int? SupplierId { get; set; }

        public virtual Supplier Supplier { get; set; }

        // Đơn đặt hàng tạo ra lô hàng này
        public int? SupplierPurchaseOrderId { get; set; }

        public virtual SupplierPurchaseOrder SupplierPurchaseOrder { get; set; }

        // Chi tiết phiếu nhận hàng đã được kiểm tra đạt
        public int? SupplierGoodsReceiptDetailId { get; set; }

        public virtual SupplierGoodsReceiptDetail SupplierGoodsReceiptDetail { get; set; }

        [Required]
        [StringLength(30)]
        public string ProductLine { get; set; }

        [Required]
        [StringLength(120)]
        public string Category { get; set; }

        [Required]
        [StringLength(220)]
        public string ProductName { get; set; }

        [Required]
        [StringLength(120)]
        public string MaterialType { get; set; }

        public int QuantityOnHand { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal WeightOnHand { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiamondCarat { get; set; }

        [StringLength(120)]
        public string CertificateCode { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = StatusAvailable;

        [StringLength(500)]
        public string Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public decimal InventoryValue
        {
            get
            {
                if (QuantityOnHand <= 0)
                {
                    return 0;
                }

                return QuantityOnHand * UnitCost;
            }
        }
    }

    public class InventoryTransaction
    {
        public const string TypeSupplierReceipt = "Nhập từ nhà cung cấp";
        public const string TypeCustomerIssue = "Xuất bán hàng";
        public const string TypeSupplierReturn = "Trả nhà cung cấp";
        public const string TypeAdjustmentIncrease = "Điều chỉnh tăng";
        public const string TypeAdjustmentDecrease = "Điều chỉnh giảm";
        public const string TypeTransferIn = "Nhận điều chuyển";
        public const string TypeTransferOut = "Xuất điều chuyển";

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        public string TransactionCode { get; set; }

        public int WarehouseId { get; set; }

        public virtual Warehouse Warehouse { get; set; }

        public int InventoryItemId { get; set; }

        public virtual InventoryItem InventoryItem { get; set; }

        [Required]
        [StringLength(50)]
        public string TransactionType { get; set; }

        /*
         * Nhập kho: số dương
         * Xuất kho: số âm
         */
        public int QuantityChange { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal WeightChange { get; set; }

        public int QuantityAfter { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal WeightAfter { get; set; }

        [StringLength(50)]
        public string ReferenceType { get; set; }

        public int? ReferenceId { get; set; }

        [StringLength(500)]
        public string Note { get; set; }

        [Required]
        public string CreatedByUserId { get; set; }

        public virtual AppUser CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}