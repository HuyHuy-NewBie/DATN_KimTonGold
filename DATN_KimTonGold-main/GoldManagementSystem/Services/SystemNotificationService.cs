using GoldManagementSystem.Data;
using GoldManagementSystem.Hubs;
using GoldManagementSystem.Models;
using Microsoft.AspNetCore.SignalR;

namespace GoldManagementSystem.Services
{
    public sealed class SystemNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public SystemNotificationService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task SendAsync(string userId, string title, string message, string link, string type = "Info")
        {
            var notification = new SystemNotification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Link = link,
                Type = type
            };
            _context.SystemNotifications.Add(notification);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group($"user:{userId}").SendAsync("ReceiveSystemNotification", new
            {
                notification.Id,
                notification.Title,
                Body = notification.Message,
                notification.Link,
                notification.Type,
                Time = notification.CreatedAt
            });
        }
    }
}
