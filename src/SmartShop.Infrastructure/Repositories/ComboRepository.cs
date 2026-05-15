using Microsoft.EntityFrameworkCore;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;
using SmartShop.Infrastructure.Data;

namespace SmartShop.Infrastructure.Repositories;

public class ComboRepository(ApplicationDbContext context) : IComboRepository
{
    public async Task<Combo?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Combos
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<List<Combo>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        return await context.Combos
            .AsNoTracking()
            .Include(c => c.Items)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<List<Combo>> GetActiveAsync(DateTime now, CancellationToken ct = default)
    {
        return await context.Combos
            .AsNoTracking()
            .Include(c => c.Items)
            .Where(c => c.IsActive && c.StartsAt <= now && (c.EndsAt == null || c.EndsAt > now))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Combo combo, CancellationToken ct = default)
    {
        await context.Combos.AddAsync(combo, ct);
    }

    public void Update(Combo combo)
    {
        context.Combos.Update(combo);
    }

    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        return await context.Combos.CountAsync(ct);
    }
}
