using MediatR;

namespace SmartShop.Domain.Events;

public record OrderStatusChangedEvent(
    Guid OrderId,
    Guid UserId,
    string UserEmail,
    string UserName,
    string NewStatus) : INotification;
