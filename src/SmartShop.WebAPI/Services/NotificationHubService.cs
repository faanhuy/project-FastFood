using Microsoft.AspNetCore.SignalR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.WebAPI.Hubs;

namespace SmartShop.WebAPI.Services;

public class NotificationHubService(IHubContext<OrderStatusHub> hubContext) : INotificationHubService
{
    public async Task SendToUserAsync(string userId, string method, object payload, CancellationToken ct = default)
    {
        await hubContext.Clients.User(userId).SendAsync(method, payload, ct);
    }
}
