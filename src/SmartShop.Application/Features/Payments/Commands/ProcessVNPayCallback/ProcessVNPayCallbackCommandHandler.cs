using MediatR;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Payments.Commands.ProcessVNPayCallback;

public class ProcessVNPayCallbackCommandHandler(
    IOrderRepository orderRepository,
    IPaymentGateway paymentGateway,
    IUnitOfWork unitOfWork) : IRequestHandler<ProcessVNPayCallbackCommand, ApiResponse<bool>>
{
    public async Task<ApiResponse<bool>> Handle(ProcessVNPayCallbackCommand command, CancellationToken ct)
    {
        var callbackResult = paymentGateway.ProcessCallback(command.QueryParams);

        var txnRef = callbackResult.OrderId; // vnp_TxnRef = orderId_timestamp hoặc orderId (backward compat)
        var rawOrderId = txnRef.Contains('_') ? txnRef[..txnRef.LastIndexOf('_')] : txnRef;

        if (!Guid.TryParse(rawOrderId, out var orderId))
            throw new NotFoundException(nameof(Domain.Entities.Order), rawOrderId);

        var order = await orderRepository.GetByIdAsync(orderId, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Order), orderId);

        // Idempotency: chỉ skip nếu đã Paid — Failed vẫn cho retry
        if (order.PaymentStatus == PaymentStatus.Paid)
            return ApiResponse<bool>.Ok(true);

        if (callbackResult.IsSuccess)
        {
            var vnTz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            order.MarkAsPaid(callbackResult.TransactionId,
                TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTz));
        }
        else
            order.MarkPaymentFailed();

        await unitOfWork.SaveChangesAsync(ct);

        return ApiResponse<bool>.Ok(callbackResult.IsSuccess);
    }
}
