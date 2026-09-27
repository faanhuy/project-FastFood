namespace SmartShop.Contracts.Events;

public record OrderItemEventDto(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice);
