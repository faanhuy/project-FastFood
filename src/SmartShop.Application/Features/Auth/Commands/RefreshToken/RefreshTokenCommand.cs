using MediatR;
using SmartShop.Application.Features.Auth.Dtos;

namespace SmartShop.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;
