using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Common.Models;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Notifications.Queries.GetNotifications;

public class GetNotificationsQueryHandler(
    INotificationRepository notificationRepository,
    ICurrentUserService currentUserService) : IRequestHandler<GetNotificationsQuery, ApiResponse<List<NotificationDto>>>
{
    public async Task<ApiResponse<List<NotificationDto>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        var notifications = await notificationRepository.GetByUserIdAsync(userId, cancellationToken);

        var dtos = notifications.Select(n => new NotificationDto(
            Id: n.Id,
            Title: n.Title,
            Message: n.Message,
            IsRead: n.IsRead,
            OrderId: n.OrderId,
            CreatedAt: n.CreatedAt
        )).ToList();

        return ApiResponse<List<NotificationDto>>.Ok(dtos);
    }
}
