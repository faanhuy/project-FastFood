namespace SmartShop.Application.Common.Interfaces;

public interface INotificationHubService
{
    Task SendToUserAsync(string userId, string method, object payload, CancellationToken ct = default);
}
