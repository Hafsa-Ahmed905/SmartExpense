using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace FP.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public async Task SendNotification(string message)
        {
            var userId = Context.UserIdentifier;
            await Clients.User(userId).SendAsync("ReceiveNotification", message);
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            await Clients.User(userId).SendAsync("UserConnected", "Welcome to SmartExpense!");
            await base.OnConnectedAsync();
        }
    }
}
