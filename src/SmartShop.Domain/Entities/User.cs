using SmartShop.Domain.Common;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Domain.Entities;

public class User : BaseAuditableEntity
{
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Role { get; private set; } = "Customer";
    public string? RefreshToken { get; private set; }
    public string? RefreshTokenHash { get; private set; }
    public DateTime? RefreshTokenExpiry { get; private set; }
    public string? AvatarUrl { get; private set; }
    public bool IsBanned { get; private set; } = false;
    public DateTime? BannedAt { get; private set; }
    public string? GoogleId { get; private set; }

    private User() { }

    public static User Create(string email, string passwordHash, string firstName, string lastName)
    {
        return new User
        {
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName
        };
    }

    public static User CreateFromGoogle(string googleId, string email, string firstName, string lastName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(googleId);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

        return new User
        {
            GoogleId = googleId,
            Email = email.ToLowerInvariant(),
            PasswordHash = "GOOGLE_OAUTH",
            FirstName = firstName,
            LastName = lastName,
            Role = "Customer"
        };
    }

    public void SetRefreshToken(string refreshToken, DateTime expiry, ITokenHasher hasher)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);
        RefreshToken = null;
        RefreshTokenHash = hasher.Hash(refreshToken);
        RefreshTokenExpiry = expiry;
    }

    public bool VerifyRefreshToken(string token, ITokenHasher hasher)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        // Try hash verification first
        if (!string.IsNullOrEmpty(RefreshTokenHash))
        {
            return hasher.Verify(token, RefreshTokenHash);
        }

        // Fallback to plaintext comparison (backward compatibility)
        return token == RefreshToken;
    }

    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenHash = null;
        RefreshTokenExpiry = null;
    }

    public void PromoteToAdmin()
    {
        Role = "Admin";
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public void SetAvatarUrl(string? avatarUrl)
    {
        AvatarUrl = avatarUrl;
    }

    public void Ban()
    {
        IsBanned = true;
        BannedAt = DateTime.UtcNow;
    }

    public void Unban()
    {
        IsBanned = false;
        BannedAt = null;
    }

    public void UpdateRole(string role)
    {
        Role = role;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newPasswordHash);
        PasswordHash = newPasswordHash;
        // Revoke all refresh tokens on password reset
        RevokeRefreshToken();
    }

    public void SetGoogleId(string googleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(googleId);
        GoogleId = googleId;
    }

}
