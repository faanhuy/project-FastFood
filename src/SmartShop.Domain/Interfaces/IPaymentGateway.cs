namespace SmartShop.Domain.Interfaces;

public interface IPaymentGateway
{
    string CreatePaymentUrl(CreatePaymentRequest request);
    VNPayCallbackResult ProcessCallback(IDictionary<string, string> queryParams);
    Task<VNPayRefundResult> RefundAsync(string originalTransactionId, long refundAmountVnd, string transactionRef, CancellationToken ct = default);
}

public record CreatePaymentRequest(
    string OrderId,
    string TxnRef,
    long Amount,
    string OrderDescription,
    string ReturnUrl,
    string IpAddress);

public record VNPayCallbackResult(
    bool IsSuccess,
    string TransactionId,
    string OrderId,
    string ResponseCode);

public record VNPayRefundResult(
    bool IsSuccess,
    string RefundTransactionId,
    string ResponseCode,
    string? ErrorMessage);
