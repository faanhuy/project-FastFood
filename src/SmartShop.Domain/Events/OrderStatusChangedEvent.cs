using MediatR;

namespace SmartShop.Domain.Events;

public record OrderStatusChangedEvent(
    Guid OrderId,
    string UserId,
    string UserEmail,
    string UserName,
    string NewStatus) : INotification;
