using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Events;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Returns.Commands.RejectReturn;

public class RejectReturnCommandHandler(
    IReturnRequestRepository returnRequestRepository,
    IOrderRepository orderRepository,
    INotificationRepository notificationRepository,
    IUserRepository userRepository,
    INotificationHubService hubService,
    IMediator mediator,
    IUnitOfWork unitOfWork,
    ILogger<RejectReturnCommandHandler> logger) : IRequestHandler<RejectReturnCommand, ReturnRequestDto>
{
    public async Task<ReturnRequestDto> Handle(
        RejectReturnCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.AdminNote, nameof(request.AdminNote));

        var returnRequest = await returnRequestRepository.GetByIdAsync(request.ReturnRequestId, cancellationToken)
            ?? throw new NotFoundException("Return Request", request.ReturnRequestId);

        if (returnRequest.Status != ReturnStatus.Pending)
            throw new ConflictException("error.return_reject_invalid_status", null);

        var order = await orderRepository.GetByIdAsync(returnRequest.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), returnRequest.OrderId);

        returnRequest.Reject(request.AdminNote);
        returnRequestRepository.Update(returnRequest);

        var userIdString = returnRequest.UserId.ToString();
        var orderCode = order.Id.ToString()[..8].ToUpper();
        const string titleKey = "notification.returnRejectedTitle";
        const string messageKey = "notification.returnRejectedBody";
        var paramsJson = JsonSerializer.Serialize(new { orderCode, reason = request.AdminNote });

        var notification = Notification.Create(returnRequest.UserId, titleKey, messageKey, paramsJson, order.Id);
        await notificationRepository.AddAsync(notification, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish event to send email
        var user = await userRepository.GetByIdAsync(returnRequest.UserId, cancellationToken);
        if (user is not null)
        {
            await mediator.Publish(new ReturnRejectedEvent(
                ReturnRequestId: returnRequest.Id,
                OrderId: order.Id,
                UserId: user.Id,
                UserEmail: user.Email,
                UserName: $"{user.FirstName} {user.LastName}".Trim(),
                AdminNote: request.AdminNote), cancellationToken);
        }

        try
        {
            await hubService.SendToUserAsync(userIdString, "ReturnRequestUpdated", new
            {
                NotificationId = notification.Id,
                TitleKey = titleKey,
                MessageKey = messageKey,
                Params = paramsJson,
                OrderId = order.Id,
                Status = "Rejected"
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Push SignalR notification cho return request {Id} thất bại.", returnRequest.Id);
        }

        return ReturnRequestMapper.ToDto(returnRequest, order.Id.ToString());
    }
}
