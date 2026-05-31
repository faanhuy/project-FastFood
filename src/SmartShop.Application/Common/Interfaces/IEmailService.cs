namespace SmartShop.Application.Common.Interfaces;

public record OrderItemInfo(string ProductName, int Quantity, decimal UnitPrice);

public interface IEmailService
{
    Task SendOrderConfirmationAsync(
        string toEmail,
        string toName,
        Guid orderId,
        string orderNumber,
        List<OrderItemInfo> items,
        decimal totalPrice);

    Task SendOrderStatusUpdateAsync(
        string toEmail,
        string toName,
        Guid orderId,
        string orderNumber,
        string newStatus);

    Task SendReturnApprovedAsync(
        string toEmail,
        string toName,
        Guid orderId,
        string orderNumber,
        decimal refundAmount,
        string? adminNote);

    Task SendReturnRejectedAsync(
        string toEmail,
        string toName,
        Guid orderId,
        string orderNumber,
        string adminNote);
}
