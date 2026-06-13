using SmartShop.Domain.Entities;

namespace SmartShop.Application.Common.Interfaces;

public interface ILoyaltyService
{
    Task<LoyaltyAccount> GetOrCreateAccountAsync(Guid userId, CancellationToken ct);
    Task EarnPointsAsync(Guid userId, Guid orderId, decimal orderAmount, CancellationToken ct);
    Task<decimal> RedeemPointsAsync(Guid userId, Guid orderId, decimal pointsToRedeem, CancellationToken ct);
    Task ReversePointsForOrderAsync(Guid userId, Guid orderId, CancellationToken ct);
    Task<int> GetCurrentTierAsync(Guid userId, CancellationToken ct);
    Task<decimal> CalculatePointsEarnedAsync(decimal orderAmount, CancellationToken ct);
}
