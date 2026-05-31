using MediatR;
using Microsoft.Extensions.Logging;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Domain.Events;

namespace SmartShop.Application.Features.Orders.EventHandlers;

public class SendOrderStatusUpdateEmailHandler(
    IEmailJobService emailJobService,
    ILogger<SendOrderStatusUpdateEmailHandler> logger) : INotificationHandler<OrderStatusChangedEvent>
{
    public Task Handle(OrderStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        emailJobService.EnqueueOrderStatusUpdate(
            toEmail: notification.UserEmail,
            toName: notification.UserName,
            orderId: notification.OrderId,
            orderNumber: notification.OrderId.ToString("N")[..8].ToUpper(),
            newStatus: notification.NewStatus);

        logger.LogInformation("Enqueued order status update email for order {OrderId}", notification.OrderId);
        return Task.CompletedTask;
    }
}
