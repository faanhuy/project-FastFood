using Hangfire;
using SmartShop.Application.Common.Interfaces;

namespace SmartShop.Infrastructure.Email;

public class HangfireEmailJobService(IBackgroundJobClient jobClient) : IEmailJobService
{
    public void EnqueueOrderConfirmation(string toEmail, string toName, Guid orderId, string orderNumber, List<OrderItemInfo> items, decimal totalPrice)
        => jobClient.Enqueue<IEmailService>(s => s.SendOrderConfirmationAsync(toEmail, toName, orderId, orderNumber, items, totalPrice));

    public void EnqueueOrderStatusUpdate(string toEmail, string toName, Guid orderId, string orderNumber, string newStatus)
        => jobClient.Enqueue<IEmailService>(s => s.SendOrderStatusUpdateAsync(toEmail, toName, orderId, orderNumber, newStatus));

    public void EnqueueReturnApproved(string toEmail, string toName, Guid orderId, string orderNumber, decimal refundAmount, string? adminNote)
        => jobClient.Enqueue<IEmailService>(s => s.SendReturnApprovedAsync(toEmail, toName, orderId, orderNumber, refundAmount, adminNote));

    public void EnqueueReturnRejected(string toEmail, string toName, Guid orderId, string orderNumber, string adminNote)
        => jobClient.Enqueue<IEmailService>(s => s.SendReturnRejectedAsync(toEmail, toName, orderId, orderNumber, adminNote));
}
