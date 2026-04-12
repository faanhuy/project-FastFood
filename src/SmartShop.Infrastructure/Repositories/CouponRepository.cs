
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;
using SmartShop.Infrastructure.Data;

namespace SmartShop.Infrastructure.Repositories;

public class CouponRepository : ICouponRepository
{
    private readonly ApplicationDbContext _context;

    public CouponRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Coupon coupon, CancellationToken ct = default)
    {
        await _context.Coupons.AddAsync(coupon, ct);
    }

    public void DeleteAsync(Coupon coupon, CancellationToken ct = default)
    {
        _context.Coupons.Remove(coupon);
    }

    public async Task<IEnumerable<Coupon>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Coupons.ToListAsync(ct);
    }

    public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.Coupons.FirstOrDefaultAsync(c => c.Code == code, ct);
    }

    public async Task<bool> HasUsageByUserAsync(Guid couponId, Guid userId, CancellationToken ct = default)
    {
        return await _context.CouponUsages
     .AnyAsync(u => u.CouponId == couponId && u.UserId == userId, ct);
    }
    public async Task<bool> HasAnyUsageAsync(Guid couponId, CancellationToken ct = default)
    {
        return await _context.CouponUsages.AnyAsync(u => u.CouponId == couponId, ct);
    }

    public void Update(Coupon coupon)
    {
        _context.Coupons.Update(coupon);
    }
}
