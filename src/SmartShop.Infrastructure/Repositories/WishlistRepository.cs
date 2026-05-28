using Microsoft.EntityFrameworkCore;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;
using SmartShop.Infrastructure.Data;

namespace SmartShop.Infrastructure.Repositories;

public class WishlistRepository(ApplicationDbContext context) : IWishlistRepository
{
    public async Task<IEnumerable<WishlistItem>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await context.WishlistItems
            .AsNoTracking()
            .Include(w => w.Product)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<WishlistItem?> GetByUserAndProductAsync(Guid userId, Guid productId, CancellationToken ct = default)
    {
        return await context.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId, ct);
    }

    public async Task AddAsync(WishlistItem item, CancellationToken ct = default)
    {
        await context.WishlistItems.AddAsync(item, ct);
    }

    public void RemoveAsync(WishlistItem item)
    {
        context.WishlistItems.Remove(item);
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid productId, CancellationToken ct = default)
    {
        return await context.WishlistItems
            .AnyAsync(w => w.UserId == userId && w.ProductId == productId, ct);
    }
}
