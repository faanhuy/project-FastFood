using MediatR;
using SmartShop.Application.Features.Auth.Dtos;

namespace SmartShop.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName
) : IRequest<AuthResponse>;
