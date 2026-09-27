using System.Text.Json;
using SmartShop.Contracts.Events;
using SmartShop.Notification.Application.Common.Interfaces;
using SmartShop.Notification.Application.Features.Notifications.Commands.RecordNotification;

namespace SmartShop.Notification.Application.Features.Notifications.Email;

// Nơi duy nhất biết ánh xạ EventType -> method IEmailService cần gọi — dùng chung cho lần gửi đồng bộ đầu
// tiên (RecordNotificationCommandHandler) và lần retry sau (EmailRetryBackgroundService), tránh viết switch
// này 2 lần ở 2 nơi khác nhau.
public class NotificationEmailSender(IEmailService emailService)
{
    public static string? TryBuildPayloadJson(RecordNotificationCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.UserEmail) || request.SourceOrderId is null)
            return null;

        var toName = string.IsNullOrWhiteSpace(request.UserName) ? request.UserEmail : request.UserName;

        var payload = new EmailRetryPayload(
            ToEmail: request.UserEmail,
            ToName: toName,
            OrderNumber: request.OrderNumber,
            Items: request.Items,
            Amount: request.Amount,
            PaymentMethod: request.PaymentMethod,
            Reason: request.Reason);

        return JsonSerializer.Serialize(payload);
    }

    public async Task SendAsync(string eventType, string emailPayloadJson, Guid orderId, CancellationToken ct)
    {
        var payload = JsonSerializer.Deserialize<EmailRetryPayload>(emailPayloadJson)
            ?? throw new InvalidOperationException($"EmailPayloadJson for order {orderId} deserialized to null.");

        switch (eventType)
        {
            case nameof(OrderPlacedIntegrationEvent):
                await emailService.SendOrderPlacedAsync(
                    payload.ToEmail, payload.ToName, orderId,
                    payload.OrderNumber ?? orderId.ToString("N")[..8].ToUpper(),
                    payload.Items ?? [],
                    payload.Amount ?? 0m);
                break;

            case nameof(OrderCancelledIntegrationEvent):
                await emailService.SendOrderCancelledAsync(payload.ToEmail, payload.ToName, orderId);
                break;

            case nameof(PaymentCompletedIntegrationEvent):
                await emailService.SendPaymentCompletedAsync(
                    payload.ToEmail, payload.ToName, orderId,
                    payload.Amount ?? 0m, payload.PaymentMethod ?? string.Empty);
                break;

            case nameof(PaymentFailedIntegrationEvent):
                await emailService.SendPaymentFailedAsync(payload.ToEmail, payload.ToName, orderId, payload.Reason);
                break;

            default:
                throw new InvalidOperationException($"Unknown email event type '{eventType}' for order {orderId}.");
        }
    }
}
