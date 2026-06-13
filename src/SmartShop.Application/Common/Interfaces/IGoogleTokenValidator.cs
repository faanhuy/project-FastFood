using SmartShop.Application.Features.Auth.Dtos;

namespace SmartShop.Application.Common.Interfaces;

public interface IGoogleTokenValidator
{
    Task<GoogleTokenValidationResult?> ValidateAsync(string idToken, CancellationToken ct = default);
}
