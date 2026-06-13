using MediatR;
using SmartShop.Application.Common.Models;

namespace SmartShop.Application.Features.Notifications.Queries.GetNotifications;

public record GetNotificationsQuery : IRequest<ApiResponse<List<NotificationDto>>>;

public record NotificationDto(
    Guid Id,
    string TitleKey,
    string MessageKey,
    string? Params,
    string? Title,
    string? Message,
    bool IsRead,
    Guid? OrderId,
    DateTime CreatedAt);
