using System.Text.Json;
using MediatR;
using SmartShop.Contracts.Events;
using SmartShop.Notification.Application.Common.Interfaces;
using SmartShop.Notification.Application.Features.Notifications.Commands.RecordNotification;

namespace SmartShop.Notification.Application.IntegrationEvents;

/// <summary>
/// Dịch 1 Kafka event sang <see cref="RecordNotificationCommand"/> — nơi duy nhất biết ánh xạ
/// topic/EventType → TitleKey/MessageKey. Không biết gì về Kafka: consumer đưa vào (topic, payload).
/// </summary>
public class NotificationFanOutEventHandler(ISender sender)
{
    /// <summary>Các topic handler này hiểu được — consumer subscribe đúng danh sách này.</summary>
    public static IReadOnlyList<string> Topics { get; } =
    [
        EventTopics.OrderPlaced,
        EventTopics.OrderCancelled,
        EventTopics.PaymentCompleted,
        EventTopics.PaymentFailed
    ];

    // Core serialize event bằng System.Text.Json mặc định (PascalCase); Web defaults đọc không phân biệt hoa/thường.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <exception cref="InvalidIntegrationEventException">Message hỏng hoặc thiếu UserId — không nên retry.</exception>
    public async Task HandleAsync(string topic, string payload, CancellationToken ct)
    {
        var command = topic switch
        {
            EventTopics.OrderPlaced => Map(Deserialize<OrderPlacedIntegrationEvent>(payload)),
            EventTopics.OrderCancelled => Map(Deserialize<OrderCancelledIntegrationEvent>(payload)),
            EventTopics.PaymentCompleted => Map(Deserialize<PaymentCompletedIntegrationEvent>(payload)),
            EventTopics.PaymentFailed => Map(Deserialize<PaymentFailedIntegrationEvent>(payload)),
            _ => throw new InvalidIntegrationEventException($"Unexpected topic '{topic}'.")
        };

        if (command.UserId == Guid.Empty)
            throw new InvalidIntegrationEventException($"Event on '{topic}' has an empty UserId.");

        await sender.Send(command, ct);
    }

    private static RecordNotificationCommand Map(OrderPlacedIntegrationEvent e) => new(
        UserId: e.UserId,
        EventType: nameof(OrderPlacedIntegrationEvent),
        TitleKey: "notification.orderPlacedTitle",
        MessageKey: "notification.orderPlacedBody",
        ParamsJson: JsonSerializer.Serialize(new { totalAmount = e.TotalAmount }),
        SourceOrderId: e.OrderId,
        UserEmail: e.UserEmail,
        UserName: e.UserName,
        Amount: e.TotalAmount,
        PaymentMethod: null,
        Reason: null,
        OrderNumber: e.OrderId.ToString("N")[..8].ToUpper(),
        Items: e.Items
            .Select(i => new NotificationOrderItemInfo(i.ProductName, i.Quantity, i.UnitPrice))
            .ToList());

    private static RecordNotificationCommand Map(OrderCancelledIntegrationEvent e) => new(
        UserId: e.UserId,
        EventType: nameof(OrderCancelledIntegrationEvent),
        TitleKey: "notification.orderCancelledTitle",
        MessageKey: "notification.orderCancelledBody",
        ParamsJson: null,
        SourceOrderId: e.OrderId,
        UserEmail: e.UserEmail,
        UserName: e.UserName,
        Amount: null,
        PaymentMethod: null,
        Reason: null);

    private static RecordNotificationCommand Map(PaymentCompletedIntegrationEvent e) => new(
        UserId: e.UserId,
        EventType: nameof(PaymentCompletedIntegrationEvent),
        TitleKey: "notification.paymentCompletedTitle",
        MessageKey: "notification.paymentCompletedBody",
        ParamsJson: JsonSerializer.Serialize(new { amount = e.Amount, method = e.Method }),
        SourceOrderId: e.OrderId,
        UserEmail: e.UserEmail,
        UserName: e.UserName,
        Amount: e.Amount,
        PaymentMethod: e.Method,
        Reason: null);

    private static RecordNotificationCommand Map(PaymentFailedIntegrationEvent e) => new(
        UserId: e.UserId,
        EventType: nameof(PaymentFailedIntegrationEvent),
        TitleKey: "notification.paymentFailedTitle",
        MessageKey: "notification.paymentFailedBody",
        ParamsJson: e.Reason is null ? null : JsonSerializer.Serialize(new { reason = e.Reason }),
        SourceOrderId: e.OrderId,
        UserEmail: e.UserEmail,
        UserName: e.UserName,
        Amount: null,
        PaymentMethod: null,
        Reason: e.Reason);

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
