using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Events;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Orders.EventHandlers;

public class PushSignalRNotificationHandler(
    INotificationHubService hubService,
    INotificationRepository notificationRepository,
    IUnitOfWork unitOfWork,
    ILogger<PushSignalRNotificationHandler> logger) : INotificationHandler<OrderStatusChangedEvent>
{
    public async Task Handle(OrderStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var orderCode = notification.OrderId.ToString()[..8].ToUpper();
            const string titleKey = "notification.orderStatusChangedTitle";
            const string messageKey = "notification.orderStatusChangedBody";
            var paramsJson = JsonSerializer.Serialize(new { orderCode, status = notification.NewStatus });

            var dbNotification = Notification.Create(
                userId: notification.UserId,
                titleKey: titleKey,
                messageKey: messageKey,
                paramsJson: paramsJson,
                orderId: notification.OrderId);

            await notificationRepository.AddAsync(dbNotification, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var payload = new
            {
                NotificationId = dbNotification.Id,
                TitleKey = titleKey,
                MessageKey = messageKey,
                Params = paramsJson,
                OrderId = notification.OrderId
            };

            await hubService.SendToUserAsync(notification.UserId.ToString(), "OrderStatusUpdated", payload, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Push SignalR notification cho đơn hàng {OrderId} thất bại.",
                notification.OrderId);
        }
    }
}
