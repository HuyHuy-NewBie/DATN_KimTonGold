using System;
using System.Collections.Generic;

namespace GoldManagementSystem.Models
{
    public class MarketDashboardSnapshot
    {
        public bool IsLive { get; set; }

        public string SourceName { get; set; } = "SILV DATA";

        public string DocumentationUrl { get; set; } = "https://datadocs.silv.app/";

        public string StatusMessage { get; set; } = string.Empty;

        public string UpdateFrequency { get; set; } = "15 minutes";

        public int RefreshIntervalMinutes { get; set; } = 60;

        public DateTime RetrievedAtUtc { get; set; }

        public string DisplayCurrency { get; set; } = "VND";

        public decimal? UsdToVndRate { get; set; }

        public string FxSourceName { get; set; } = "Frankfurter";

        public string FxDocumentationUrl { get; set; } = "https://frankfurter.dev/docs/";

        public DateTime? FxUpdatedAtUtc { get; set; }

        public PreciousMetalSnapshot Silver { get; set; } = new();

        public PreciousMetalSnapshot Gold { get; set; } = new();
    }

    public class PreciousMetalSnapshot
    {
        public string Key { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string Symbol { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Currency { get; set; } = "USD";

        public string Unit { get; set; } = "troy_oz";

        public decimal Price { get; set; }

        public decimal? Bid { get; set; }

        public decimal? Ask { get; set; }

        public decimal? SpreadPercent { get; set; }

        public int? DataAgeMinutes { get; set; }

        public DateTime? LastUpdatedUtc { get; set; }

        public MarketChangeSnapshot Change24H { get; set; } = new();

        public MarketChangeSnapshot Change7D { get; set; } = new();

        public MarketChangeSnapshot Change30D { get; set; } = new();

        public List<MarketSourcePriceSnapshot> Sources { get; set; } = new();
    }

    public class MarketChangeSnapshot
    {
        public decimal Amount { get; set; }

        public decimal Percent { get; set; }
    }

    public class MarketSourcePriceSnapshot
    {
        public string Source { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public decimal? Bid { get; set; }

        public decimal? Ask { get; set; }

        public DateTime? TimestampUtc { get; set; }
    }
}
