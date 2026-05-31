using MediatR;
using Microsoft.Extensions.Logging;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Domain.Events;

namespace SmartShop.Application.Features.Returns.EventHandlers;

public class SendReturnApprovedEmailHandler(
    IEmailJobService emailJobService,
    ILogger<SendReturnApprovedEmailHandler> logger) : INotificationHandler<ReturnApprovedEvent>
{
    public Task Handle(ReturnApprovedEvent notification, CancellationToken cancellationToken)
    {
        emailJobService.EnqueueReturnApproved(
            notification.UserEmail,
            notification.UserName,
            notification.OrderId,
            notification.OrderId.ToString("N")[..8].ToUpper(),
            notification.RefundAmount,
            notification.AdminNote);

        logger.LogInformation("Enqueued return approved email for order {OrderId}", notification.OrderId);
        return Task.CompletedTask;
    }
}
