using GoldManagementSystem.Data;
using GoldManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoldManagementSystem.Models;

namespace GoldManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMarketPriceService _marketPriceService;

        public HomeController(ApplicationDbContext context, IMarketPriceService marketPriceService)
        {
            _context = context;
            _marketPriceService = marketPriceService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .OrderByDescending(p => p.Status == "Mới")
                .ThenByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.Id)
                .Take(12)
                .ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Market()
        {
            var goldRates = await _marketPriceService.GetLatestRatesAsync("Gold");
            var silverRates = await _marketPriceService.GetLatestRatesAsync("Silver");
            var currencyRates = await _marketPriceService.GetLatestRatesAsync("Currency");
            
            // Lấy 30 ngày để vẽ biểu đồ
            var historyGold = await _marketPriceService.GetHistoricalRatesAsync("Gold", 30);
            var historySilver = await _marketPriceService.GetHistoricalRatesAsync("Silver", 30);
            var historyCurrency = await _marketPriceService.GetHistoricalRatesAsync("Currency", 30);

            ViewBag.GoldRates = goldRates;
            ViewBag.SilverRates = silverRates;
            ViewBag.CurrencyRates = currencyRates;

            ViewBag.HistoryGold = historyGold;
            ViewBag.HistorySilver = historySilver;
            ViewBag.HistoryCurrency = historyCurrency;
            
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetExchangeRates()
        {
            var rates = await _marketPriceService.GetLatestRatesAsync("Currency");
            var usd = rates?.FirstOrDefault(r => r.Symbol == "USD");
            var eur = rates?.FirstOrDefault(r => r.Symbol == "EUR");

            return Json(new { 
                USD = usd != null ? usd.SellPrice : 25400,
                EUR = eur != null ? eur.SellPrice : 27900
            });
        }

        [HttpGet]
        public async Task<IActionResult> MarketPrices()
        {
            var goldLatest = await _marketPriceService.GetLatestRatesAsync("Gold");
            var silverLatest = await _marketPriceService.GetLatestRatesAsync("Silver");
            var currencyLatest = await _marketPriceService.GetLatestRatesAsync("Currency");

            var goldHistory = await _marketPriceService.GetHistoricalRatesAsync("Gold", 31);
            var silverHistory = await _marketPriceService.GetHistoricalRatesAsync("Silver", 31);

            var usdRate = currencyLatest.FirstOrDefault(r => r.Symbol == "USD")?.SellPrice ?? 25400;

            var snapshot = new MarketDashboardSnapshot
            {
                IsLive = true,
                RetrievedAtUtc = System.DateTime.Now,
                UsdToVndRate = usdRate,
                Gold = CreateMetalSnapshot("Gold", goldLatest, goldHistory),
                Silver = CreateMetalSnapshot("Silver", silverLatest, silverHistory),
                StatusMessage = "Dữ liệu thị trường được cập nhật trực tiếp từ hệ thống GoldSys."
            };

            return Json(snapshot);
        }

        private PreciousMetalSnapshot CreateMetalSnapshot(string type, List<MarketHistory> latest, List<MarketHistory> history)
        {
            var main = latest.FirstOrDefault() ?? new MarketHistory();
            
            return new PreciousMetalSnapshot
            {
                Key = type.ToLower(),
                Symbol = main.Symbol ?? (type == "Gold" ? "AU" : "AG"),
                DisplayName = main.DisplayName ?? (type == "Gold" ? "Vàng" : "Bạc"),
                Price = main.SellPrice,
                Bid = main.BuyPrice,
                Ask = main.SellPrice,
                Currency = "VND",
                Unit = main.Unit ?? "VND/Chỉ",
                LastUpdatedUtc = main.Timestamp,
                DataAgeMinutes = (int)(System.DateTime.Now - main.Timestamp).TotalMinutes,
                Change24H = CalculateChange(history, 1),
                Change7D = CalculateChange(history, 7),
                Change30D = CalculateChange(history, 30),
                Sources = latest.Select(r => new MarketSourcePriceSnapshot
                {
                    Source = r.Symbol,
                    Price = r.SellPrice,
                    Bid = r.BuyPrice,
                    Ask = r.SellPrice,
                    TimestampUtc = r.Timestamp
                }).ToList()
            };
        }

        private MarketChangeSnapshot CalculateChange(List<MarketHistory> history, int days)
        {
            if (history == null || !history.Any()) return new MarketChangeSnapshot();

            var latest = history.OrderByDescending(h => h.Timestamp).FirstOrDefault();
            var targetDate = System.DateTime.Now.Date.AddDays(-days);
            var past = history.Where(h => h.Timestamp.Date <= targetDate)
                              .OrderByDescending(h => h.Timestamp)
                              .FirstOrDefault();

            if (latest == null || past == null || past.SellPrice == 0)
                return new MarketChangeSnapshot();

            var amount = latest.SellPrice - past.SellPrice;
            var percent = (amount / past.SellPrice) * 100;

            return new MarketChangeSnapshot
            {
                Amount = amount,
                Percent = System.Math.Round(percent, 2)
            };
        }
    }
}
