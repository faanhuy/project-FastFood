namespace SmartShop.Notification.Domain.Entities;

// Chính sách backoff khi gửi email thất bại — dùng bởi NotificationRecord.RecordEmailFailure(). Lần gửi đầu
// tiên (đồng bộ, trong lúc xử lý Kafka message) không tính vào đây; chỉ từ lần thất bại đầu tiên trở đi mới
// vào backoff. Hết BackoffSchedule (lần thất bại thứ 5) thì chuyển DeadLettered, không thử lại nữa.
public static class EmailRetryPolicy
{
    public const int MaxAttempts = 5;

    public static readonly IReadOnlyList<TimeSpan> BackoffSchedule =
    [
        TimeSpan.FromMinutes(1),
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(30),
        TimeSpan.FromHours(2)
    ];
}
