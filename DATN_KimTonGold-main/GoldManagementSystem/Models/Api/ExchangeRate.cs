using System;

namespace GoldManagementSystem.Models.Api
{
    public class ExchangeRate
    {
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal BuyPrice { get; set; }
        public decimal SellPrice { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
