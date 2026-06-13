using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Features.Auth.Commands.ChangePassword;
using SmartShop.Application.Features.Auth.Commands.ForgotPassword;
using SmartShop.Application.Features.Auth.Commands.GoogleLogin;
using SmartShop.Application.Features.Auth.Commands.Login;
using SmartShop.Application.Features.Auth.Commands.RefreshToken;
using SmartShop.Application.Features.Auth.Commands.Register;
using SmartShop.Application.Features.Auth.Commands.RevokeToken;
using SmartShop.Application.Features.Auth.Dtos;

namespace SmartShop.WebAPI.Controllers;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record GoogleLoginRequest(string IdToken);

[ApiController]
[Route("api/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(Register), ApiResponse<AuthResponse>.Ok(result));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Ok(ApiResponse<AuthResponse>.Ok(result));
    }

    [HttpPost("google")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> GoogleLogin([FromBody] GoogleLoginRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new GoogleLoginCommand(request.IdToken), ct);
        return CreatedAtAction(nameof(GoogleLogin), ApiResponse<AuthResponse>.Ok(result));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Refresh([FromBody] RefreshTokenCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Ok(ApiResponse<AuthResponse>.Ok(result));
    }

    [HttpPost("revoke")]
    public async Task<ActionResult<ApiResponse<object?>>> Revoke([FromBody] RevokeTokenCommand command, CancellationToken ct)
    {
        await mediator.Send(command, ct);
        return Ok(ApiResponse.Ok("Token revoked successfully."));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<object?>>> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await mediator.Send(new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword), ct);
        return Ok(ApiResponse.Ok("Password changed successfully."));
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<object?>>> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken ct)
    {
        await mediator.Send(command, ct);
        return Ok(ApiResponse.Ok("If the email exists, a reset email has been sent."));
    }
}
