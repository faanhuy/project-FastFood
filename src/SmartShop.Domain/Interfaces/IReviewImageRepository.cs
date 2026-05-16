using SmartShop.Domain.Entities;

namespace SmartShop.Domain.Interfaces;

public interface IReviewImageRepository
{
    Task<List<ReviewImage>> GetByReviewIdAsync(Guid reviewId, CancellationToken ct = default);
    Task<int> CountByReviewIdAsync(Guid reviewId, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<ReviewImage> images, CancellationToken ct = default);
    void DeleteRange(IEnumerable<ReviewImage> images);
}
