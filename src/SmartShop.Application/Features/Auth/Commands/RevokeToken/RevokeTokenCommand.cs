using MediatR;

namespace SmartShop.Application.Features.Auth.Commands.RevokeToken;

public record RevokeTokenCommand(string RefreshToken) : IRequest;
