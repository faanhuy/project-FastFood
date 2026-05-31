using SmartShop.Domain.Common;

namespace SmartShop.Domain.Entities;

public class AuditLog : BaseAuditableEntity
{
    private AuditLog() { }

    public Guid? UserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string? EntityType { get; private set; }
    public Guid? EntityId { get; private set; }
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public string IPAddress { get; private set; } = string.Empty;
    public DateTime Timestamp { get; private set; }

    public static AuditLog Create(
        Guid? userId,
        string action,
        string? entityType = null,
        Guid? entityId = null,
        string? oldValues = null,
        string? newValues = null,
        string ipAddress = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);

        return new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            IPAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        };
    }
}
