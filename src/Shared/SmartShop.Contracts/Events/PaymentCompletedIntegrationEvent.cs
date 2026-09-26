namespace SmartShop.Contracts.Events;

public record PaymentCompletedIntegrationEvent(
    Guid OrderId,
    string? TransactionId,
    decimal Amount,
    string Method,
    DateTime OccurredAt);
