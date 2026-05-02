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

    private async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        var host = configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
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
