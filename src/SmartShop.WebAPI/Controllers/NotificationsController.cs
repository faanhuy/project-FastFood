using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Features.Notifications.Commands.MarkAsRead;
using SmartShop.Application.Features.Notifications.Queries.GetNotifications;

namespace SmartShop.WebAPI.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(IMediator mediator) : ControllerBase
{
    /// <summary>Lấy danh sách thông báo của user hiện tại</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> GetNotifications(CancellationToken ct)
    {
        var result = await mediator.Send(new GetNotificationsQuery(), ct);
        return Ok(result);
    }

    /// <summary>Đánh dấu đã đọc một thông báo</summary>
    [HttpPatch("{id:guid}/read")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new MarkAsReadCommand(id), ct);
        return Ok(result);
    }

    /// <summary>Đánh dấu tất cả thông báo đã đọc</summary>
    [HttpPatch("read-all")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAllAsRead(CancellationToken ct)
    {
        var result = await mediator.Send(new MarkAsReadCommand(null), ct);
        return Ok(result);
    }
}
