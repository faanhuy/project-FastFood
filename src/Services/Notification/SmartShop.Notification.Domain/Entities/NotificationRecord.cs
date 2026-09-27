using SmartShop.Notification.Domain.Common;
using SmartShop.Notification.Domain.Enums;

namespace SmartShop.Notification.Domain.Entities;

public class NotificationRecord : BaseAuditableEntity
{
    private NotificationRecord() { }

    public Guid UserId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string TitleKey { get; private set; } = string.Empty;
    public string MessageKey { get; private set; } = string.Empty;
    public string? ParamsJson { get; private set; }
    public bool IsRead { get; private set; }
    public Guid? SourceOrderId { get; private set; }

    public NotificationEmailStatus EmailStatus { get; private set; }
    public int EmailAttemptCount { get; private set; }
    public DateTime? NextEmailRetryAt { get; private set; }
    public string? EmailLastError { get; private set; }
    public DateTime? EmailSentAt { get; private set; }
    public string? EmailPayloadJson { get; private set; }

    public static NotificationRecord Create(
        Guid userId,
        string eventType,
        string titleKey,
        string messageKey,
        string? paramsJson,
        Guid? sourceOrderId,
        string? emailPayloadJson)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));

        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        ArgumentException.ThrowIfNullOrWhiteSpace(titleKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(messageKey);

        return new NotificationRecord
        {
            UserId = userId,
            EventType = eventType,
            TitleKey = titleKey,
            MessageKey = messageKey,
            ParamsJson = paramsJson,
            SourceOrderId = sourceOrderId,
            IsRead = false,
            EmailPayloadJson = emailPayloadJson,
            EmailStatus = emailPayloadJson is null
                ? NotificationEmailStatus.NotApplicable
                : NotificationEmailStatus.Pending
        };
    }

    public void MarkEmailSent()
    {
        EmailStatus = NotificationEmailStatus.Sent;
        EmailSentAt = DateTime.UtcNow;
        NextEmailRetryAt = null;
        EmailLastError = null;
    }

    // Kafka redeliver không tự retry được bước gửi email vì record đã tồn tại (chặn bởi idempotency check
    // ở RecordNotificationCommandHandler) — trạng thái retry phải tự quản ở đây, đọc bởi
    // EmailRetryBackgroundService qua INotificationRecordRepository.GetDueEmailRetriesAsync.
    public void RecordEmailFailure(string error)
    {
        EmailAttemptCount++;
        EmailLastError = Truncate(error, 2000);

        if (EmailAttemptCount >= EmailRetryPolicy.MaxAttempts)
        {
            EmailStatus = NotificationEmailStatus.DeadLettered;
            NextEmailRetryAt = null;
        }
        else
        {
            EmailStatus = NotificationEmailStatus.Failed;
            NextEmailRetryAt = DateTime.UtcNow + EmailRetryPolicy.BackoffSchedule[EmailAttemptCount - 1];
        }
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];
}
