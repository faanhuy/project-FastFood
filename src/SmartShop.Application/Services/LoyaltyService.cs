using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Services;

public class LoyaltyService(
    ILoyaltyRepository loyaltyRepository,
    IUnitOfWork unitOfWork) : ILoyaltyService
{
    private const int PointsPerThousandVnd = 10;      // 10 points per 1000 VND
    private const decimal MaxRedeemPercent = 0.30m;   // Max 30% of order
    private const decimal PointValue = 10m;           // 1 point = 10 VND

    public async Task<LoyaltyAccount> GetOrCreateAccountAsync(Guid userId, CancellationToken ct)
    {
        var account = await loyaltyRepository.GetByUserIdAsync(userId, ct);
        if (account != null) return account;

        account = LoyaltyAccount.Create(userId);
        await loyaltyRepository.AddAsync(account, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return account;
    }

    public async Task EarnPointsAsync(Guid userId, Guid orderId, decimal orderAmount, CancellationToken ct)
    {
        var account = await GetOrCreateAccountAsync(userId, ct);
        var pointsToEarn = CalculatePointsEarned(orderAmount);

        if (pointsToEarn > 0)
        {
            account.EarnPoints(pointsToEarn, orderId, $"Order {orderId:N}");
            await unitOfWork.SaveChangesAsync(ct);
        }
    }

    public async Task<decimal> RedeemPointsAsync(Guid userId, Guid orderId, decimal pointsToRedeem, CancellationToken ct)
    {
        var account = await loyaltyRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException("LoyaltyAccount", userId);

        account.RedeemPoints(pointsToRedeem, orderId, $"Order {orderId:N}");
        await unitOfWork.SaveChangesAsync(ct);

        return pointsToRedeem * PointValue;  // Return VND value
    }

    public async Task<int> GetCurrentTierAsync(Guid userId, CancellationToken ct)
    {
        var account = await loyaltyRepository.GetByUserIdAsync(userId, ct);
        return account?.Tier ?? 0;
    }

    public decimal CalculatePointsEarned(decimal orderAmount)
        => (decimal)Math.Floor(orderAmount / 1000) * PointsPerThousandVnd;
}
