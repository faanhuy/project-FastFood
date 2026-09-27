namespace SmartShop.Notification.Application.Common.Interfaces;

public record InternalNotifyPayload(
    Guid UserId,
    Guid? NotificationId,
    string TitleKey,
    string MessageKey,
    string? ParamsJson,
    Guid? OrderId);

// Gọi POST /api/internal/notify ở Core để đẩy realtime qua SignalR — kênh REST nội bộ
// service-to-service (xem docs/architecture/microservices-boundaries.md). Best-effort: Core down/timeout
// không được làm hỏng việc đã ghi NotificationRecord hay gửi email.
public interface ICoreNotifyClient
{
    Task NotifyAsync(InternalNotifyPayload payload, CancellationToken ct = default);
}
