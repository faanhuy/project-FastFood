using SmartShop.Domain.Entities;

namespace SmartShop.Domain.Interfaces;

public interface IOrderStatusHistoryRepository
{
    Task<List<OrderStatusHistory>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task AddAsync(OrderStatusHistory history, CancellationToken ct = default);
}
