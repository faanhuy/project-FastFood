using SmartShop.Domain.Entities;

namespace SmartShop.Domain.Interfaces;

public interface IFlashSaleRepository
{
    Task<FlashSale?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<FlashSale>> GetActiveFlashSalesAsync(DateTime now, CancellationToken ct = default);
    Task<FlashSale?> GetActiveByProductIdAsync(Guid productId, DateTime now, CancellationToken ct = default);
    Task<List<FlashSale>> GetExpiredFlashSalesAsync(DateTime now, CancellationToken ct = default);
    Task<(IEnumerable<FlashSale> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, bool? isActive = null, string? status = null, CancellationToken ct = default);
    Task AddAsync(FlashSale flashSale, CancellationToken ct = default);
    void Update(FlashSale flashSale);
    void Remove(FlashSale flashSale);
    Task AddItemAsync(FlashSaleItem item, CancellationToken ct = default);
    Task RemoveItemsByFlashSaleIdAsync(Guid flashSaleId, CancellationToken ct = default);
}
