using SmartShop.Application.Interfaces;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Infrastructure.Services;

public class AuditLogService(
    IAuditLogRepository auditLogRepository,
    IUnitOfWork unitOfWork) : IAuditLogService
{
    public async Task LogAsync(
        Guid? userId,
        string action,
        string? entityType = null,
        Guid? entityId = null,
        string? oldValues = null,
        string? newValues = null,
        string ipAddress = "",
        CancellationToken ct = default)
    {
        var log = AuditLog.Create(userId, action, entityType, entityId, oldValues, newValues, ipAddress);
        await auditLogRepository.AddAsync(log, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
