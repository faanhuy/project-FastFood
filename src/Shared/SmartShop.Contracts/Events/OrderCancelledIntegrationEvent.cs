namespace SmartShop.Contracts.Events;

public record OrderCancelledIntegrationEvent(
    Guid OrderId,
    Guid UserId,
    DateTime OccurredAt);
