using Microsoft.EntityFrameworkCore;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;
using SmartShop.Infrastructure.Data;

namespace SmartShop.Infrastructure.Repositories;

public class CartRepository(ApplicationDbContext context) : ICartRepository
{
    public async Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);
    }

    public async Task AddAsync(Cart cart, CancellationToken ct = default)
    {
        await context.Carts.AddAsync(cart, ct);
    }

    public async Task AddCartItemAsync(CartItem item, CancellationToken ct = default)
    {
        await context.CartItems.AddAsync(item, ct);
    }

    public void Update(Cart cart)
    {
        context.Carts.Update(cart);
    }
}
