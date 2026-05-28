using SmartShop.Domain.Entities;

namespace SmartShop.Domain.Interfaces;

public interface IWishlistRepository
{
    Task<IEnumerable<WishlistItem>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<WishlistItem?> GetByUserAndProductAsync(Guid userId, Guid productId, CancellationToken ct = default);
    Task AddAsync(WishlistItem item, CancellationToken ct = default);
    void RemoveAsync(WishlistItem item);
    Task<bool> ExistsAsync(Guid userId, Guid productId, CancellationToken ct = default);
}
