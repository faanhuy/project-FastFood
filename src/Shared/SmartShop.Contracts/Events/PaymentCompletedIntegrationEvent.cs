namespace SmartShop.Contracts.Events;

public record PaymentCompletedIntegrationEvent(
    Guid OrderId,
    Guid UserId,
    string? UserEmail,
    string? UserName,
    string? TransactionId,
    decimal Amount,
    string Method,
    DateTime OccurredAt);
