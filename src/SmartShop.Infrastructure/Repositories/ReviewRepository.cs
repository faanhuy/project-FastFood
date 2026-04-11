using Microsoft.EntityFrameworkCore;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;
using SmartShop.Infrastructure.Data;

namespace SmartShop.Infrastructure.Repositories;

public class ReviewRepository(ApplicationDbContext context) : IReviewRepository
{
    public async Task<Review?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.Reviews
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<Review?> GetByUserAndProductAsync(Guid userId, Guid productId, CancellationToken ct = default)
        => await context.Reviews
            .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == productId, ct);

    public async Task<(IEnumerable<Review> Items, int TotalCount)> GetPagedByProductAsync(
        Guid productId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = context.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductId == productId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(Review review, CancellationToken ct = default)
        => await context.Reviews.AddAsync(review, ct);

    public void Delete(Review review)
        => context.Reviews.Remove(review);
}
