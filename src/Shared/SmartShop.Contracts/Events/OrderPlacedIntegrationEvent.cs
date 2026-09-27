namespace SmartShop.Contracts.Events;

public record OrderPlacedIntegrationEvent(
    Guid OrderId,
    Guid UserId,
    Guid? StoreId,
    decimal TotalAmount,
    string? UserEmail,
    string? UserName,
    List<OrderItemEventDto> Items,
    DateTime OccurredAt);
