using MediatR;
using SmartShop.Application.Common.Models;

namespace SmartShop.Application.Features.Notifications.Commands.DeleteNotification;

/// <summary>
/// Xóa một thông báo (NotificationId có giá trị) hoặc xóa tất cả thông báo của user (NotificationId = null).
/// </summary>
public record DeleteNotificationCommand(Guid? NotificationId) : IRequest<ApiResponse<bool>>;
