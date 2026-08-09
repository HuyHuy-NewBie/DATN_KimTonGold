using System;
using System.ComponentModel.DataAnnotations;  

namespace GoldManagementSystem.Models
{
    /// <summary>
    /// //3//
    /// Lưu cấu hình chatbot AI và nội dung 5 mục gợi ý nhanh cho toàn hệ thống.
    /// </summary>
    public class ChatSettings
    {
        [Key]
        public int Id { get; set; }

        // ── Thông tin cửa hàng ──────────────────────────────────────────────
        [StringLength(200)]
        public string ShopName { get; set; } = "KimTon Gold";

        [StringLength(50)]
        public string Hotline { get; set; } = "1800 9999";

        [StringLength(500)]
        public string ShopAddress { get; set; } = string.Empty;

        // ── 5 mục dữ liệu cho nút gợi ý nhanh ─────────────────────────────
        /// <summary>Giá sản phẩm: công thức tính, bảng giá công, mức giá theo chất liệu.</summary>
        public string ProductPriceInfo { get; set; } = string.Empty;

        /// <summary>Hướng dẫn chọn size nhẫn, bảng quy đổi chu vi sang size VN.</summary>
        public string SizeGuideInfo { get; set; } = string.Empty;

        /// <summary>Chính sách bảo hành: thời gian, quyền lợi, điều kiện áp dụng.</summary>
        public string WarrantyInfo { get; set; } = string.Empty;

        /// <summary>Chính sách thu đổi vàng: tỷ lệ hao hụt, điều kiện, giấy tờ cần.</summary>
        public string ExchangePolicy { get; set; } = string.Empty;

        /// <summary>Quy trình đặt hàng: đặt cọc, thanh toán, giao hàng.</summary>
        public string OrderProcess { get; set; } = string.Empty;

        // ── Metadata ────────────────────────────────────────────────────────
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(256)]
        public string UpdatedBy { get; set; } = string.Empty; 
    }


}
