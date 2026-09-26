using Microsoft.EntityFrameworkCore;
using SmartShop.Inventory.Domain.Entities;
using SmartShop.Inventory.Domain.Interfaces;
using SmartShop.Inventory.Infrastructure.Persistence;

namespace SmartShop.Inventory.Infrastructure.Repositories;

public class InventoryItemRepository(InventoryDbContext context) : IInventoryItemRepository
{
    // i.SizeId == sizeId với Guid? — EF tự sinh "size_id IS NULL" khi sizeId là null
    public Task<InventoryItem?> GetByKeyAsync(Guid storeId, Guid productId, Guid? sizeId, CancellationToken ct = default)
        => context.InventoryItems.FirstOrDefaultAsync(
            i => i.StoreId == storeId && i.ProductId == productId && i.SizeId == sizeId, ct);

    public async Task<List<InventoryItem>> GetByKeysAsync(
        IReadOnlyCollection<(Guid StoreId, Guid ProductId, Guid? SizeId)> keys, CancellationToken ct = default)
    {
        var storeIds = keys.Select(k => k.StoreId).Distinct().ToList();
        var productIds = keys.Select(k => k.ProductId).Distinct().ToList();
        var wanted = keys.ToHashSet();

        // EF không dịch được so sánh tuple sang SQL → lọc thô theo store/product rồi lọc lại đúng cặp khoá trong bộ nhớ
        var candidates = await context.InventoryItems
            .Where(i => storeIds.Contains(i.StoreId) && productIds.Contains(i.ProductId))
            .ToListAsync(ct);

        return candidates
            .Where(i => wanted.Contains((i.StoreId, i.ProductId, i.SizeId)))
            .ToList();
    }

    public async Task AddAsync(InventoryItem item, CancellationToken ct = default)
        => await context.InventoryItems.AddAsync(item, ct);
}
