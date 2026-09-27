using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using SmartShop.Notification.Application.Common.Interfaces;

namespace SmartShop.Notification.Infrastructure.Email;

// Copy pattern từ SmtpEmailService của Core (xem docs/architecture/microservices-boundaries.md,
// rule "copy pattern nhỏ thay vì shared kernel") — cấu hình Email:* riêng, không dùng chung config Core.
public class SmtpEmailService(
    IConfiguration configuration,
    ILogger<SmtpEmailService> logger) : IEmailService
{
    public Task SendOrderCancelledAsync(string toEmail, string toName, Guid orderId)
    {
        var orderNumber = FormatOrderNumber(orderId);
        var subject = $"Đơn hàng #{orderNumber} đã bị hủy";
        var body = $"""
            <html><body style="font-family:Arial,sans-serif;color:#333;">
            <h2 style="color:#c62828;">SmartShop — Đơn hàng đã hủy</h2>
            <p>Xin chào <strong>{toName}</strong>,</p>
            <p>Đơn hàng <strong>#{orderNumber}</strong> của bạn đã được hủy thành công.</p>
            <p>Cảm ơn bạn đã sử dụng dịch vụ của SmartShop!</p>
            </body></html>
            """;

        return SendAsync(toEmail, toName, subject, body);
    }

    // Nội dung khớp SendOrderConfirmationAsync của Core (src/SmartShop.Infrastructure/Email/SmtpEmailService.cs)
    // — cùng format mã đơn ngắn, bảng sản phẩm, tổng tiền. Có gửi thật hay không do config
    // `Notification:SendOrderPlacedEmail` (RecordNotificationCommandHandler.ShouldAttemptEmail) — mặc định tắt
    // vì Core cũng tự gửi email xác nhận đơn hàng, tránh trùng.
    public Task SendOrderPlacedAsync(
        string toEmail,
        string toName,
        Guid orderId,
        string orderNumber,
        List<NotificationOrderItemInfo> items,
        decimal totalAmount)
    {
        var subject = $"Xác nhận đơn hàng #{orderNumber}";

        var itemRows = string.Join("\n", items.Select(i =>
            $"""
            <tr>
                <td style="padding:8px;border:1px solid #ddd;">{i.ProductName}</td>
                <td style="padding:8px;border:1px solid #ddd;text-align:center;">{i.Quantity}</td>
                <td style="padding:8px;border:1px solid #ddd;text-align:right;">{i.UnitPrice:N0} ₫</td>
                <td style="padding:8px;border:1px solid #ddd;text-align:right;">{i.UnitPrice * i.Quantity:N0} ₫</td>
            </tr>
            """));

        var body = $"""
            <html><body style="font-family:Arial,sans-serif;color:#333;">
            <h2 style="color:#e53935;">SmartShop — Xác nhận đơn hàng</h2>
            <p>Xin chào <strong>{toName}</strong>,</p>
            <p>Đơn hàng <strong>#{orderNumber}</strong> của bạn đã được đặt thành công!</p>
            <table style="border-collapse:collapse;width:100%;margin:16px 0;">
                <thead>
                    <tr style="background:#f5f5f5;">
                        <th style="padding:8px;border:1px solid #ddd;text-align:left;">Sản phẩm</th>
                        <th style="padding:8px;border:1px solid #ddd;">Số lượng</th>
                        <th style="padding:8px;border:1px solid #ddd;">Đơn giá</th>
                        <th style="padding:8px;border:1px solid #ddd;">Thành tiền</th>
                    </tr>
                </thead>
                <tbody>
                    {itemRows}
                </tbody>
                <tfoot>
                    <tr>
                        <td colspan="3" style="padding:8px;border:1px solid #ddd;text-align:right;font-weight:bold;">Tổng cộng:</td>
                        <td style="padding:8px;border:1px solid #ddd;text-align:right;font-weight:bold;color:#e53935;">{totalAmount:N0} ₫</td>
                    </tr>
                </tfoot>
            </table>
            <p>Cảm ơn bạn đã mua hàng tại SmartShop!</p>
            </body></html>
            """;

        return SendAsync(toEmail, toName, subject, body);
    }


    public Task SendPaymentCompletedAsync(string toEmail, string toName, Guid orderId, decimal amount, string method)
    {
        var orderNumber = FormatOrderNumber(orderId);
        var subject = $"Thanh toán đơn hàng #{orderNumber} thành công";
        var body = $"""
            <html><body style="font-family:Arial,sans-serif;color:#333;">
            <h2 style="color:#2e7d32;">SmartShop — Thanh toán thành công</h2>
            <p>Xin chào <strong>{toName}</strong>,</p>
            <p>Đơn hàng <strong>#{orderNumber}</strong> đã được thanh toán qua <strong>{method}</strong>.</p>
            <p style="font-size:18px;font-weight:bold;color:#2e7d32;">Số tiền: {amount:N0} ₫</p>
            <p>Cảm ơn bạn đã mua hàng tại SmartShop!</p>
            </body></html>
            """;

        return SendAsync(toEmail, toName, subject, body);
    }

    public Task SendPaymentFailedAsync(string toEmail, string toName, Guid orderId, string? reason)
    {
        var orderNumber = FormatOrderNumber(orderId);
        var subject = $"Thanh toán đơn hàng #{orderNumber} thất bại";
        var reasonHtml = string.IsNullOrWhiteSpace(reason)
            ? string.Empty
            : $"<p><strong>Lý do:</strong> {reason}</p>";

        var body = $"""
            <html><body style="font-family:Arial,sans-serif;color:#333;">
            <h2 style="color:#c62828;">SmartShop — Thanh toán thất bại</h2>
            <p>Xin chào <strong>{toName}</strong>,</p>
            <p>Thanh toán cho đơn hàng <strong>#{orderNumber}</strong> của bạn không thành công.</p>
            {reasonHtml}
            <p>Bạn có thể thử thanh toán lại cho đơn hàng này.</p>
            </body></html>
            """;

        return SendAsync(toEmail, toName, subject, body);
    }

    // Cùng format mã đơn ngắn với SendOrderPlacedAsync/Core (orderNumber = orderId.ToString("N")[..8].ToUpper())
    // — 3 method trên trước đây hiển thị nguyên Guid, không nhất quán với email đặt hàng.
    private static string FormatOrderNumber(Guid orderId) => orderId.ToString("N")[..8].ToUpper();

    private async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        var enabled = configuration.GetValue("Email:Enabled", false);
        var host = configuration["Email:SmtpHost"];
        if (!enabled || string.IsNullOrWhiteSpace(host))
        {
            logger.LogInformation("Email skipped (disabled or SmtpHost not configured): {Subject}", subject);
            return;
        }

        var port = int.Parse(configuration["Email:SmtpPort"] ?? "587");
        var username = configuration["Email:Username"] ?? string.Empty;
        var password = configuration["Email:Password"] ?? string.Empty;
        var fromName = configuration["Email:FromName"] ?? "SmartShop";
        var fromAddress = configuration["Email:FromAddress"] ?? username;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromAddress));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(username, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        logger.LogInformation("Email đã gửi tới {ToEmail}: {Subject}", toEmail, subject);
    }
}
