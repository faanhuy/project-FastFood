using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Domain.Events;

namespace SmartShop.Application.Features.Orders.EventHandlers;

public class SendOrderConfirmationEmailHandler(
    IEmailJobService emailJobService,
    IConfiguration configuration,
    ILogger<SendOrderConfirmationEmailHandler> logger) : INotificationHandler<OrderPlacedEvent>
{
    public Task Handle(OrderPlacedEvent notification, CancellationToken cancellationToken)
    {
        // Notification Service (Kafka) cũng có thể gửi email này (config `Notification:SendOrderPlacedEmail`,
        // mặc định tắt) — 2 flag đi thành cặp, đảo cả 2 để chuyển hẳn trách nhiệm gửi mà không cần sửa code.
        if (!configuration.GetValue("Notification:CoreSendsOrderPlacedEmail", true))
        {
            logger.LogInformation(
                "Skipping Core order confirmation email for order {OrderId} (Notification:CoreSendsOrderPlacedEmail disabled).",
                notification.OrderId);
            return Task.CompletedTask;
        }

        var items = notification.Items
            .Select(i => new OrderItemInfo(i.ProductName, i.Quantity, i.UnitPrice))
            .ToList();

        emailJobService.EnqueueOrderConfirmation(
            toEmail: notification.UserEmail,
            toName: notification.UserName,
            orderId: notification.OrderId,
            orderNumber: notification.OrderId.ToString("N")[..8].ToUpper(),
            items: items,
            totalPrice: notification.TotalPrice);

        logger.LogInformation("Enqueued order confirmation email for order {OrderId}", notification.OrderId);
        return Task.CompletedTask;
    }
}
