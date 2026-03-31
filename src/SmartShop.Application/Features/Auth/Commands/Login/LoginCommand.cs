using MediatR;
using SmartShop.Application.Features.Auth.Dtos;

namespace SmartShop.Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<AuthResponse>;
