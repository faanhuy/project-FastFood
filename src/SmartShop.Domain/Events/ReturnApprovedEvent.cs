using MediatR;

namespace SmartShop.Domain.Events;

public record ReturnApprovedEvent(
    Guid ReturnRequestId,
    Guid OrderId,
    Guid UserId,
    string UserEmail,
    string UserName,
    decimal RefundAmount,
    string? AdminNote) : INotification;
