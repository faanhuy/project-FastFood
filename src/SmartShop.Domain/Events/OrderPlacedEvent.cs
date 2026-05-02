using MediatR;

namespace SmartShop.Domain.Events;

public record OrderEventItemDto(string ProductName, int Quantity, decimal UnitPrice);

public record OrderPlacedEvent(
    Guid OrderId,
    string UserId,
    string UserEmail,
    string UserName,
    decimal TotalPrice,
    List<OrderEventItemDto> Items) : INotification;
