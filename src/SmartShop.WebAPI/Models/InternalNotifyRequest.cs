namespace SmartShop.WebAPI.Models;

// Body của POST /api/internal/notify — Notification Service gọi vào để đẩy realtime qua SignalR.
public record InternalNotifyRequest(
    Guid UserId,
    Guid? NotificationId,
    string TitleKey,
    string MessageKey,
    string? ParamsJson,
    Guid? OrderId);
