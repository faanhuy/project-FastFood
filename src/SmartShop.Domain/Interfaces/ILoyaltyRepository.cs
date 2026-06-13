using SmartShop.Domain.Entities;

namespace SmartShop.Domain.Interfaces;

public interface ILoyaltyRepository
{
    // Account operations
    Task<LoyaltyAccount?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<LoyaltyAccount?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(LoyaltyAccount account, CancellationToken ct = default);

    // Transaction operations
    Task<PointTransaction?> GetTransactionByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IEnumerable<PointTransaction> Items, int TotalCount)> GetTransactionsByAccountIdAsync(
        Guid accountId, int page, int pageSize, CancellationToken ct = default);
    Task AddTransactionAsync(PointTransaction transaction, CancellationToken ct = default);
    Task<decimal> GetEarnedPointsByOrderAsync(Guid accountId, Guid orderId, CancellationToken ct = default);
    Task<decimal> GetReversedPointsByOrderAsync(Guid accountId, Guid orderId, CancellationToken ct = default);

    // Analytics
    Task<(int TotalCustomersWithLoyalty, int PlatinumCustomers)> GetLoyaltyStatsAsync(CancellationToken ct = default);
}
