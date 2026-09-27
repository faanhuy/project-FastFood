using SmartShop.Notification.Domain.Entities;

namespace SmartShop.Notification.Domain.Interfaces;

public interface INotificationRecordRepository
{
    Task<bool> ExistsAsync(Guid sourceOrderId, string eventType, CancellationToken ct = default);
    Task AddAsync(NotificationRecord record, CancellationToken ct = default);

    Task<List<NotificationRecord>> GetDueEmailRetriesAsync(
        DateTime now, TimeSpan stuckPendingGracePeriod, int batchSize, CancellationToken ct = default);

    void Update(NotificationRecord record);
}
