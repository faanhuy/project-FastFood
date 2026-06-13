using SmartShop.Domain.Entities;

namespace SmartShop.Domain.Interfaces;

public interface IOrderFlashSaleUsageRepository
{
    Task AddAsync(OrderFlashSaleUsage usage, CancellationToken ct = default);
    Task<IReadOnlyList<OrderFlashSaleUsage>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task<IReadOnlyList<OrderFlashSaleUsage>> GetByFlashSaleIdAsync(Guid flashSaleId, CancellationToken ct = default);
}
