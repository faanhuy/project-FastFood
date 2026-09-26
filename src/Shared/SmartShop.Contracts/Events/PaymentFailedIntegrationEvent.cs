namespace SmartShop.Contracts.Events;

public record PaymentFailedIntegrationEvent(
    Guid OrderId,
    string? Reason,
    DateTime OccurredAt);
