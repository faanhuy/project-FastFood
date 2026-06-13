using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Features.Auth.Dtos;

namespace SmartShop.Infrastructure.Services;

public class GoogleTokenValidator : IGoogleTokenValidator
{
    private static readonly string GoogleIssuer = "https://accounts.google.com";
    private static readonly string GoogleJwksUrl = "https://www.googleapis.com/oauth2/v3/certs";
    private static readonly TimeSpan JwksCacheDuration = TimeSpan.FromHours(24);

    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private ICollection<SecurityKey>? _googleKeys;
    private DateTime _keysExpiry = DateTime.MinValue;

    public GoogleTokenValidator(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<GoogleTokenValidationResult?> ValidateAsync(string idToken, CancellationToken ct = default)
    {
        try
        {
            // Load Google's public keys
            var keys = await GetGoogleKeysAsync(ct);
            if (keys == null || keys.Count == 0)
                return null;

            var clientId = _configuration["GoogleAuth:ClientId"];
            if (string.IsNullOrEmpty(clientId))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = keys,
                ValidateIssuer = true,
                ValidIssuer = GoogleIssuer,
                ValidateAudience = true,
                ValidAudience = clientId,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            var principal = tokenHandler.ValidateToken(idToken, validationParameters, out var validatedToken);

            // Extract claims
            var googleUserId = principal.FindFirst("sub")?.Value;
            var email = principal.FindFirst("email")?.Value;
            var emailVerified = principal.FindFirst("email_verified")?.Value == "true";
            var firstName = principal.FindFirst("given_name")?.Value ?? "Google";
            var lastName = principal.FindFirst("family_name")?.Value ?? "User";

            if (string.IsNullOrEmpty(googleUserId) || string.IsNullOrEmpty(email))
                return null;

            return new GoogleTokenValidationResult
            {
                GoogleUserId = googleUserId,
                Email = email,
                EmailVerified = emailVerified,
                FirstName = firstName,
                LastName = lastName
            };
        }
        catch
        {
            return null;
        }
    }

    private async Task<ICollection<SecurityKey>?> GetGoogleKeysAsync(CancellationToken ct)
    {
        // Return cached keys if still valid
        if (_googleKeys != null && DateTime.UtcNow < _keysExpiry)
            return _googleKeys;

        try
        {
            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                GoogleJwksUrl,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever(_httpClient));

            var config = await configurationManager.GetConfigurationAsync(ct);
            _googleKeys = config.SigningKeys;
            _keysExpiry = DateTime.UtcNow.Add(JwksCacheDuration);
            return _googleKeys;
        }
        catch
        {
            return null;
        }
    }
}
