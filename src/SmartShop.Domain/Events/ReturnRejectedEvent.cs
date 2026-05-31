using MediatR;

namespace SmartShop.Domain.Events;

public record ReturnRejectedEvent(
    Guid ReturnRequestId,
    Guid OrderId,
    Guid UserId,
    string UserEmail,
    string UserName,
    string AdminNote) : INotification;
