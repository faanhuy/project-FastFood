using MediatR;
using SmartShop.Application.Products.Queries.GetProducts;
using SmartShop.Domain.Enums;

namespace SmartShop.Application.Features.Orders.Queries.GetAllOrders;

public record GetAllOrdersQuery(int Page = 1, int PageSize = 20, OrderStatus? StatusFilter = null) : IRequest<PagedResult<OrderDto>>;
