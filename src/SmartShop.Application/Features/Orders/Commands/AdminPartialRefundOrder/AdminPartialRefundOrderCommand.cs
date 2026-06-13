using MediatR;

namespace SmartShop.Application.Features.Orders.Commands.AdminPartialRefundOrder;

public record AdminPartialRefundOrderCommand(
    Guid OrderId,
    Guid AdminUserId,
    List<Guid> SelectedItemIds,
    string? RefundNote) : IRequest<OrderDto>;
