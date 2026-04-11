using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Features.Users;
using SmartShop.Application.Features.Users.Commands.UpdateMyProfile;
using SmartShop.Application.Features.Users.Queries.GetMyProfile;

namespace SmartShop.WebAPI.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(IMediator mediator) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Lấy thông tin profile của user hiện tại</summary>
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetMyProfile(CancellationToken ct)
    {
        var result = await mediator.Send(new GetMyProfileQuery(CurrentUserId), ct);
        return Ok(ApiResponse<UserProfileDto>.Ok(result));
    }

    /// <summary>Cập nhật họ tên của user hiện tại</summary>
    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> UpdateMyProfile(
        [FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new UpdateMyProfileCommand(CurrentUserId, request.FirstName, request.LastName), ct);
        return Ok(ApiResponse<UserProfileDto>.Ok(result));
    }
}

public record UpdateProfileRequest(string FirstName, string LastName);
