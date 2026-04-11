using MediatR;
using SmartShop.Application.Products.Queries.GetProducts;

namespace SmartShop.Application.Features.Orders.Queries.GetAllOrders;

public record GetAllOrdersQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<OrderDto>>;
