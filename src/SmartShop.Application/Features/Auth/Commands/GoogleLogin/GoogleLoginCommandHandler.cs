using MediatR;
using Microsoft.AspNetCore.Http;
using SmartShop.Domain.Common;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Features.Auth.Dtos;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Auth.Commands.GoogleLogin;

public class GoogleLoginCommandHandler(
    IGoogleTokenValidator googleTokenValidator,
    IUserRepository userRepository,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork,
    ITokenHasher tokenHasher,
    IAuditLogService auditLogService,
    IHttpContextAccessor httpContextAccessor
) : IRequestHandler<GoogleLoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var ipAddress = GetClientIP();

        // Validate Google ID token
        var payload = await googleTokenValidator.ValidateAsync(request.IdToken, cancellationToken);
        if (payload == null)
        {
            await auditLogService.LogAsync(null, AuditActions.LoginFailed, "User", null, ipAddress: ipAddress, ct: cancellationToken);
            throw new UnauthorizedException("error.google_token_invalid", null);
        }

        // Check if email is verified by Google
        if (!payload.EmailVerified)
        {
            await auditLogService.LogAsync(null, AuditActions.LoginFailed, "User", null, ipAddress: ipAddress, ct: cancellationToken);
            throw new UnauthorizedException("error.google_email_not_verified", null);
        }

        // Find user by GoogleId first
        var user = await userRepository.GetByGoogleIdAsync(payload.GoogleUserId, cancellationToken);

        if (user == null)
        {
            // Check if email already exists (traditional user or different Google account)
            var existingUser = await userRepository.GetByEmailAsync(payload.Email, cancellationToken);
            if (existingUser != null)
            {
                await auditLogService.LogAsync(
                    null, AuditActions.LoginFailed, "User", null,
                    ipAddress: ipAddress, ct: cancellationToken);
                throw new ConflictException("error.email_already_registered");
            }

            // Auto-create new user
            user = User.CreateFromGoogle(
                payload.GoogleUserId,
                payload.Email,
                payload.FirstName,
                payload.LastName);
            await userRepository.AddAsync(user, cancellationToken);
        }

        // Check if user is banned
        if (user.IsBanned)
        {
            await auditLogService.LogAsync(
                user.Id, AuditActions.LoginFailed, "User", user.Id,
                ipAddress: ipAddress, ct: cancellationToken);
            throw new ConflictException("error.account_banned");
        }

        // Generate JWT tokens
        var token = jwtTokenService.GenerateToken(user);
        var refreshToken = jwtTokenService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(1);

        user.SetRefreshToken(refreshToken, refreshTokenExpiry, tokenHasher);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            user.Id, AuditActions.Login, "User", user.Id,
            ipAddress: ipAddress, ct: cancellationToken);

        return new AuthResponse(token, refreshToken, refreshTokenExpiry, user.Email, user.FirstName, user.LastName, user.Role);
    }

    private string GetClientIP()
    {
        return httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
