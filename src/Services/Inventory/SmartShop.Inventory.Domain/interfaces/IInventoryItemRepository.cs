using SmartShop.Inventory.Domain.Entities;

namespace SmartShop.Inventory.Domain.Interfaces;
public interface IInventoryItemRepository
{
    Task<InventoryItem?> GetByKeyAsync(Guid storeId, Guid productId, Guid? sizeId, CancellationToken ct = default);
    Task<List<InventoryItem>> GetByKeysAsync(IReadOnlyCollection<(Guid StoreId, Guid ProductId, Guid? SizeId)> keys, CancellationToken ct = default);
    Task AddAsync(InventoryItem item, CancellationToken ct = default);
}