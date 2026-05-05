using MediatR;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Payments.Commands.CreateVNPayPayment;

public class CreateVNPayPaymentCommandHandler(
    IOrderRepository orderRepository,
    IPaymentGateway paymentGateway) : IRequestHandler<CreateVNPayPaymentCommand, ApiResponse<string>>
{
    public async Task<ApiResponse<string>> Handle(CreateVNPayPaymentCommand command, CancellationToken ct)
    {
        var order = await orderRepository.GetByIdAsync(command.OrderId, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Order), command.OrderId);

        if (order.UserId.ToString() != command.UserId)
            throw new UnauthorizedException("Không có quyền truy cập đơn hàng này.");

        if (order.PaymentMethod != PaymentMethod.VNPay)
            throw new ConflictException("Đơn hàng này không sử dụng phương thức thanh toán VNPay.");

        if (order.PaymentStatus == PaymentStatus.Paid)
            throw new ConflictException("Đơn hàng này đã thanh toán thành công, không thể tạo lại link thanh toán.");

        if (order.Status == OrderStatus.Cancelled)
            throw new ConflictException("Đơn hàng đã bị hủy, không thể thực hiện thanh toán.");

        if (order.PaymentStatus != PaymentStatus.Pending && order.PaymentStatus != PaymentStatus.Failed)
            throw new ConflictException("Trạng thái thanh toán không hợp lệ để tạo link thanh toán.");

        var txnRef = $"{order.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}";

        var paymentRequest = new CreatePaymentRequest(
            OrderId: order.Id.ToString(),
            TxnRef: txnRef,
            Amount: (long)order.TotalAmount,
            OrderDescription: $"Thanh toan don hang {order.Id}",
            ReturnUrl: command.ReturnUrl,
            IpAddress: command.IpAddress);

        var paymentUrl = paymentGateway.CreatePaymentUrl(paymentRequest);

        return ApiResponse<string>.Ok(paymentUrl, "Tạo link thanh toán thành công.");
    }
}
