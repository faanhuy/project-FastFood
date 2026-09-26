using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartShop.Inventory.Domain.Entities;
using SmartShop.Inventory.Infrastructure.Persistence;

namespace SmartShop.Inventory.Infrastructure.Seeding;

/// <summary>
/// Nạp tồn kho ban đầu từ Seeding/inventory-seed.json (xuất từ Core: StoreInventories + StoreSizeInventories).
/// ProductId/StoreId/SizeId phải trùng Core vì hai DB không có FK chéo.
/// </summary>
public class InventorySeeder(InventoryDbContext db, ILogger<InventorySeeder> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private sealed record SeedRow(
        Guid ProductId, Guid StoreId, Guid? SizeId, string Sku, int Quantity, int LowStockThreshold);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        // Idempotent: đã có dữ liệu thì thôi — chạy lại không tạo trùng
        if (await db.InventoryItems.AnyAsync(ct))
        {
            logger.LogInformation("Inventory already seeded, skipping.");
            return;
        }

        var path = Path.Combine(AppContext.BaseDirectory, "Seeding", "inventory-seed.json");
        if (!File.Exists(path))
        {
            logger.LogWarning("Inventory seed file not found at {Path}, skipping.", path);
            return;
        }

        var rows = JsonSerializer.Deserialize<List<SeedRow>>(
            await File.ReadAllTextAsync(path, ct), JsonOptions) ?? [];

        if (rows.Count == 0)
        {
            logger.LogWarning("Inventory seed file {Path} is empty, skipping.", path);
            return;
        }

        var items = rows.Select(r => InventoryItem.Create(
            r.ProductId, r.StoreId, r.SizeId, r.Sku, r.Quantity, r.LowStockThreshold));

        await db.InventoryItems.AddRangeAsync(items, ct);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Seeded {Count} inventory items.", rows.Count);
    }
}
