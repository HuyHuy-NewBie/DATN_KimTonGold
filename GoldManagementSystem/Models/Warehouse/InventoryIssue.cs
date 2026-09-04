using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoldManagementSystem.Models
{
    /*
     * Phiếu xuất kho.
     *
     * Khi vừa tạo, phiếu ở trạng thái Chờ xuất kho
     * và chưa làm thay đổi tồn kho.
     *
     * Chỉ khi xác nhận xuất kho, hệ thống mới:
     * - Trừ InventoryItem
     * - Tạo InventoryTransaction số âm
     * - Chuyển phiếu sang Đã xuất kho
     */
    public class InventoryIssue
    {
        public const string StatusPending =
            "Chờ xuất kho";

        public const string StatusIssued =
            "Đã xuất kho";

        public const string StatusCancelled =
            "Đã hủy";

        /*
        * Phiếu xuất kho chỉ giải quyết những trường hợp
        * làm giảm tồn kho thực tế của chi nhánh.
        */
        public const string TypeSale =
            "Xuất ra quầy trưng bày";

        public const string TypeSupplierReturn =
            "Trả nhà cung cấp";

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        public string IssueCode { get; set; }

        /*
         * Chi nhánh thực hiện nghiệp vụ xuất.
         */
        public int BranchId { get; set; }

        public int? OrderId { get; set; }

        public virtual Order Order { get; set; }

        public virtual Branch Branch { get; set; }

        /*
         * Kho thực tế bị trừ hàng.
         */
        public int WarehouseId { get; set; }

        public virtual Warehouse Warehouse { get; set; }

        public int? DestinationWarehouseId { get; set; }

        public virtual Warehouse DestinationWarehouse
        {
            get;
            set;
        }

        public int? SupplierId { get; set; }

        public virtual Supplier Supplier { get; set; }

        [Required]
        [StringLength(50)]
        public string IssueType { get; set; }
            = TypeSale;

        [Required]
        [StringLength(50)]
        public string Status { get; set; }
            = StatusPending;

        /*
        * Nhân viên chịu trách nhiệm nhận hàng tại quầy.
        * Để nullable nhằm không làm lỗi các phiếu thử nghiệm cũ.
        * Phiếu mới vẫn bắt buộc chọn trong Controller.
        */
        [StringLength(450)]
        public string ReceiverUserId { get; set; }

        public virtual AppUser ReceiverUser { get; set; }

        /*
         * Có thể nhập mã đơn bán hàng,
         * mã yêu cầu nội bộ hoặc mã chứng từ liên quan.
         */
        /*
        * Xuất bán hàng: mã đơn bán hàng.
        * Trả nhà cung cấp: mã yêu cầu trả hoặc mã chứng từ.
        */
        [StringLength(100)]
        public string ReferenceCode { get; set; }

        /*
        * Bắt buộc khi trả nhà cung cấp.
        * Ví dụ: sai quy cách, lỗi phát hiện sau nhập kho,
        * giao không đúng thỏa thuận.
        */
        [StringLength(500)]
        public string Reason { get; set; }

        [StringLength(1000)]
        public string Note { get; set; }

        [Required]
        public string CreatedByUserId { get; set; }

        public virtual AppUser CreatedByUser { get; set; }

        /*
         * Người nhấn nút xác nhận xuất kho.
         * Khi phiếu còn chờ xuất, trường này bằng null.
         */
        public string ConfirmedByUserId { get; set; }

        public virtual AppUser ConfirmedByUser { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        public DateTime? IssuedAt { get; set; }

        public virtual ICollection<InventoryIssueDetail>
            Details { get; set; }
                = new List<InventoryIssueDetail>();
    }

    /*
     * Một dòng hàng trong phiếu xuất kho.
     */
    public class InventoryIssueDetail
    {
        [Key]
        public int Id { get; set; }

        public int InventoryIssueId { get; set; }

        public virtual InventoryIssue InventoryIssue
        {
            get;
            set;
        }

        /*
         * Mã tồn kho được chọn để xuất.
         */
        public int InventoryItemId { get; set; }

        public virtual InventoryItem InventoryItem
        {
            get;
            set;
        }

        /*
         * Số lượng xuất phải lớn hơn 0
         * và không được vượt QuantityOnHand.
         */
        public int Quantity { get; set; }

        /*
         * Trọng lượng thực tế xuất.
         * Phụ kiện không có trọng lượng có thể bằng 0.
         */
        [Column(TypeName = "decimal(18,2)")]
        public decimal IssuedWeight { get; set; }

        /*
         * Lưu lại giá vốn tại thời điểm lập phiếu,
         * tránh việc giá nhập sau này thay đổi.
         */
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }

        [StringLength(500)]
        public string Note { get; set; }

        [NotMapped]
        public decimal LineValue
        {
            get
            {
                if (Quantity <= 0)
                {
                    return 0;
                }

                return Quantity * UnitCost;
            }
        }
    }
}
