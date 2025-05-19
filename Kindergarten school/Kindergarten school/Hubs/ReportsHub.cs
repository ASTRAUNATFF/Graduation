using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Kindergarten_school.Hubs
{
    public class ReportsHub : Hub
    {

        public override async Task OnConnectedAsync()
        {
            // Lấy UserID từ Claims 
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                // Nếu không có UserID, sử dụng ConnectionId thay thế
                userId = Context.ConnectionId;
            }

            // Lưu userId vào Context.Items để sử dụng khi disconnect
            Context.Items["UserId"] = userId;

            // Thêm vào nhóm
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);

            Console.WriteLine($"User connected: {userId}");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Lấy userId từ Context.Items
            if (Context.Items.TryGetValue("UserId", out var userIdObj) && userIdObj is string userId)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                Console.WriteLine($"User disconnected: {userId}");
            }

            await base.OnDisconnectedAsync(exception);
        }

    }
}