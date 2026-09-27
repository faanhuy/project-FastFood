using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartShop.Notification.Application.Common.Interfaces;
using SmartShop.Notification.Application.Features.Notifications.Email;
using SmartShop.Notification.Domain.Interfaces;

namespace SmartShop.Notification.Infrastructure.BackgroundServices;

// Nhặt lại NotificationRecord đã lỗi khi gửi email lần đầu (xem NotificationRecord.RecordEmailFailure) —
// Kafka redeliver không tự retry được bước này vì record đã tồn tại (chặn bởi idempotency check ở
// RecordNotificationCommandHandler), nên trạng thái retry phải tự quản trong DB thay vì dựa vào Kafka.
public class EmailRetryBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<EmailRetryBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan StuckPendingGracePeriod = TimeSpan.FromMinutes(5);
    private const int BatchSize = 20;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RetryDueBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Email retry loop failed");
            }

            try { await Task.Delay(PollInterval, stoppingToken); }
            catch (TaskCanceledException) { /* shutting down */ }
        }
    }

    private async Task RetryDueBatchAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<INotificationRecordRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var emailSender = scope.ServiceProvider.GetRequiredService<NotificationEmailSender>();

        var due = await repository.GetDueEmailRetriesAsync(DateTime.UtcNow, StuckPendingGracePeriod, BatchSize, ct);
        if (due.Count == 0)
            return;

        // Lưu ngay sau mỗi record (không gộp cuối batch): nếu SaveChanges giữa batch lỗi, record đã gửi
        // thành công trước đó trong batch không bị mất trạng thái Sent — tránh gửi trùng email lần poll sau.
        foreach (var record in due)
        {
            try
            {
                await emailSender.SendAsync(
                    record.EventType, record.EmailPayloadJson!, record.SourceOrderId!.Value, ct);
                record.MarkEmailSent();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Email retry attempt failed for record {RecordId} (order {OrderId}, attempt {Attempt}).",
                    record.Id, record.SourceOrderId, record.EmailAttemptCount + 1);
                record.RecordEmailFailure(ex.Message);
            }

            try
            {
                repository.Update(record);
                await unitOfWork.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to persist email retry result for record {RecordId} (order {OrderId}).",
                    record.Id, record.SourceOrderId);
            }
        }
    }
}
