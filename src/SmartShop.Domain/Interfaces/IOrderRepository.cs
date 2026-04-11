using SmartShop.Domain.Entities;
using SmartShop.Domain.Enums;

namespace SmartShop.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default);
    Task<(IEnumerable<Order> Items, int TotalCount)> GetPagedByUserIdAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<(IEnumerable<Order> Items, int TotalCount)> GetAllPagedAsync(
        int page, int pageSize, OrderStatus? statusFilter = null, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
}
