using SmartShop.Domain.Entities;

namespace SmartShop.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken ct = default);
    Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task<List<User>> GetByIdsAsync(List<Guid> ids, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task<bool> ExistsAsync(string email, CancellationToken ct = default);
    Task<PagedResult<User>> GetPagedAsync(
        int page,
        int pageSize,
        string? roleFilter = null,
        bool? bannedFilter = null,
        string? searchEmail = null,
        string sortBy = "createdAt",
        string sortDirection = "desc",
        CancellationToken ct = default);
}

public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
};
