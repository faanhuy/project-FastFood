using MediatR;
using Microsoft.Extensions.Logging;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Domain.Events;

namespace SmartShop.Application.Features.Returns.EventHandlers;

public class SendReturnRejectedEmailHandler(
    IEmailJobService emailJobService,
    ILogger<SendReturnRejectedEmailHandler> logger) : INotificationHandler<ReturnRejectedEvent>
{
    public Task Handle(ReturnRejectedEvent notification, CancellationToken cancellationToken)
    {
        emailJobService.EnqueueReturnRejected(
            notification.UserEmail,
            notification.UserName,
            notification.OrderId,
            notification.OrderId.ToString("N")[..8].ToUpper(),
            notification.AdminNote);

        logger.LogInformation("Enqueued return rejected email for order {OrderId}", notification.OrderId);
        return Task.CompletedTask;
    }
}
