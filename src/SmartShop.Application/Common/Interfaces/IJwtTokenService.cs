using SmartShop.Domain.Entities;

namespace SmartShop.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
}
