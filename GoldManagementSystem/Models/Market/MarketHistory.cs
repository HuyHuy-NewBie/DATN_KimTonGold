using System;
using System.ComponentModel.DataAnnotations;

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

        public decimal BuyPrice { get; set; }
        public decimal SellPrice { get; set; }
        public string Unit { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
