using MediatR;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Orders.Queries.GetAllOrders;

public record GetAllOrdersQuery(int Page = 1, int PageSize = 20, OrderStatus? StatusFilter = null) : IRequest<PagedResult<OrderDto>>;
