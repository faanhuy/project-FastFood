namespace SmartShop.Application.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(
        Guid? userId,
        string action,
        string? entityType = null,
        Guid? entityId = null,
        string? oldValues = null,
        string? newValues = null,
        string ipAddress = "",
        CancellationToken ct = default);
}
