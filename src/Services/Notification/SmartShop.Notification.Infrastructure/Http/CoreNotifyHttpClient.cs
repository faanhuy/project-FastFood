using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using SmartShop.Notification.Application.Common.Interfaces;

namespace SmartShop.Notification.Infrastructure.Http;

// Gọi POST /api/internal/notify ở Core — header shared-secret là cơ chế bảo vệ chính (cổng của Core
// đã public ra host cho REST thông thường nên network isolation không đủ, xem microservices-boundaries.md).
public class CoreNotifyHttpClient(HttpClient httpClient, IConfiguration configuration) : ICoreNotifyClient
{
    public async Task NotifyAsync(InternalNotifyPayload payload, CancellationToken ct = default)
    {
        var apiKey = configuration["Internal:ApiKey"]
            ?? throw new InvalidOperationException("Missing configuration 'Internal:ApiKey'.");

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/notify")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("X-Internal-Api-Key", apiKey);

        using var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }
}
