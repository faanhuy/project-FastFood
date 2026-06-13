using SmartShop.Domain.Entities;

namespace SmartShop.Domain.Interfaces;

public interface ICouponRepository
{
    Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<Coupon?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Coupon coupon, CancellationToken ct = default);
    Task<IEnumerable<Coupon>> GetAllAsync(CancellationToken ct = default);
    Task<(IEnumerable<Coupon> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search = null, bool? isExpired = null,
        CancellationToken ct = default);
    Task<List<Coupon>> GetByIdsAsync(List<Guid> ids, CancellationToken ct = default);
    void DeleteAsync(Coupon coupon, CancellationToken ct = default);
    void Update(Coupon coupon);
    Task<bool> HasUsageByUserAsync(Guid couponId, Guid userId, CancellationToken ct = default);
    Task<bool> HasAnyUsageAsync(Guid couponId, CancellationToken ct = default);
}
