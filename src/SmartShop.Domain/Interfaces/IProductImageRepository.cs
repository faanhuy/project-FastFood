using SmartShop.Domain.Entities;

namespace SmartShop.Domain.Interfaces;

public interface IProductImageRepository
{
    Task<List<ProductImage>> GetByProductIdAsync(Guid productId, CancellationToken ct = default);
    Task<ProductImage?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ProductImage image, CancellationToken ct = default);
    void Remove(ProductImage image);
}
