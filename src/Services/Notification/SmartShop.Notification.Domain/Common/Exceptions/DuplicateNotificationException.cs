namespace SmartShop.Notification.Domain.Common.Exceptions;

// Kafka giao message ít nhất 1 lần: 2 consumer instance có thể cùng insert 1 record cho cùng
// (SourceOrderId, EventType) — unique index ở DB là lớp chặn cuối, đây là cách Infrastructure báo lại
// cho Application biết ghi đè đã bị chặn, không phải lỗi thật.
public class DuplicateNotificationException(Guid? sourceOrderId, string eventType)
    : Exception($"Notification for order '{sourceOrderId}' and event '{eventType}' already exists.")
{
    public Guid? SourceOrderId { get; } = sourceOrderId;
    public string EventType { get; } = eventType;
}
