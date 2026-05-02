using MediatR;
using SmartShop.Application.Common.Models;

namespace SmartShop.Application.Features.Notifications.Commands.MarkAsRead;

public record MarkAsReadCommand(Guid? NotificationId) : IRequest<ApiResponse<bool>>;
