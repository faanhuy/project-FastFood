using MediatR;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Interfaces;
using SmartShop.Contracts.Events;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;
using System.Text.Json;

namespace SmartShop.Application.Features.Payments.Commands.ProcessVNPayCallback;

public class ProcessVNPayCallbackCommandHandler(
    IOrderRepository orderRepository,
    IPaymentGateway paymentGateway,
    IOutboxRepository outboxRepository,
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

        // Idempotency: skip nếu đã Paid, hoặc đã Failed + callback cũng Failed (tránh ghi DB thừa)
        if (order.PaymentStatus == PaymentStatus.Paid)
            return ApiResponse<bool>.Ok(true);

        if (order.PaymentStatus == PaymentStatus.Failed && !callbackResult.IsSuccess)
            return ApiResponse<bool>.Ok(false);

        // Publish Payment*IntegrationEvent qua Outbox (Notification Service consume các event này;
        // Inventory Service cố ý bỏ qua PaymentFailed: đơn chưa bị hủy và khách có thể thanh toán lại,
        // nên chỗ đã giữ chỉ được nhả khi đơn bị hủy)
        OutboxMessage outboxMessage;
        if (callbackResult.IsSuccess)
        {
            var vnTz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            order.MarkAsPaid(callbackResult.TransactionId,
                TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTz));

            var completedEvent = new PaymentCompletedIntegrationEvent(
                OrderId: order.Id,
                TransactionId: callbackResult.TransactionId,
                Amount: order.TotalAmount,
                Method: "VNPay",
                OccurredAt: DateTime.UtcNow);

            outboxMessage = OutboxMessage.Create(
                nameof(PaymentCompletedIntegrationEvent), order.Id.ToString(),
                JsonSerializer.Serialize(completedEvent), DateTime.UtcNow);
        }
        else
        {
            order.MarkPaymentFailed();

            var failedEvent = new PaymentFailedIntegrationEvent(
                OrderId: order.Id,
                Reason: "VNPay callback reported failure",
                OccurredAt: DateTime.UtcNow);

            outboxMessage = OutboxMessage.Create(
                nameof(PaymentFailedIntegrationEvent), order.Id.ToString(),
                JsonSerializer.Serialize(failedEvent), DateTime.UtcNow);
        }

        await outboxRepository.AddAsync(outboxMessage, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return ApiResponse<bool>.Ok(callbackResult.IsSuccess);
    }
}
