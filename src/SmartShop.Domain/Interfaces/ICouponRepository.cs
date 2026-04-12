using SmartShop.Domain.Entities;

namespace SmartShop.Domain.Interfaces;

public interface ICouponRepository
{
    Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task AddAsync(Coupon coupon, CancellationToken ct = default);
    Task<IEnumerable<Coupon>> GetAllAsync(CancellationToken ct = default);
    void DeleteAsync(Coupon coupon, CancellationToken ct = default);
    void Update(Coupon coupon);
    Task<bool> HasUsageByUserAsync(Guid couponId, Guid userId, CancellationToken ct = default);
    Task<bool> HasAnyUsageAsync(Guid couponId, CancellationToken ct = default);
}
