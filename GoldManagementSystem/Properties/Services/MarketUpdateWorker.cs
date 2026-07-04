using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using GoldManagementSystem.Services;

namespace GoldManagementSystem.Services
{
    public class MarketUpdateWorker : BackgroundService
    {
        private readonly ILogger<MarketUpdateWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _period = TimeSpan.FromHours(1);

        public MarketUpdateWorker(ILogger<MarketUpdateWorker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MarketUpdateWorker bắt đầu chạy.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var marketService = scope.ServiceProvider.GetRequiredService<IMarketPriceService>();
                        await marketService.SyncMarketRatesAsync(stoppingToken);
                        _logger.LogInformation("Đồng bộ Tỷ giá & Cập nhật giá sản phẩm thành công lúc {time}", DateTimeOffset.Now);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi xảy ra trong quá trình chạy MarketUpdateWorker.");
                }

                await Task.Delay(_period, stoppingToken);
            }
        }
    }
}
