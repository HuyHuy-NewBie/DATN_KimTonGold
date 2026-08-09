using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoldManagementSystem.Models
{
    public class MarketHistory
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Symbol { get; set; }

        public string DisplayName { get; set; }
        public string MarketType { get; set; } // "Gold", "Silver", "Currency"

        [Column(TypeName = "decimal(18,2)")]
        public decimal BuyPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SellPrice { get; set; }
        public string Unit { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
