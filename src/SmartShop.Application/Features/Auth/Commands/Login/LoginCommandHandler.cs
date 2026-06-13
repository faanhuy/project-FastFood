using MediatR;
using Microsoft.AspNetCore.Http;
using SmartShop.Domain.Common;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Features.Auth.Dtos;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork,
    ITokenHasher tokenHasher,
    IAuditLogService auditLogService,
    IHttpContextAccessor httpContextAccessor
) : IRequestHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        var ipAddress = GetClientIP();

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            await auditLogService.LogAsync(null, AuditActions.LoginFailed, "User", null, ipAddress: ipAddress, ct: cancellationToken);
            throw new UnauthorizedException("error.auth_invalid_credentials", null);
        }

        if (user.IsBanned)
        {
            await auditLogService.LogAsync(user.Id, AuditActions.LoginFailed, "User", user.Id, ipAddress: ipAddress, ct: cancellationToken);
            throw new ConflictException("error.account_banned_by_admin", null);
        }

        var token = jwtTokenService.GenerateToken(user);
        var refreshToken = jwtTokenService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(1);

        user.SetRefreshToken(refreshToken, refreshTokenExpiry, tokenHasher);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(user.Id, AuditActions.Login, "User", user.Id, ipAddress: ipAddress, ct: cancellationToken);

        return new AuthResponse(token, refreshToken, refreshTokenExpiry, user.Email, user.FirstName, user.LastName, user.Role);
    }

    private string GetClientIP()
    {
        return httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
