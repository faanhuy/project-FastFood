using MediatR;
using SmartShop.Application.Features.Auth.Dtos;

namespace SmartShop.Application.Features.Auth.Commands.GoogleLogin;

public record GoogleLoginCommand(string IdToken) : IRequest<AuthResponse>;
