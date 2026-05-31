using Microsoft.EntityFrameworkCore;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;
using SmartShop.Infrastructure.Data;

namespace SmartShop.Infrastructure.Repositories;

public class ProductImageRepository(ApplicationDbContext context) : IProductImageRepository
{
    public async Task<List<ProductImage>> GetByProductIdAsync(Guid productId, CancellationToken ct = default)
    {
        return await context.ProductImages
            .Where(p => p.ProductId == productId)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(ct);
    }

    public async Task<ProductImage?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.ProductImages.FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task AddAsync(ProductImage image, CancellationToken ct = default)
    {
        await context.ProductImages.AddAsync(image, ct);
    }

    public void Remove(ProductImage image)
    {
        context.ProductImages.Remove(image);
    }
}
