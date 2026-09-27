namespace SmartShop.Contracts.Events;

public record OrderCancelledIntegrationEvent(
    Guid OrderId,
    Guid UserId,
    string? UserEmail,
    string? UserName,
    DateTime OccurredAt);
