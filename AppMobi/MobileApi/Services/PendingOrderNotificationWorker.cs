using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MobileApi.Data;
using MobileApi.Models;

namespace MobileApi.Services;

public class PendingOrderNotificationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PendingOrderNotificationWorker> _logger;

    public PendingOrderNotificationWorker(IServiceScopeFactory scopeFactory, ILogger<PendingOrderNotificationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingOrdersAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Pending order notification worker failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task ProcessPendingOrdersAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var pushService = scope.ServiceProvider.GetRequiredService<ExpoPushNotificationService>();

        var notifiedOrderIds = context.MobileOrderNotificationLogs.Select(log => log.OrderId);
        var pendingOrders = await context.Orders
            .Include(order => order.Branch)
            .Where(order => order.Status == OrderStatusOptions.PendingApproval && !notifiedOrderIds.Contains(order.Id))
            .OrderBy(order => order.OrderDate)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var order in pendingOrders)
        {
            var devices = await context.MobileDeviceTokens
                .Include(token => token.User)
                .Where(token => token.IsActive)
                .ToListAsync(cancellationToken);

            var targetDevices = new List<MobileDeviceToken>();
            foreach (var device in devices)
            {
                if (device.User == null || !device.User.IsActive)
                {
                    continue;
                }

                var roles = await userManager.GetRolesAsync(device.User);
                var canReceive = roles.Any(role =>
                    string.Equals(role, RoleCatalog.Admin, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(role, RoleCatalog.BranchOwner, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(role, RoleCatalog.Manager, StringComparison.OrdinalIgnoreCase));

                if (!canReceive)
                {
                    continue;
                }

                var isAdmin = roles.Contains(RoleCatalog.Admin, StringComparer.OrdinalIgnoreCase);
                if (!isAdmin && device.User.BranchId != order.BranchId)
                {
                    continue;
                }

                targetDevices.Add(device);
            }

            await pushService.SendPendingOrderAsync(order, targetDevices, cancellationToken);
            context.MobileOrderNotificationLogs.Add(new MobileOrderNotificationLog
            {
                OrderId = order.Id,
                Status = order.Status,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
