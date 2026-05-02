using MediatR;
using Microsoft.Extensions.Logging;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Domain.Events;

namespace SmartShop.Application.Features.Orders.EventHandlers;

public class SendOrderStatusUpdateEmailHandler(
    IEmailService emailService,
    ILogger<SendOrderStatusUpdateEmailHandler> logger) : INotificationHandler<OrderStatusChangedEvent>
{
    public async Task Handle(OrderStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            await emailService.SendOrderStatusUpdateAsync(
                toEmail: notification.UserEmail,
                toName: notification.UserName,
                orderId: notification.OrderId,
                orderNumber: notification.OrderId.ToString("N")[..8].ToUpper(),
                newStatus: notification.NewStatus);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Gửi email cập nhật trạng thái đơn hàng {OrderId} thất bại. Email: {Email}",
                notification.OrderId, notification.UserEmail);
        }
    }
}
