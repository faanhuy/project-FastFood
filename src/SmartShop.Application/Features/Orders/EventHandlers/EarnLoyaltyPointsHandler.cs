using MediatR;
using Microsoft.Extensions.Logging;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Events;
using SmartShop.Domain.Interfaces;
using SmartShop.Application.Interfaces;

namespace SmartShop.Application.Features.Orders.EventHandlers;

public class EarnLoyaltyPointsHandler(
    ILoyaltyRepository loyaltyRepository,
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    ILogger<EarnLoyaltyPointsHandler> logger) : INotificationHandler<OrderStatusChangedEvent>
{
    public async Task Handle(OrderStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        // Only earn points when order reaches Delivered status
        if (notification.NewStatus != OrderStatus.Delivered.ToString())
            return;

        var order = await orderRepository.GetByIdAsync(notification.OrderId, cancellationToken);
        if (order is null)
        {
            logger.LogWarning("Order {OrderId} not found for loyalty points", notification.OrderId);
            return;
        }

        var account = await loyaltyRepository.GetByUserIdAsync(order.UserId, cancellationToken);
        if (account is null)
        {
            logger.LogWarning("Loyalty account not found for user {UserId}", order.UserId);
            return;
        }

        var pointsToEarn = (decimal)Math.Floor(order.TotalAmount / 1000) * 10;

        if (pointsToEarn > 0)
        {
            account.EarnPoints(pointsToEarn, order.Id, $"Delivered order {order.Id:N}");
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Earned {Points} loyalty points for user {UserId} from order {OrderId}",
                pointsToEarn, order.UserId, order.Id);
        }
    }
}
