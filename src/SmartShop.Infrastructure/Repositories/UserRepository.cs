using Microsoft.EntityFrameworkCore;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;
using SmartShop.Infrastructure.Data;

namespace SmartShop.Infrastructure.Repositories;

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await context.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public async Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken ct = default)
        => await context.Users
            .FirstOrDefaultAsync(u => u.GoogleId == googleId, ct);

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
        => await context.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, ct);

    public async Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken ct = default)
        => await context.Users
            .FirstOrDefaultAsync(u => u.RefreshTokenHash == tokenHash, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await context.Users.AddAsync(user, ct);

    public async Task<bool> ExistsAsync(string email, CancellationToken ct = default)
        => await context.Users.AnyAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public async Task<List<User>> GetByIdsAsync(List<Guid> ids, CancellationToken ct = default)
        => await context.Users.Where(u => ids.Contains(u.Id)).ToListAsync(ct);

    public async Task<PagedResult<User>> GetPagedAsync(
        int page,
        int pageSize,
        string? roleFilter = null,
        bool? bannedFilter = null,
        string? searchEmail = null,
        string sortBy = "createdAt",
        string sortDirection = "desc",
        CancellationToken ct = default)
    {
        var query = context.Users.AsQueryable();

        if (!string.IsNullOrEmpty(roleFilter))
            query = query.Where(u => u.Role == roleFilter);

        if (bannedFilter.HasValue)
            query = query.Where(u => u.IsBanned == bannedFilter.Value);

        if (!string.IsNullOrEmpty(searchEmail))
            query = query.Where(u => u.Email.Contains(searchEmail));

        // Apply sorting
        var sorted = (sortBy, sortDirection) switch
        {
            ("email", "asc") => query.OrderBy(u => u.Email),
            ("email", _) => query.OrderByDescending(u => u.Email),
            ("createdAt", "asc") => query.OrderBy(u => u.CreatedAt),
            (_, _) => query.OrderByDescending(u => u.CreatedAt),
        };

        var total = await query.CountAsync(ct);
        var items = await sorted
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<User>(items, total, page, pageSize);
    }
}
