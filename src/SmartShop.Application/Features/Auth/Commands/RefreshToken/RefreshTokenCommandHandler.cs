using MediatR;
using SmartShop.Application.Common.Exceptions;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Features.Auth.Dtos;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork
) : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (user is null || user.RefreshTokenExpiry < DateTime.UtcNow)
            throw new UnauthorizedException("Refresh token không hợp lệ hoặc đã hết hạn.");

        var newAccessToken = jwtTokenService.GenerateToken(user);
        var newRefreshToken = jwtTokenService.GenerateRefreshToken();
        var newExpiry = DateTime.UtcNow.AddDays(7);

        user.SetRefreshToken(newRefreshToken, newExpiry);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(newAccessToken, newRefreshToken, newExpiry, user.Email, user.FirstName, user.LastName, user.Role);
    }
}
