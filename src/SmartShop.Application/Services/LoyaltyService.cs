using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Services;

public class LoyaltyService(
    ILoyaltyRepository loyaltyRepository,
    IAppSettingRepository appSettingRepository,
    IUnitOfWork unitOfWork) : ILoyaltyService
{
    private const decimal MaxRedeemPercent = 0.30m;
    private const decimal PointValue = 10m;            // 1 point = 10 VND
    private const decimal DefaultVndPerPoint = 1000m;  // fallback nếu chưa có config

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
        var pointsToEarn = await CalculatePointsEarnedAsync(orderAmount, ct);

        if (pointsToEarn <= 0) return;

        // Idempotent: skip nếu đã tích điểm cho order này rồi
        var alreadyEarned = await loyaltyRepository.GetEarnedPointsByOrderAsync(account.Id, orderId, ct);
        if (alreadyEarned > 0) return;

        var transaction = account.EarnPoints(pointsToEarn, orderId, $"Order {orderId:N}");
        await loyaltyRepository.AddTransactionAsync(transaction, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<decimal> RedeemPointsAsync(Guid userId, Guid orderId, decimal pointsToRedeem, CancellationToken ct)
    {
        var account = await loyaltyRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException("LoyaltyAccount", userId);

        var transaction = account.RedeemPoints(pointsToRedeem, orderId, $"Order {orderId:N}");
        await loyaltyRepository.AddTransactionAsync(transaction, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return pointsToRedeem * PointValue;
    }

    public async Task ReversePointsForOrderAsync(Guid userId, Guid orderId, CancellationToken ct)
    {
        var account = await loyaltyRepository.GetByUserIdAsync(userId, ct);
        if (account is null) return;

        var earnedPoints = await loyaltyRepository.GetEarnedPointsByOrderAsync(account.Id, orderId, ct);
        if (earnedPoints <= 0) return;

        // Chỉ reverse phần chưa được reverse (tránh double-reverse)
        var alreadyReversed = await loyaltyRepository.GetReversedPointsByOrderAsync(account.Id, orderId, ct);
        var netReversible = earnedPoints - alreadyReversed;
        if (netReversible <= 0) return;

        var transaction = account.ReversePoints(netReversible, orderId, $"Reversed for order {orderId:N}");
        await loyaltyRepository.AddTransactionAsync(transaction, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<int> GetCurrentTierAsync(Guid userId, CancellationToken ct)
    {
        var account = await loyaltyRepository.GetByUserIdAsync(userId, ct);
        return account?.Tier ?? 0;
    }

    public async Task<decimal> CalculatePointsEarnedAsync(decimal orderAmount, CancellationToken ct)
    {
        var vndPerPoint = (decimal)await appSettingRepository.GetDoubleAsync(
            "Loyalty:VndPerPoint", (double)DefaultVndPerPoint, ct);

        if (vndPerPoint <= 0) vndPerPoint = DefaultVndPerPoint;

        return Math.Floor(orderAmount / vndPerPoint);
    }
}
