using SmartShop.Domain.Entities;

namespace SmartShop.Domain.Interfaces;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Review?> GetByUserAndProductAsync(Guid userId, Guid productId, CancellationToken ct = default);
    Task<(IEnumerable<Review> Items, int TotalCount)> GetPagedByProductAsync(
        Guid productId, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(Review review, CancellationToken ct = default);
    void Delete(Review review);
}
