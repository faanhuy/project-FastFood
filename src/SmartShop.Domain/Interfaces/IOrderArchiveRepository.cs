using SmartShop.Domain.Entities;

namespace SmartShop.Domain.Interfaces;

public interface IOrderArchiveRepository
{
    Task AddAsync(OrderArchive archive, CancellationToken ct = default);
    Task<List<Order>> GetOrdersToArchiveAsync(DateTime cutoffDate, CancellationToken ct = default);
}
