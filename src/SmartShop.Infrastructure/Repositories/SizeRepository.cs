using Microsoft.EntityFrameworkCore;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;
using SmartShop.Infrastructure.Data;

namespace SmartShop.Infrastructure.Repositories;

public class SizeRepository(ApplicationDbContext db) : ISizeRepository
{
    public async Task<List<Size>> GetByCategoryAsync(SizeType category, CancellationToken ct = default)
    {
        return await db.Sizes
            .AsNoTracking()
            .Where(s => s.Category == category && s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync(ct);
    }

    public async Task<List<Size>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Sizes
            .AsNoTracking()
            .OrderBy(s => s.Category)
            .ThenBy(s => s.DisplayOrder)
            .ToListAsync(ct);
    }

    public async Task<Size?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Sizes
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<bool> ExistsByLabelAndCategoryAsync(string label, SizeType category, CancellationToken ct = default)
    {
        return await db.Sizes
            .AnyAsync(s => s.Label == label && s.Category == category, ct);
    }

    public async Task<bool> IsReferencedAsync(Guid id, CancellationToken ct = default)
    {
        return await db.ProductSizes.AnyAsync(ps => ps.SizeId == id, ct)
            || await db.StoreSizeInventories.AnyAsync(si => si.SizeId == id, ct)
            || await db.StockReceiptItems.AnyAsync(sri => sri.SizeId == id, ct);
    }

    public async Task AddAsync(Size size, CancellationToken ct = default)
    {
        await db.Sizes.AddAsync(size, ct);
    }

    public void Update(Size size)
    {
        db.Sizes.Update(size);
    }

    public void Remove(Size size)
    {
        db.Sizes.Remove(size);
    }
}
