using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Features.Admin.Users;
using SmartShop.Application.Features.Admin.Users.Commands.BanUser;
using SmartShop.Application.Features.Admin.Users.Commands.BulkUpdateUsers;
using SmartShop.Application.Features.Admin.Users.Commands.ForceLogout;
using SmartShop.Application.Features.Admin.Users.Commands.ResetPassword;
using SmartShop.Application.Features.Admin.Users.Commands.UnbanUser;
using SmartShop.Application.Features.Admin.Users.Commands.UpdateUserRole;
using SmartShop.Application.Features.Admin.Users.Queries.GetUserDetail;
using SmartShop.Application.Features.Admin.Users.Queries.GetUsers;
using SmartShop.Application.Features.Common;
using SmartShop.Domain.Interfaces;
using System.Security.Claims;

namespace SmartShop.WebAPI.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? roleFilter = null,
        [FromQuery] bool? bannedFilter = null,
        [FromQuery] string? searchEmail = null,
        [FromQuery] string sortBy = "createdAt",
        [FromQuery] string sortDirection = "desc",
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetUsersQuery(page, pageSize, roleFilter, bannedFilter, searchEmail, sortBy, sortDirection),
            cancellationToken);
        return Ok(ApiResponse<PagedResult<UserDto>>.Ok(result));
    }

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetUserDetail(Guid userId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserDetailQuery(userId), cancellationToken);
        return Ok(ApiResponse<UserDto>.Ok(result));
    }

    [HttpPatch("{userId:guid}/ban")]
    public async Task<IActionResult> BanUser(Guid userId, CancellationToken cancellationToken)
    {
        var requestingUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await mediator.Send(new BanUserCommand(userId, requestingUserId), cancellationToken);
        return Ok(ApiResponse<UserDto>.Ok(result));
    }

    [HttpPatch("{userId:guid}/unban")]
    public async Task<IActionResult> UnbanUser(Guid userId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UnbanUserCommand(userId), cancellationToken);
        return Ok(ApiResponse<UserDto>.Ok(result));
    }

    [HttpPatch("{userId:guid}/role")]
    public async Task<IActionResult> UpdateUserRole(
        Guid userId,
        [FromBody] UpdateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var requestingUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await mediator.Send(
            new UpdateUserRoleCommand(userId, requestingUserId, request.NewRole),
            cancellationToken);
        return Ok(ApiResponse<UserDto>.Ok(result));
    }

    [HttpPost("bulk-actions")]
    public async Task<IActionResult> BulkUpdateUsers(
        [FromBody] BulkUpdateUsersRequest request,
        CancellationToken cancellationToken)
    {
        var adminUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await mediator.Send(
            new BulkUpdateUsersCommand(request.UserIds, request.Action, adminUserId, request.RoleValue),
            cancellationToken);
        return Ok(ApiResponse<BulkActionResult>.Ok(result));
    }

    [HttpPost("{userId:guid}/force-logout")]
    public async Task<IActionResult> ForceLogout(Guid userId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ForceLogoutCommand(userId), cancellationToken);
        return Ok(ApiResponse<ForceLogoutResult>.Ok(result));
    }

    [HttpPost("{userId:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid userId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ResetPasswordCommand(userId), cancellationToken);
        return Ok(ApiResponse<ResetPasswordResult>.Ok(result));
    }
}

public record UpdateRoleRequest(string NewRole);

public record BulkUpdateUsersRequest(
    List<Guid> UserIds,
    string Action,
    string? RoleValue = null
);
