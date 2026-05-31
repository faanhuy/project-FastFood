using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Notifications.Commands.DeleteNotification;

public class DeleteNotificationCommandHandler(
    INotificationRepository notificationRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IRequestHandler<DeleteNotificationCommand, ApiResponse<bool>>
{
    public async Task<ApiResponse<bool>> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(currentUserService.UserId);

        if (request.NotificationId.HasValue)
        {
            var notification = await notificationRepository.GetByIdAsync(request.NotificationId.Value, cancellationToken)
                ?? throw new NotFoundException("Notification", request.NotificationId.Value);

            if (notification.UserId != userId)
                throw new UnauthorizedException("error.notification_delete_unauthorized", null);

            await notificationRepository.DeleteAsync(notification, cancellationToken);
        }
        else
        {
            await notificationRepository.DeleteAllByUserIdAsync(userId, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.Ok(true);
    }
}
