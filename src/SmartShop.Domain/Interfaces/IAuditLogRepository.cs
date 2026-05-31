using SmartShop.Domain.Entities;

namespace SmartShop.Domain.Interfaces;

public record AuditLogPagedResult(
    IEnumerable<AuditLog> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken ct = default);
    Task<AuditLogPagedResult> GetPagedAsync(
        Guid? userId,
        string? action,
        string? entityType,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
