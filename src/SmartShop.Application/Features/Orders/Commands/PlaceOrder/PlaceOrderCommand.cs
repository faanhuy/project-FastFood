using MediatR;

namespace SmartShop.Application.Features.Orders.Commands.PlaceOrder;

public record PlaceOrderCommand(Guid UserId, string ShippingAddress, string? Notes) : IRequest<OrderDto>;
