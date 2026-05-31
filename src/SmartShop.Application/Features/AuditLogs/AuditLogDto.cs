using SmartShop.Domain.Entities;

namespace SmartShop.Application.Features.AuditLogs;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string IPAddress { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }

    public static AuditLogDto From(AuditLog log) => new()
    {
        Id = log.Id,
        UserId = log.UserId,
        Action = log.Action,
        EntityType = log.EntityType,
        EntityId = log.EntityId,
        OldValues = log.OldValues,
        NewValues = log.NewValues,
        IPAddress = log.IPAddress,
        Timestamp = log.Timestamp
    };
}
