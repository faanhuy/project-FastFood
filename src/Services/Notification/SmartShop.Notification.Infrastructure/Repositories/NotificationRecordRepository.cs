using Microsoft.EntityFrameworkCore;
using SmartShop.Notification.Domain.Entities;
using SmartShop.Notification.Domain.Enums;
using SmartShop.Notification.Domain.Interfaces;
using SmartShop.Notification.Infrastructure.Persistence;

namespace SmartShop.Notification.Infrastructure.Repositories;

public class NotificationRecordRepository(NotificationDbContext context) : INotificationRecordRepository
{
    public Task<bool> ExistsAsync(Guid sourceOrderId, string eventType, CancellationToken ct = default) =>
        context.NotificationRecords.AnyAsync(
            x => x.SourceOrderId == sourceOrderId && x.EventType == eventType, ct);

    public async Task AddAsync(NotificationRecord record, CancellationToken ct = default) =>
        await context.NotificationRecords.AddAsync(record, ct);

    public async Task<List<NotificationRecord>> GetDueEmailRetriesAsync(
        DateTime now, TimeSpan stuckPendingGracePeriod, int batchSize, CancellationToken ct = default)
    {
        var stuckBefore = now - stuckPendingGracePeriod;

        return await context.NotificationRecords
            .Where(x =>
                (x.EmailStatus == NotificationEmailStatus.Failed
                    && x.NextEmailRetryAt != null && x.NextEmailRetryAt <= now) ||
                (x.EmailStatus == NotificationEmailStatus.Pending && x.CreatedAt <= stuckBefore))
            .OrderBy(x => x.NextEmailRetryAt ?? x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public void Update(NotificationRecord record) => context.NotificationRecords.Update(record);
}
