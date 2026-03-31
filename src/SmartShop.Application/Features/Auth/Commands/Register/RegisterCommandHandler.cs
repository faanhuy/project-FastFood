using MediatR;
using SmartShop.Application.Common.Exceptions;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Features.Auth.Dtos;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork
) : IRequestHandler<RegisterCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await userRepository.ExistsAsync(request.Email, cancellationToken))
            throw new ConflictException($"Email '{request.Email}' đã được sử dụng.");

        var passwordHash = passwordHasher.Hash(request.Password);
        var user = User.Create(request.Email, passwordHash, request.FirstName, request.LastName);

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var token = jwtTokenService.GenerateToken(user);
        var refreshToken = jwtTokenService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        user.SetRefreshToken(refreshToken, refreshTokenExpiry);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(token, refreshToken, refreshTokenExpiry, user.Email, user.FirstName, user.LastName, user.Role);
    }
}
