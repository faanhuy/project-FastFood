using Microsoft.EntityFrameworkCore;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;
using SmartShop.Infrastructure.Data;

namespace SmartShop.Infrastructure.Repositories;

public class FlashSaleRepository : IFlashSaleRepository
{
    private readonly ApplicationDbContext _context;

    public FlashSaleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FlashSale?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.FlashSales
            .Include(fs => fs.Items)
            .FirstOrDefaultAsync(fs => fs.Id == id, ct);
    }

    public async Task<List<FlashSale>> GetActiveFlashSalesAsync(DateTime now, CancellationToken ct = default)
    {
        return await _context.FlashSales
            .AsNoTracking()
            .Include(fs => fs.Items)
            .Where(fs => fs.IsActive && fs.StartAt <= now && fs.EndAt > now)
            .OrderByDescending(fs => fs.StartAt)
            .ToListAsync(ct);
    }

    public async Task<FlashSale?> GetActiveByProductIdAsync(Guid productId, DateTime now, CancellationToken ct = default)
    {
        return await _context.FlashSales
            .Include(fs => fs.Items)
            .FirstOrDefaultAsync(
                fs => fs.Items.Any(i => i.ProductId == productId) &&
                      fs.IsActive &&
                      fs.StartAt <= now &&
                      fs.EndAt > now,
                ct);
    }

    public async Task<List<FlashSale>> GetExpiredFlashSalesAsync(DateTime now, CancellationToken ct = default)
    {
        return await _context.FlashSales
            .AsNoTracking()
            .Include(fs => fs.Items)
            .Where(fs => fs.IsActive && fs.EndAt <= now)
            .ToListAsync(ct);
    }

    public async Task<(IEnumerable<FlashSale> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, bool? isActive = null, string? status = null, CancellationToken ct = default)
    {
        var query = _context.FlashSales
            .AsNoTracking()
            .Include(fs => fs.Items)
            .AsQueryable();

        if (isActive.HasValue)
            query = query.Where(fs => fs.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<FlashSaleStatus>(status, out var parsedStatus))
            query = query.Where(fs => fs.Status == parsedStatus);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(fs => fs.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(FlashSale flashSale, CancellationToken ct = default)
    {
        await _context.FlashSales.AddAsync(flashSale, ct);
    }

    public void Update(FlashSale flashSale)
    {
        _context.FlashSales.Update(flashSale);
    }

    public void Remove(FlashSale flashSale)
    {
        _context.FlashSales.Remove(flashSale);
    }

    public async Task AddItemAsync(FlashSaleItem item, CancellationToken ct = default)
    {
        await _context.FlashSaleItems.AddAsync(item, ct);
    }

    public async Task RemoveItemsByFlashSaleIdAsync(Guid flashSaleId, CancellationToken ct = default)
    {
        var items = await _context.FlashSaleItems
            .Where(i => i.FlashSaleId == flashSaleId)
            .ToListAsync(ct);

        _context.FlashSaleItems.RemoveRange(items);
    }
}
