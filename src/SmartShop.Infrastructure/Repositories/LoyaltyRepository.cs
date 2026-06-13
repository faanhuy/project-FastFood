using Microsoft.EntityFrameworkCore;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;
using SmartShop.Infrastructure.Data;

namespace SmartShop.Infrastructure.Repositories;

public class LoyaltyRepository(ApplicationDbContext context) : ILoyaltyRepository
{
    public async Task<LoyaltyAccount?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.LoyaltyAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<LoyaltyAccount?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await context.LoyaltyAccounts
            .FirstOrDefaultAsync(a => a.UserId == userId, ct);
    }

    public async Task AddAsync(LoyaltyAccount account, CancellationToken ct = default)
    {
        await context.LoyaltyAccounts.AddAsync(account, ct);
    }

    public async Task<PointTransaction?> GetTransactionByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.PointTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<(IEnumerable<PointTransaction> Items, int TotalCount)> GetTransactionsByAccountIdAsync(
        Guid accountId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = context.PointTransactions
            .AsNoTracking()
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.CreatedAt);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddTransactionAsync(PointTransaction transaction, CancellationToken ct = default)
    {
        await context.PointTransactions.AddAsync(transaction, ct);
    }

    public async Task<decimal> GetEarnedPointsByOrderAsync(Guid accountId, Guid orderId, CancellationToken ct = default)
    {
        return await context.PointTransactions
            .Where(t => t.AccountId == accountId
                     && t.OrderId == orderId
                     && t.Type == PointTransactionType.Earn)
            .SumAsync(t => t.Points, ct);
    }

    public async Task<decimal> GetReversedPointsByOrderAsync(Guid accountId, Guid orderId, CancellationToken ct = default)
    {
        return await context.PointTransactions
            .Where(t => t.AccountId == accountId
                     && t.OrderId == orderId
                     && t.Type == PointTransactionType.Reverse)
            .SumAsync(t => t.Points, ct);
    }

    public async Task<(int TotalCustomersWithLoyalty, int PlatinumCustomers)> GetLoyaltyStatsAsync(
        CancellationToken ct = default)
    {
        var totalCustomers = await context.LoyaltyAccounts.CountAsync(ct);
        var platinumCustomers = await context.LoyaltyAccounts
            .CountAsync(a => a.Tier == 3, ct);

        return (totalCustomers, platinumCustomers);
    }
}
