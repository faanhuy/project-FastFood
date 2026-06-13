using SmartShop.Domain.Common;

namespace SmartShop.Domain.Entities;

public class OrderArchive : BaseAuditableEntity
{
    private OrderArchive() { }

    public Guid OriginalOrderId { get; private set; }
    public string SnapshotJson { get; private set; } = string.Empty;
    public DateTime ArchivedAt { get; private set; }

    public static OrderArchive Create(Guid originalOrderId, string snapshotJson)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(originalOrderId));
        ArgumentException.ThrowIfNullOrWhiteSpace(snapshotJson);

        return new OrderArchive
        {
            OriginalOrderId = originalOrderId,
            SnapshotJson = snapshotJson,
            ArchivedAt = DateTime.UtcNow
        };
    }
}
