namespace SmartShop.Notification.Application.Common.Interfaces;

// Interface riêng của Notification Service — copy pattern từ IEmailService của Core, không reference ngược
// (xem docs/architecture/microservices-boundaries.md, rule "copy pattern nhỏ thay vì shared kernel").
// SendOrderPlacedAsync mặc định không được gọi (Core đã tự gửi email xác nhận đơn hàng, tránh gửi trùng —
// xem RecordNotificationCommandHandler.ShouldAttemptEmail) nhưng vẫn giữ đủ tham số để nội dung khớp Core,
// có thể bật gửi thật bằng config `Notification:SendOrderPlacedEmail` mà không cần sửa code.
public record NotificationOrderItemInfo(string ProductName, int Quantity, decimal UnitPrice);

public interface IEmailService
{
    Task SendOrderPlacedAsync(
        string toEmail,
        string toName,
        Guid orderId,
        string orderNumber,
        List<NotificationOrderItemInfo> items,
        decimal totalAmount);

    Task SendOrderCancelledAsync(string toEmail, string toName, Guid orderId);

    Task SendPaymentCompletedAsync(string toEmail, string toName, Guid orderId, decimal amount, string method);

    Task SendPaymentFailedAsync(string toEmail, string toName, Guid orderId, string? reason);
}
