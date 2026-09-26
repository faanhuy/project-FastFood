namespace SmartShop.Contracts.Events;

public record OrderItemEventDto(Guid ProductId, int Quantity);
