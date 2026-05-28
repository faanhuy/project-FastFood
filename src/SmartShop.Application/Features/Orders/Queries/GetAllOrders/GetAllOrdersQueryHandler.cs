using MediatR;
using SmartShop.Application.Products.Queries.GetProducts;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Orders.Queries.GetAllOrders;

public class GetCouponsQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetAllOrdersQuery, PagedResult<OrderDto>>
{
    public async Task<PagedResult<OrderDto>> Handle(
        GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await orderRepository.GetAllPagedAsync(
            request.Page, request.PageSize, request.StatusFilter, cancellationToken);

        var dtos = items.Select(o => new OrderDto
        {
            Id                  = o.Id,
            UserId              = o.UserId,
            UserName            = o.User != null ? $"{o.User.FirstName} {o.User.LastName}".Trim() : string.Empty,
            Status              = o.Status.ToString(),
            TotalAmount         = o.TotalAmount,
            ShippingAddress     = string.Join(", ", new[] { o.ShippingStreet, o.ShippingWard?.Name, o.ShippingProvince?.Name }.Where(s => !string.IsNullOrWhiteSpace(s))),
            ShippingAddressId   = o.ShippingAddressId,
            ShippingStreet      = o.ShippingStreet,
            ShippingWardId      = o.ShippingWardId,
            ShippingProvinceId  = o.ShippingProvinceId,
            ShippingWardName    = o.ShippingWard?.Name,
            ShippingProvinceName = o.ShippingProvince?.Name,
            Notes               = o.Notes,
            PaymentMethod       = o.PaymentMethod.ToString(),
            PaymentStatus       = o.PaymentStatus.ToString(),
            PaidAt              = o.PaidAt,
            VnpayTransactionId  = o.VnpayTransactionId,
            Items               = o.Items.Select(OrderMapper.ToDto).ToList(),
            CreatedAt = o.CreatedAt
        });

        return new PagedResult<OrderDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}
