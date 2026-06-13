using Microsoft.EntityFrameworkCore;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;
using SmartShop.Infrastructure.Data;

namespace SmartShop.Infrastructure.Repositories;

public class OrderStatusHistoryRepository(ApplicationDbContext context) : IOrderStatusHistoryRepository
{
    public async Task<List<OrderStatusHistory>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        return await context.OrderStatusHistories
            .AsNoTracking()
            .Where(h => h.OrderId == orderId)
            .OrderBy(h => h.ChangedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(OrderStatusHistory history, CancellationToken ct = default)
    {
        await context.OrderStatusHistories.AddAsync(history, ct);
    }
}
