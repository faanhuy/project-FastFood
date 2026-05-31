using MediatR;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Orders.Queries.GetMyOrders;

public record GetMyOrdersQuery(Guid UserId, int Page = 1, int PageSize = 10) : IRequest<PagedResult<OrderDto>>;
