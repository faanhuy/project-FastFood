using SmartShop.Domain.Entities;

namespace SmartShop.Domain.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Cart cart, CancellationToken ct = default);
    Task AddCartItemAsync(CartItem item, CancellationToken ct = default);
    void Update(Cart cart);
}
