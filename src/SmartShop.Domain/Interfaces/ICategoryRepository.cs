using SmartShop.Domain.Entities;

namespace SmartShop.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllActiveAsync(CancellationToken ct = default);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task AddAsync(Category category, CancellationToken ct = default);
    void Update(Category category);
}
