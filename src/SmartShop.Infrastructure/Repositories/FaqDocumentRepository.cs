using Microsoft.EntityFrameworkCore;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;
using SmartShop.Infrastructure.Data;

namespace SmartShop.Infrastructure.Repositories;

public class FaqDocumentRepository(ApplicationDbContext db) : IFaqDocumentRepository
{
    public async Task<IReadOnlyList<FaqDocument>> GetAllActiveAsync(CancellationToken ct = default)
    {
        return await db.FaqDocuments
            .AsNoTracking()
            .Where(f => f.IsActive)
            .OrderBy(f => f.Category)
            .ThenBy(f => f.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<FaqDocument>> GetByCategoryAsync(string category, CancellationToken ct = default)
    {
        return await db.FaqDocuments
            .AsNoTracking()
            .Where(f => f.IsActive && f.Category == category.ToLowerInvariant())
            .OrderBy(f => f.CreatedAt)
            .ToListAsync(ct);
    }
}
