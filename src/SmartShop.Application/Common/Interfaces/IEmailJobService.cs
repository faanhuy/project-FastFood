namespace SmartShop.Application.Common.Interfaces;

public interface IEmailJobService
{
    void EnqueueOrderConfirmation(string toEmail, string toName, Guid orderId, string orderNumber, List<OrderItemInfo> items, decimal totalPrice);
    void EnqueueOrderStatusUpdate(string toEmail, string toName, Guid orderId, string orderNumber, string newStatus);
    void EnqueueReturnApproved(string toEmail, string toName, Guid orderId, string orderNumber, decimal refundAmount, string? adminNote);
    void EnqueueReturnRejected(string toEmail, string toName, Guid orderId, string orderNumber, string adminNote);
}
