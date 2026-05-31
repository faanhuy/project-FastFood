using MediatR;
using Microsoft.AspNetCore.Http;
using SmartShop.Domain.Common;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Features.Auth.Dtos;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork,
    ITokenHasher tokenHasher,
    IAuditLogService auditLogService,
    IHttpContextAccessor httpContextAccessor
) : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var ipAddress = GetClientIP();
        var tokenHash = tokenHasher.Hash(request.RefreshToken);
        var user = await userRepository.GetByRefreshTokenHashAsync(tokenHash, cancellationToken)
            ?? await userRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (user is null || user.RefreshTokenExpiry < DateTime.UtcNow)
        {
            await auditLogService.LogAsync(null, AuditActions.RefreshTokenFailed, "User", null, ipAddress: ipAddress, ct: cancellationToken);
            throw new UnauthorizedException("error.auth_refresh_token_invalid", null);
        }

        var newAccessToken = jwtTokenService.GenerateToken(user);
        var newRefreshToken = jwtTokenService.GenerateRefreshToken();
        var newExpiry = DateTime.UtcNow.AddDays(1);

        user.SetRefreshToken(newRefreshToken, newExpiry, tokenHasher);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(user.Id, AuditActions.TokenRefreshed, "User", user.Id, ipAddress: ipAddress, ct: cancellationToken);

        return new AuthResponse(newAccessToken, newRefreshToken, newExpiry, user.Email, user.FirstName, user.LastName, user.Role);
    }

    private string GetClientIP()
    {
        return httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
