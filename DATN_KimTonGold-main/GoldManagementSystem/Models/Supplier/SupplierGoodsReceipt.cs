using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoldManagementSystem.Models
{
    public class SupplierGoodsReceipt
    {
        public const string StatusPendingInspection = "Chờ kiểm tra";
        public const string StatusInspecting = "Đang kiểm tra";
        public const string StatusPendingApproval = "Chờ duyệt nhập kho";
        public const string StatusApproved = "Đã duyệt nhập kho";
        public const string StatusPartiallyApproved = "Đạt một phần";
        public const string StatusRejected = "Không đạt";
        public const string StatusReturned = "Đã trả nhà cung cấp";

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public string ReceiptCode { get; set; } = string.Empty;

        public int SupplierPurchaseOrderId { get; set; }

        public virtual SupplierPurchaseOrder SupplierPurchaseOrder { get; set; }

        /*
         * Kho vật lý nhận hàng.
         * Kho phải thuộc đúng chi nhánh của đơn đặt hàng.
         */
        public int WarehouseId { get; set; }

        public virtual Warehouse Warehouse { get; set; }

        [Required]
        public string CreatedByUserId { get; set; } = string.Empty;

        public virtual AppUser CreatedByUser { get; set; }

        /*
         * Thời điểm hàng thực tế được giao tới kho.
         */
        public DateTime ReceivedAt { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string Status { get; set; }
            = StatusPendingInspection;

        /*
         * Số phiếu giao hàng hoặc số hóa đơn của NCC.
         */
        [StringLength(100)]
        public string DeliveryDocumentNumber { get; set; }

        /*
         * Người giao hàng bên phía NCC.
         */
        [StringLength(150)]
        public string DeliveredBy { get; set; }

        /*
         * Chỉ có giá trị sau khi kiểm hàng và duyệt.
         * Khi mới nhận hàng phải bằng 0.
         */
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAcceptedValue { get; set; }

        [StringLength(1000)]
        public string Note { get; set; }

        public virtual ICollection<SupplierGoodsReceiptDetail> Details
        {
            get;
            set;
        } = new List<SupplierGoodsReceiptDetail>();
    }

    public class SupplierGoodsReceiptDetail
    {
        public const string QualityPending = "Chưa kiểm tra";
        public const string QualityPassed = "Đạt";
        public const string QualityPartiallyPassed = "Đạt một phần";
        public const string QualityFailed = "Không đạt";

        public const string ResolutionNone = "Chưa xử lý";
        public const string ResolutionReturn = "Trả nhà cung cấp";
        public const string ResolutionReplace = "Yêu cầu đổi hàng";
        public const string ResolutionDiscount = "Nhận với giá điều chỉnh";

        [Key]
        public int Id { get; set; }

        public int SupplierGoodsReceiptId { get; set; }

        public virtual SupplierGoodsReceipt SupplierGoodsReceipt { get; set; }

        public int SupplierPurchaseOrderDetailId { get; set; }

        public virtual SupplierPurchaseOrderDetail
            SupplierPurchaseOrderDetail { get; set; }

        /*
         * Số lượng giao trong riêng lần nhận hàng này.
         */
        public int ReceivedQuantity { get; set; }

        /*
         * Các trường này chỉ được cập nhật khi kiểm hàng.
         */
        public int AcceptedQuantity { get; set; }

        public int RejectedQuantity { get; set; }

        /*
         * Trọng lượng thực tế của hàng trong lần giao này.
         */
        [Column(TypeName = "decimal(18,2)")]
        public decimal ActualWeight { get; set; }

        /*
         * Carat thực tế, dùng cho kim cương hoặc đá quý.
         */
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ActualDiamondCarat { get; set; }

        [StringLength(120)]
        public string ActualDiamondCertificate { get; set; }

        /*
         * Đơn giá tham chiếu từ đơn đặt hàng.
         * Sau này có thể được điều chỉnh nếu quản lý duyệt giảm giá.
         */
        [Column(TypeName = "decimal(18,2)")]
        public decimal ActualUnitCost { get; set; }

        /*
         * Giá trị hàng được chấp nhận.
         * Khi chưa kiểm hàng thì bằng 0.
         */
        [Column(TypeName = "decimal(18,2)")]
        public decimal LineValue { get; set; }

        [Required]
        [StringLength(50)]
        public string QualityStatus { get; set; }
            = QualityPending;

        /*
         * Ghi chú lúc vừa nhận hàng.
         */
        [StringLength(500)]
        public string ReceivingNote { get; set; }

        /*
         * Ghi chú của người kiểm hàng.
         */
        [StringLength(500)]
        public string QualityNote { get; set; }

        [StringLength(500)]
        public string RejectionReason { get; set; }

        [StringLength(50)]
        public string Resolution { get; set; }
            = ResolutionNone;
    }
}