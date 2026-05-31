using MediatR;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Orders.Queries.GetOrderById;

public class GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdWithItemsAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException("Order", request.OrderId);

        if (order.UserId != request.UserId)
            throw new UnauthorizedException("error.order_view_unauthorized", null);

        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            ShippingAddress = string.Join(", ", new[] { order.ShippingStreet, order.ShippingWard?.Name, order.ShippingProvince?.Name }.Where(s => !string.IsNullOrWhiteSpace(s))),
            ShippingAddressId = order.ShippingAddressId,
            ShippingStreet = order.ShippingStreet,
            ShippingWardId = order.ShippingWardId,
            ShippingProvinceId = order.ShippingProvinceId,
            ShippingWardName = order.ShippingWard?.Name,
            ShippingProvinceName = order.ShippingProvince?.Name,
            Notes = order.Notes,
            PaymentMethod = order.PaymentMethod.ToString(),
            PaymentStatus = order.PaymentStatus.ToString(),
            PaidAt = order.PaidAt,
            VnpayTransactionId = order.VnpayTransactionId,
            Items = order.Items.Select(OrderMapper.ToDto).ToList(),
            CreatedAt = order.CreatedAt
        };
    }
}
