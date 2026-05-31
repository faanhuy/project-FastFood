using MediatR;
using Microsoft.Extensions.Logging;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Domain.Events;

namespace SmartShop.Application.Features.Orders.EventHandlers;

public class SendOrderConfirmationEmailHandler(
    IEmailJobService emailJobService,
    ILogger<SendOrderConfirmationEmailHandler> logger) : INotificationHandler<OrderPlacedEvent>
{
    public Task Handle(OrderPlacedEvent notification, CancellationToken cancellationToken)
    {
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
