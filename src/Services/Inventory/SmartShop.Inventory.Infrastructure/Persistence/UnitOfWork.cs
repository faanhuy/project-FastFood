using Microsoft.EntityFrameworkCore;
using SmartShop.Inventory.Application.Common.Interfaces;
using SmartShop.Inventory.Domain.Common.Exceptions;

namespace SmartShop.Inventory.Infrastructure.Persistence;

public class UnitOfWork(InventoryDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Khác Core: phải Clear tracker để ConcurrencyRetry (Application) tải lại dữ liệu mới ở lần thử 2
            context.ChangeTracker.Clear();
            throw new ConcurrencyException("Stock was modified by another request.");
        }
    }
}
