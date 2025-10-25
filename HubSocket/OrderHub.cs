using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Ecommerce.HubSocket
{
    [Authorize]
    public class OrderHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst("userId")?.Value;
            var role = Context.User?.FindFirst("role")?.Value;

            // Mỗi user có 1 group riêng (dành cho khi muốn gửi riêng)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");

            // Phân role vào nhóm
            if (role == "Admin")
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            else if (role == "Staff")
                await Groups.AddToGroupAsync(Context.ConnectionId, "Staffs");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User?.FindFirst("userId")?.Value;
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
            await base.OnDisconnectedAsync(exception);
        }


    }
}
