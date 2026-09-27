namespace SmartShop.Contracts.Events;

public record PaymentFailedIntegrationEvent(
    Guid OrderId,
    Guid UserId,
    string? UserEmail,
    string? UserName,
    string? Reason,
    DateTime OccurredAt);
