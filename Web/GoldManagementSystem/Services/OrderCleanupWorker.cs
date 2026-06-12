using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using GoldManagementSystem.Data;
using GoldManagementSystem.Models;
using GoldManagementSystem.Services;
using Microsoft.AspNetCore.Identity;

namespace GoldManagementSystem.Services
{
    public class OrderCleanupWorker : BackgroundService
    {
        private readonly ILogger<OrderCleanupWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _period = TimeSpan.FromMinutes(2); // Check every 2 minutes

        public OrderCleanupWorker(ILogger<OrderCleanupWorker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrderCleanupWorker background task started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                        var notificationService = scope.ServiceProvider.GetRequiredService<AuthNotificationService>();

                        var now = DateTime.UtcNow;
                        var expiredOrders = await context.Orders
                            .Include(order => order.OrderDetails)
                                .ThenInclude(detail => detail.Product)
                            .Where(order =>
                                (order.Status == Order.StatusAwaitingDepositPayment || order.Status == Order.StatusUnpaidDeposit)
                                && order.DepositDueAt.HasValue
                                && order.DepositDueAt.Value <= now)
                            .ToListAsync(stoppingToken);

                        if (expiredOrders.Any())
                        {
                            foreach (var order in expiredOrders)
                            {
                                order.Status = Order.StatusCancelled;
                                order.CancelReason = "Đơn hàng tự hủy vì khách chưa thanh toán cọc trong 1 giờ 30 phút (quét tự động).";

                                // Restore product stock status to 'Còn hàng'
                                foreach (var detail in order.OrderDetails ?? Enumerable.Empty<OrderDetail>())
                                {
                                    if (detail.Product != null && detail.Product.Status == "Đã bán")
                                    {
                                        detail.Product.Status = "Còn hàng";
                                    }
                                }

                                // Notify customer of auto-cancellation
                                try
                                {
                                    var customer = await userManager.FindByIdAsync(order.UserId);
                                    var destination = customer != null && !string.IsNullOrWhiteSpace(customer.Email)
                                        ? customer.Email
                                        : order.CustomerPhone;

                                    if (!string.IsNullOrWhiteSpace(destination))
                                    {
                                        await notificationService.SendOrderCancelledDueToNoDepositNotificationAsync(
                                            destination,
                                            order.CustomerName ?? customer?.FullName ?? "Quý khách",
                                            order.OrderNumber);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Error sending cancellation notification to customer for order {orderNumber}", order.OrderNumber);
                                }
                            }

                            await context.SaveChangesAsync(stoppingToken);
                            _logger.LogInformation("OrderCleanupWorker auto-cancelled {count} expired deposit orders.", expiredOrders.Count);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error running OrderCleanupWorker cycle.");
                }

                await Task.Delay(_period, stoppingToken);
            }
        }
    }
}
