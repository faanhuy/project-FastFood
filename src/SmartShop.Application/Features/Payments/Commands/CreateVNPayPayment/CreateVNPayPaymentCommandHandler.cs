using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Payments.Commands.CreateVNPayPayment;

public class CreateVNPayPaymentCommandHandler(
    IOrderRepository orderRepository,
    IPaymentGateway paymentGateway,
    ILocalizationService localization,
    ICurrentLanguageService languageService) : IRequestHandler<CreateVNPayPaymentCommand, ApiResponse<string>>
{
    public async Task<ApiResponse<string>> Handle(CreateVNPayPaymentCommand command, CancellationToken ct)
    {
        var order = await orderRepository.GetByIdAsync(command.OrderId, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Order), command.OrderId);

        if (order.UserId.ToString() != command.UserId)
            throw new UnauthorizedException("error.payment_unauthorized", null);

        if (order.PaymentMethod != PaymentMethod.VNPay)
            throw new ConflictException("error.payment_not_vnpay", null);

        if (order.PaymentStatus == PaymentStatus.Paid)
            throw new ConflictException("error.payment_already_paid", null);

        if (order.Status == OrderStatus.Cancelled)
            throw new ConflictException("error.payment_order_cancelled", null);

        if (order.PaymentStatus != PaymentStatus.Pending && order.PaymentStatus != PaymentStatus.Failed)
            throw new ConflictException("error.payment_invalid_status", null);

        var txnRef = $"{order.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}";

        var paymentRequest = new CreatePaymentRequest(
            OrderId: order.Id.ToString(),
            TxnRef: txnRef,
            Amount: (long)order.TotalAmount,
            OrderDescription: $"Thanh toan don hang {order.Id}",
            ReturnUrl: command.ReturnUrl,
            IpAddress: command.IpAddress);

        var paymentUrl = paymentGateway.CreatePaymentUrl(paymentRequest);

        return ApiResponse<string>.Ok(paymentUrl,
            localization.GetMessage("success.payment_link_created", languageService.Language));
    }
}
