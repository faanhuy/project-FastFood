namespace SmartShop.Contracts.Events;

public record OrderPlacedIntegrationEvent(
    Guid OrderId,
    Guid UserId,
    Guid? StoreId,
    decimal TotalAmount,
    List<OrderItemEventDto> Items,
    DateTime OccurredAt);
