using MediatR;

namespace SmartShop.Application.Features.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid UserId, Guid OrderId) : IRequest<OrderDto>;
