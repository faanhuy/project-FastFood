using MediatR;

namespace SmartShop.Application.Features.Orders.Commands.AdminRefundOrder;

public record AdminRefundOrderCommand(
    Guid OrderId,
    Guid AdminUserId,
    string? RefundNote) : IRequest<OrderDto>;
