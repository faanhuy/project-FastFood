using System.Text.Json;
using MediatR;
using SmartShop.Contracts.Events;
using SmartShop.Inventory.Application.Features.Stock.Commands.ReleaseStock;

namespace SmartShop.Inventory.Application.IntegrationEvents;

/// <summary>
/// Nhả chỗ đã giữ khi Core báo đơn hàng bị hủy.
/// Không biết gì về Kafka: consumer đưa vào (topic, payload), class này dịch sang <see cref="ReleaseStockCommand"/>
/// — cùng đường xử lý với RPC ReleaseStock, và idempotent theo order_id nên nhận trùng message vẫn an toàn.
///
/// Cố ý KHÔNG nhả khi thanh toán thất bại: đơn vẫn còn và khách có thể thanh toán lại, trong khi Core không hủy đơn
/// và không hoàn tồn kho Core lúc đó. Nhả chỗ sớm thì đơn được trả tiền sau đó sẽ không còn được giữ chỗ ở Inventory.
/// </summary>
public class StockReleaseEventHandler(ISender sender)
{
    /// <summary>Các topic handler này hiểu được — consumer subscribe đúng danh sách này.</summary>
    public static IReadOnlyList<string> Topics { get; } = [EventTopics.OrderCancelled];

    // Core serialize event bằng System.Text.Json mặc định (PascalCase); Web defaults đọc không phân biệt hoa/thường.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <exception cref="InvalidIntegrationEventException">Message hỏng — không nên retry.</exception>
    public async Task HandleAsync(string topic, string payload, CancellationToken ct)
    {
        var orderId = topic switch
        {
            EventTopics.OrderCancelled => Deserialize<OrderCancelledIntegrationEvent>(payload).OrderId,
            _ => throw new InvalidIntegrationEventException($"Unexpected topic '{topic}'.")
        };

        if (orderId == Guid.Empty)
            throw new InvalidIntegrationEventException($"Event on '{topic}' has an empty OrderId.");

        await sender.Send(new ReleaseStockCommand(orderId), ct);
    }

    private static T Deserialize<T>(string payload) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(payload, JsonOptions)
                ?? throw new InvalidIntegrationEventException($"Payload for {typeof(T).Name} is null.");
        }
        catch (JsonException ex)
        {
            throw new InvalidIntegrationEventException($"Payload for {typeof(T).Name} is not valid JSON.", ex);
        }
    }
}
