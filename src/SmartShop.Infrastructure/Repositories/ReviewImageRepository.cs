using Microsoft.EntityFrameworkCore;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;
using SmartShop.Infrastructure.Data;

namespace SmartShop.Infrastructure.Repositories;

public class ReviewImageRepository(ApplicationDbContext context) : IReviewImageRepository
{
    public async Task<List<ReviewImage>> GetByReviewIdAsync(Guid reviewId, CancellationToken ct = default)
        => await context.ReviewImages
            .AsNoTracking()
            .Where(ri => ri.ReviewId == reviewId)
            .OrderBy(ri => ri.DisplayOrder)
            .ToListAsync(ct);

    public async Task<int> CountByReviewIdAsync(Guid reviewId, CancellationToken ct = default)
        => await context.ReviewImages
            .CountAsync(ri => ri.ReviewId == reviewId, ct);

    public async Task AddRangeAsync(IEnumerable<ReviewImage> images, CancellationToken ct = default)
        => await context.ReviewImages.AddRangeAsync(images, ct);

    public void DeleteRange(IEnumerable<ReviewImage> images)
        => context.ReviewImages.RemoveRange(images);
}
