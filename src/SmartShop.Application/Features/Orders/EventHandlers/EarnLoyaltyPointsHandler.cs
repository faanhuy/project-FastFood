using MediatR;
using Microsoft.Extensions.Logging;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Events;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Orders.EventHandlers;

public class EarnLoyaltyPointsHandler(
    ILoyaltyService loyaltyService,
    IOrderRepository orderRepository,
    ILogger<EarnLoyaltyPointsHandler> logger) : INotificationHandler<OrderStatusChangedEvent>
{
    public async Task Handle(OrderStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.NewStatus != OrderStatus.Delivered.ToString())
            return;

        var order = await orderRepository.GetByIdAsync(notification.OrderId, cancellationToken);
        if (order is null)
        {
            logger.LogWarning("Order {OrderId} not found for loyalty points", notification.OrderId);
            return;
        }

        await loyaltyService.EarnPointsAsync(order.UserId, order.Id, order.TotalAmount, cancellationToken);

        logger.LogInformation(
            "Earned loyalty points for user {UserId} from order {OrderId}",
            order.UserId, order.Id);
    }
}
