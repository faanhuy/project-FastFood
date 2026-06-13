using Microsoft.EntityFrameworkCore;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;
using SmartShop.Infrastructure.Data;

namespace SmartShop.Infrastructure.Repositories;

public class OrderArchiveRepository(ApplicationDbContext context) : IOrderArchiveRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task AddAsync(OrderArchive archive, CancellationToken ct = default)
    {
        await _context.OrderArchives.AddAsync(archive, ct);
    }

    public async Task<List<Order>> GetOrdersToArchiveAsync(DateTime cutoffDate, CancellationToken ct = default)
    {
        return await _context.Orders
            .Where(o => !o.IsArchived
                && o.CreatedAt < cutoffDate
                && (o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Cancelled))
            .Take(500)
            .ToListAsync(ct);
    }
}
