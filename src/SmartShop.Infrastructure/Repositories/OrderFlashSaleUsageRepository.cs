using Microsoft.EntityFrameworkCore;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;
using SmartShop.Infrastructure.Data;

namespace SmartShop.Infrastructure.Repositories;

public class OrderFlashSaleUsageRepository(ApplicationDbContext context) : IOrderFlashSaleUsageRepository
{
    public async Task AddAsync(OrderFlashSaleUsage usage, CancellationToken ct = default)
        => await context.OrderFlashSaleUsages.AddAsync(usage, ct);

    public async Task<IReadOnlyList<OrderFlashSaleUsage>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        => await context.OrderFlashSaleUsages
            .Where(x => x.OrderId == orderId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<OrderFlashSaleUsage>> GetByFlashSaleIdAsync(Guid flashSaleId, CancellationToken ct = default)
        => await context.OrderFlashSaleUsages
            .Where(x => x.FlashSaleId == flashSaleId)
            .ToListAsync(ct);
}
