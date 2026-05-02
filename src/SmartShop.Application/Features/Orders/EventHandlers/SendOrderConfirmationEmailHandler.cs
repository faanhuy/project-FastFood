using MediatR;
using Microsoft.Extensions.Logging;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Domain.Events;

namespace SmartShop.Application.Features.Orders.EventHandlers;

public class SendOrderConfirmationEmailHandler(
    IEmailService emailService,
    ILogger<SendOrderConfirmationEmailHandler> logger) : INotificationHandler<OrderPlacedEvent>
{
    public async Task Handle(OrderPlacedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var items = notification.Items
                .Select(i => new OrderItemInfo(i.ProductName, i.Quantity, i.UnitPrice))
                .ToList();

            await emailService.SendOrderConfirmationAsync(
                toEmail: notification.UserEmail,
                toName: notification.UserName,
                orderId: notification.OrderId,
                orderNumber: notification.OrderId.ToString("N")[..8].ToUpper(),
                items: items,
                totalPrice: notification.TotalPrice);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Gửi email xác nhận đơn hàng {OrderId} thất bại. Email: {Email}",
                notification.OrderId, notification.UserEmail);
        }
    }
}
