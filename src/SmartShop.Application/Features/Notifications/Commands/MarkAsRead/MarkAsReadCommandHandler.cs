using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Notifications.Commands.MarkAsRead;

public class MarkAsReadCommandHandler(
    INotificationRepository notificationRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IRequestHandler<MarkAsReadCommand, ApiResponse<bool>>
{
    public async Task<ApiResponse<bool>> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(currentUserService.UserId);

        if (request.NotificationId.HasValue)
        {
            var notification = await notificationRepository.GetByIdAsync(request.NotificationId.Value, cancellationToken)
                ?? throw new NotFoundException("Notification", request.NotificationId.Value);

            if (notification.UserId != userId)
                throw new UnauthorizedException("error.notification_access_unauthorized", null);

            notification.MarkAsRead();
        }
        else
        {
            var notifications = await notificationRepository.GetByUserIdAsync(userId, cancellationToken);
            foreach (var notification in notifications.Where(n => !n.IsRead))
                notification.MarkAsRead();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.Ok(true);
    }
}
