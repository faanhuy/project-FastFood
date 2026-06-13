using Microsoft.EntityFrameworkCore;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;
using SmartShop.Infrastructure.Data;

namespace SmartShop.Infrastructure.Repositories;

public class OrderRepository(ApplicationDbContext context) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.Items)
                .ThenInclude(i => i.Components)
            .Include(o => o.ShippingWard)
            .Include(o => o.ShippingProvince)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<(IEnumerable<Order> Items, int TotalCount)> GetPagedByUserIdAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.Items)
                .ThenInclude(i => i.Components)
            .Include(o => o.ShippingWard)
            .Include(o => o.ShippingProvince)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<(IEnumerable<Order> Items, int TotalCount)> GetAllPagedAsync(
        int page, int pageSize, OrderStatus? statusFilter = null, CancellationToken ct = default)
    {
        var query = context.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.Items)
                .ThenInclude(i => i.Components)
            .Include(o => o.ShippingWard)
            .Include(o => o.ShippingProvince)
            .AsQueryable();

        if (statusFilter.HasValue)
            query = query.Where(o => o.Status == statusFilter.Value);

        query = query.OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<(IEnumerable<Order> Items, int TotalCount)> GetAllPagedAsync(
        int page, int pageSize, OrderStatus? statusFilter,
        string? search, DateTime? createdAfter, DateTime? createdBefore,
        string sortBy, string sortDirection,
        CancellationToken ct = default)
    {
        var query = context.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.Items)
                .ThenInclude(i => i.Components)
            .Include(o => o.ShippingWard)
            .Include(o => o.ShippingProvince)
            .AsQueryable();

        if (statusFilter.HasValue)
            query = query.Where(o => o.Status == statusFilter.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search}%";
            query = query.Where(o =>
                EF.Functions.Like(EF.Functions.Collate(o.Id.ToString(), "Latin1_General_CI_AI"), pattern) ||
                EF.Functions.Like(EF.Functions.Collate(o.User!.Email, "Latin1_General_CI_AI"), pattern));
        }

        if (createdAfter.HasValue)
            query = query.Where(o => o.CreatedAt >= createdAfter.Value);

        if (createdBefore.HasValue)
            query = query.Where(o => o.CreatedAt <= createdBefore.Value);

        // Apply sorting
        query = (sortBy, sortDirection) switch
        {
            ("totalAmount", "asc") => query.OrderBy(o => o.TotalAmount),
            ("totalAmount", _) => query.OrderByDescending(o => o.TotalAmount),
            ("createdAt", "asc") => query.OrderBy(o => o.CreatedAt),
            (_, _) => query.OrderByDescending(o => o.CreatedAt),
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<List<Order>> GetByIdsAsync(List<Guid> ids, CancellationToken ct = default)
    {
        return await context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.Items)
                .ThenInclude(i => i.Components)
            .Include(o => o.ShippingWard)
            .Include(o => o.ShippingProvince)
            .Where(o => ids.Contains(o.Id))
            .ToListAsync(ct);
    }

    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        await context.Orders.AddAsync(order, ct);
    }

    public async Task<(decimal TotalRevenue, int TotalOrders, decimal AverageOrderValue)> GetRevenueSummaryAsync(
        DateTime from, DateTime to, CancellationToken ct = default)
    {
        var query = context.Orders
            .Where(o => o.Status != OrderStatus.Cancelled
                        && o.CreatedAt.Date >= from.Date
                        && o.CreatedAt.Date <= to.Date);

        var totalOrders = await query.CountAsync(ct);
        var totalRevenue = totalOrders > 0
            ? await query.SumAsync(o => o.TotalAmount, ct)
            : 0m;
        var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0m;

        return (totalRevenue, totalOrders, averageOrderValue);
    }

    public async Task<(decimal PrevRevenue, int PrevOrders)> GetPrevPeriodSummaryAsync(
        DateTime from, DateTime to, CancellationToken ct = default)
    {
        var duration = to - from;
        var prevFrom = from - duration;
        var prevTo = from;

        var query = context.Orders
            .Where(o => o.Status != OrderStatus.Cancelled
                        && o.CreatedAt.Date >= prevFrom.Date
                        && o.CreatedAt.Date < prevTo.Date);

        var prevOrders = await query.CountAsync(ct);
        var prevRevenue = prevOrders > 0
            ? await query.SumAsync(o => o.TotalAmount, ct)
            : 0m;

        return (prevRevenue, prevOrders);
    }

    public async Task<IEnumerable<(DateTime Date, decimal Revenue, int OrderCount)>> GetRevenueByDateAsync(
        DateTime from, DateTime to, CancellationToken ct = default)
    {
        var rows = await context.Orders
            .Where(o => o.Status != OrderStatus.Cancelled
                        && o.CreatedAt.Date >= from.Date
                        && o.CreatedAt.Date <= to.Date)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                Revenue = g.Sum(o => o.TotalAmount),
                OrderCount = g.Count()
            })
            .OrderBy(r => r.Date)
            .ToListAsync(ct);

        return rows.Select(r => (r.Date, r.Revenue, r.OrderCount));
    }

    public async Task<IEnumerable<(Guid ProductId, string ProductName, int TotalQuantity, decimal TotalRevenue)>> GetTopProductsAsync(
        DateTime from, DateTime to, int limit, CancellationToken ct = default)
    {
        // Product items: direct purchases
        var productRows = await context.Orders
            .Where(o => o.Status != OrderStatus.Cancelled
                        && o.CreatedAt.Date >= from.Date
                        && o.CreatedAt.Date <= to.Date)
            .SelectMany(o => o.Items.Where(i => i.ItemType == CartItemType.Product))
            .GroupBy(i => new { i.ProductId, i.ProductName })
            .Select(g => new
            {
                ProductId = g.Key.ProductId!.Value,
                g.Key.ProductName,
                TotalQuantity = g.Sum(i => i.Quantity),
                TotalRevenue = g.Sum(i => i.UnitPrice * i.Quantity)
            })
            .ToListAsync(ct);

        // Combo items: product consumption via OrderItemComponents
        var comboRows = await context.Orders
            .Where(o => o.Status != OrderStatus.Cancelled
                        && o.CreatedAt.Date >= from.Date
                        && o.CreatedAt.Date <= to.Date)
            .SelectMany(o => o.Items.Where(i => i.ItemType == CartItemType.Combo))
            .SelectMany(i => i.Components)
            .GroupBy(c => new { c.ProductId, c.ProductName })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.ProductName,
                TotalQuantity = g.Sum(c => c.TotalQuantity),
                TotalRevenue = g.Sum(c => c.UnitPriceSnapshot * c.TotalQuantity)
            })
            .ToListAsync(ct);

        // Merge by ProductId
        var merged = productRows.Concat(comboRows)
            .GroupBy(r => r.ProductId)
            .Select(g => (
                ProductId: g.Key,
                ProductName: g.First().ProductName,
                TotalQuantity: g.Sum(r => r.TotalQuantity),
                TotalRevenue: g.Sum(r => r.TotalRevenue)
            ))
            .OrderByDescending(r => r.TotalRevenue)
            .Take(limit);

        return merged;
    }

    public async Task<IEnumerable<(string Status, int Count)>> GetOrderStatusBreakdownAsync(
        CancellationToken ct = default)
    {
        var rows = await context.Orders
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return rows.Select(r => (r.Status.ToString(), r.Count));
    }

    public async Task<int> GetNewCustomersCountAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        return await context.Users
            .CountAsync(u => u.CreatedAt.Date >= from.Date && u.CreatedAt.Date <= to.Date, ct);
    }

    public async Task<(int OrderCount, decimal TotalSpent)> GetUserOrderStatsAsync(Guid userId, CancellationToken ct = default)
    {
        var stats = await context.Orders
            .Where(o => o.UserId == userId && o.Status != OrderStatus.Cancelled)
            .GroupBy(o => o.UserId)
            .Select(g => new { OrderCount = g.Count(), TotalSpent = g.Sum(o => o.TotalAmount) })
            .FirstOrDefaultAsync(ct);

        return stats is null ? (0, 0m) : (stats.OrderCount, stats.TotalSpent);
    }
}
