using SmartShop.Notification.Application.Common.Interfaces;

namespace SmartShop.Notification.Application.Features.Notifications.Email;

// Snapshot nội dung email tại thời điểm ghi nhận Kafka event — cần lưu lại (NotificationRecord.EmailPayloadJson)
// vì đây là dữ liệu duy nhất đủ để gửi lại email khi retry, sau khi message Kafka gốc đã xử lý xong.
public record EmailRetryPayload(
    string ToEmail,
    string ToName,
    string? OrderNumber,
    List<NotificationOrderItemInfo>? Items,
    decimal? Amount,
    string? PaymentMethod,
    string? Reason);
