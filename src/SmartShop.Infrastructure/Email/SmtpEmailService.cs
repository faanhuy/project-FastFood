using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using SmartShop.Application.Common.Interfaces;

namespace SmartShop.Infrastructure.Email;

public class SmtpEmailService(
    IConfiguration configuration,
    ILogger<SmtpEmailService> logger) : IEmailService
{
    public async Task SendOrderConfirmationAsync(
        string toEmail,
        string toName,
        Guid orderId,
        string orderNumber,
        List<OrderItemInfo> items,
        decimal totalPrice)
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
                        <td style="padding:8px;border:1px solid #ddd;text-align:right;font-weight:bold;color:#e53935;">{totalPrice:N0} ₫</td>
                    </tr>
                </tfoot>
            </table>
            <p>Cảm ơn bạn đã mua hàng tại SmartShop!</p>
            </body></html>
            """;

        await SendAsync(toEmail, toName, subject, body);
    }

    public async Task SendOrderStatusUpdateAsync(
        string toEmail,
        string toName,
        Guid orderId,
        string orderNumber,
        string newStatus)
    {
        var subject = $"Cập nhật đơn hàng #{orderNumber}";

        var statusLabel = newStatus switch
        {
            "Processing" => "Đang xử lý",
            "Shipped" => "Đang giao hàng",
            "Delivered" => "Đã giao hàng",
            "Cancelled" => "Đã hủy",
            _ => newStatus
        };

        var body = $"""
            <html><body style="font-family:Arial,sans-serif;color:#333;">
            <h2 style="color:#e53935;">SmartShop — Cập nhật đơn hàng</h2>
            <p>Xin chào <strong>{toName}</strong>,</p>
            <p>Đơn hàng <strong>#{orderNumber}</strong> của bạn đã được cập nhật:</p>
            <p style="font-size:18px;font-weight:bold;color:#1976d2;">Trạng thái: {statusLabel}</p>
            <p>Cảm ơn bạn đã mua hàng tại SmartShop!</p>
            </body></html>
            """;

        await SendAsync(toEmail, toName, subject, body);
    }

    public async Task SendReturnApprovedAsync(
        string toEmail,
        string toName,
        Guid orderId,
        string orderNumber,
        decimal refundAmount,
        string? adminNote)
    {
        var subject = $"Yêu cầu trả hàng #{orderNumber} được duyệt";

        var adminNoteHtml = string.IsNullOrWhiteSpace(adminNote)
            ? string.Empty
            : $"<p><strong>Ghi chú:</strong> {adminNote}</p>";

        var body = $"""
            <html><body style="font-family:Arial,sans-serif;color:#333;">
            <h2 style="color:#2e7d32;">SmartShop — Yêu cầu trả hàng được duyệt</h2>
            <p>Xin chào <strong>{toName}</strong>,</p>
            <p>Yêu cầu trả hàng cho đơn hàng <strong>#{orderNumber}</strong> của bạn đã được duyệt!</p>
            <p style="font-size:16px;font-weight:bold;color:#2e7d32;">
                Số tiền hoàn: <span style="font-size:20px;">{refundAmount:N0} ₫</span>
            </p>
            {adminNoteHtml}
            <p>Tiền hoàn sẽ được gửi về tài khoản của bạn trong 3-5 ngày làm việc.</p>
            <p>Cảm ơn bạn đã sử dụng dịch vụ của SmartShop!</p>
            </body></html>
            """;

        await SendAsync(toEmail, toName, subject, body);
    }

    public async Task SendReturnRejectedAsync(
        string toEmail,
        string toName,
        Guid orderId,
        string orderNumber,
        string adminNote)
    {
        var subject = $"Yêu cầu trả hàng #{orderNumber} bị từ chối";

        var body = $"""
            <html><body style="font-family:Arial,sans-serif;color:#333;">
            <h2 style="color:#c62828;">SmartShop — Yêu cầu trả hàng bị từ chối</h2>
            <p>Xin chào <strong>{toName}</strong>,</p>
            <p>Rất tiếc, yêu cầu trả hàng cho đơn hàng <strong>#{orderNumber}</strong> của bạn đã bị từ chối.</p>
            <p><strong>Lý do:</strong></p>
            <p style="background:#f5f5f5;padding:12px;border-left:4px solid #c62828;">{adminNote}</p>
            <p>Nếu bạn có bất kỳ câu hỏi, vui lòng liên hệ đội hỗ trợ của chúng tôi.</p>
            <p>Cảm ơn bạn đã sử dụng dịch vụ của SmartShop!</p>
            </body></html>
            """;

        await SendAsync(toEmail, toName, subject, body);
    }

    public async Task SendPasswordResetAsync(
        string toEmail,
        string toName,
        string tempPassword)
    {
        var subject = "SmartShop — Đặt lại mật khẩu";

        var body = $"""
            <html><body style="font-family:Arial,sans-serif;color:#333;">
            <h2 style="color:#e53935;">SmartShop — Đặt lại mật khẩu</h2>
            <p>Xin chào <strong>{toName}</strong>,</p>
            <p>Quản trị viên đã yêu cầu đặt lại mật khẩu của tài khoản bạn.</p>
            <p>Dưới đây là mật khẩu tạm thời của bạn:</p>
            <p style="font-size:18px;font-weight:bold;background:#f5f5f5;padding:12px;border-left:4px solid #e53935;font-family:monospace;">
                {tempPassword}
            </p>
            <p style="color:#d32f2f;font-weight:bold;">Lưu ý:</p>
            <ul>
                <li>Mật khẩu tạm thời này chỉ có hiệu lực một lần.</li>
                <li>Vui lòng đăng nhập với mật khẩu này và thay đổi thành mật khẩu của riêng bạn.</li>
                <li>Nếu bạn không yêu cầu điều này, vui lòng liên hệ với đội hỗ trợ ngay lập tức.</li>
            </ul>
            <p>Cảm ơn bạn đã sử dụng dịch vụ của SmartShop!</p>
            </body></html>
            """;

        await SendAsync(toEmail, toName, subject, body);
    }

    private async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        // Graceful skip if email is disabled or SmtpHost not configured
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
