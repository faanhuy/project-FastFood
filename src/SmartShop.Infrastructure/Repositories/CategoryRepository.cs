using Microsoft.EntityFrameworkCore;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;
using SmartShop.Infrastructure.Data;

namespace SmartShop.Infrastructure.Repositories;

public class CategoryRepository(ApplicationDbContext context) : ICategoryRepository
{
    public async Task<IEnumerable<Category>> GetAllActiveAsync(CancellationToken ct = default)
    {
        return await context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await context.Categories.FirstOrDefaultAsync(c => c.Slug == slug, ct);
    }

    public async Task AddAsync(Category category, CancellationToken ct = default)
    {
        await context.Categories.AddAsync(category, ct);
    }

    public void Update(Category category)
    {
        context.Categories.Update(category);
    }
}
