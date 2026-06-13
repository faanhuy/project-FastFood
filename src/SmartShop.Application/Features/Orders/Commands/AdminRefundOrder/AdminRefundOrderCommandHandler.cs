using MediatR;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Orders.Commands.AdminRefundOrder;

public class AdminRefundOrderCommandHandler(
    IOrderRepository orderRepository,
    IOrderStatusHistoryRepository statusHistoryRepository,
    IReturnRequestRepository returnRequestRepository,
    IPaymentGateway paymentGateway,
    IUnitOfWork unitOfWork) : IRequestHandler<AdminRefundOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(AdminRefundOrderCommand request, CancellationToken cancellationToken)
    {
        // 1. Load order with return request
        var order = await orderRepository.GetByIdWithItemsAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        // 2. Get return request
        var returnRequest = await returnRequestRepository.GetByOrderIdAsync(order.Id, cancellationToken)
            ?? throw new ConflictException("error.order_no_return_request", null);

        // 3. Validate: return request must be approved
        if (returnRequest.Status != ReturnStatus.Approved)
            throw new ConflictException("error.refund_requires_approved", null);

        // 4. Validate: not already refunded
        if (returnRequest.RefundedAt.HasValue)
            throw new ConflictException("error.order_already_refunded", null);

        // 5. Process refund based on payment method
        string? vnpayRefundTxnRef = null;
        string finalRefundNote = request.RefundNote ?? "";

        if (order.PaymentMethod == PaymentMethod.VNPay)
        {
            if (string.IsNullOrEmpty(order.VnpayTransactionId))
                throw new ConflictException("error.vnpay_original_txn_not_found", null);

            var refundResult = await paymentGateway.RefundAsync(
                order.VnpayTransactionId,
                (long)order.TotalAmount,
                $"REFUND-{order.Id}",
                cancellationToken);

            if (!refundResult.IsSuccess)
                throw new ConflictException("error.vnpay_refund_failed",
                    new Dictionary<string, string> { ["reason"] = $"{refundResult.ErrorMessage ?? refundResult.ResponseCode}" });

            vnpayRefundTxnRef = refundResult.RefundTransactionId;
        }
        else if (order.PaymentMethod == PaymentMethod.COD)
        {
            finalRefundNote = $"{finalRefundNote}\n[COD - Manual Refund]".Trim();
        }

        // 6. Mark return request as refunded
        returnRequest.MarkAsRefunded(vnpayRefundTxnRef, finalRefundNote);

        // 7. Update order status to Refunded
        var previousStatus = order.Status;
        order.UpdateStatus(OrderStatus.Refunded);

        // 8. Create status history record
        var statusHistory = OrderStatusHistory.Create(
            order.Id,
            previousStatus,
            OrderStatus.Refunded,
            request.AdminUserId,
            $"Hoàn tiền: {finalRefundNote}");

        await statusHistoryRepository.AddAsync(statusHistory, cancellationToken);

        // 9. Save changes
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            UserName = order.User != null ? $"{order.User.FirstName} {order.User.LastName}".Trim() : "",
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            DiscountAmount = order.DiscountAmount,
            OriginalAmount = order.OriginalAmount,
            ShippingAddress = order.ShippingStreet ?? "",
            ShippingAddressId = order.ShippingAddressId,
            ShippingStreet = order.ShippingStreet,
            ShippingWardId = order.ShippingWardId,
            ShippingProvinceId = order.ShippingProvinceId,
            ShippingWardName = order.ShippingWard?.Name,
            ShippingProvinceName = order.ShippingProvince?.Name,
            Notes = order.Notes,
            CouponCode = order.CouponCode,
            PaymentMethod = order.PaymentMethod.ToString(),
            PaymentStatus = order.PaymentStatus.ToString(),
            PaidAt = order.PaidAt,
            VnpayTransactionId = order.VnpayTransactionId,
            Items = order.Items.Select(OrderMapper.ToDto).ToList(),
            CreatedAt = order.CreatedAt
        };
    }
}
