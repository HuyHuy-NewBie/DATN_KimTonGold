using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace GoldManagementSystem.Hubs
{
    public class SupportChatHub : Hub
    {
        public async Task JoinSessionGroup(string sessionCode)
        {
            if (!string.IsNullOrWhiteSpace(sessionCode))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"session:{sessionCode}");
            }
        }

        public async Task LeaveSessionGroup(string sessionCode)
        {
            if (!string.IsNullOrWhiteSpace(sessionCode))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"session:{sessionCode}");
            }
        }

        public async Task JoinStaffGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "role:cskh_staff");
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrWhiteSpace(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
            }
            await base.OnConnectedAsync();
        }
    }
}
