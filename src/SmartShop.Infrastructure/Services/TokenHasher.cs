using System.Security.Cryptography;
using System.Text;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Infrastructure.Services;

public class TokenHasher : ITokenHasher
{
    public string Hash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }

    public bool Verify(string token, string hash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        ArgumentException.ThrowIfNullOrWhiteSpace(hash);
        var tokenHash = Hash(token);
        return tokenHash.Equals(hash, StringComparison.OrdinalIgnoreCase);
    }
}
