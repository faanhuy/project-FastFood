using Microsoft.EntityFrameworkCore;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;
using SmartShop.Infrastructure.Data;

namespace SmartShop.Infrastructure.Repositories;

public class AuditLogRepository(ApplicationDbContext context) : IAuditLogRepository
{
    public async Task AddAsync(AuditLog log, CancellationToken ct = default)
        => await context.AuditLogs.AddAsync(log, ct);

    public async Task<AuditLogPagedResult> GetPagedAsync(
        Guid? userId,
        string? action,
        string? entityType,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = context.AuditLogs.AsQueryable();

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId);

        if (!string.IsNullOrEmpty(action))
            query = query.Where(a => a.Action == action);

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(a => a.EntityType == entityType);

        if (fromDate.HasValue)
            query = query.Where(a => a.Timestamp >= fromDate);

        if (toDate.HasValue)
            query = query.Where(a => a.Timestamp <= toDate);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new AuditLogPagedResult(items, totalCount, page, pageSize);
    }
}
