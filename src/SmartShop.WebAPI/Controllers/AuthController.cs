using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Features.Auth.Commands.Login;
using SmartShop.Application.Features.Auth.Commands.RefreshToken;
using SmartShop.Application.Features.Auth.Commands.Register;
using SmartShop.Application.Features.Auth.Commands.RevokeToken;
using SmartShop.Application.Features.Auth.Dtos;

namespace SmartShop.WebAPI.Controllers;

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
}
