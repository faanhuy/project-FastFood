using MediatR;

namespace SmartShop.Application.Features.Cart.Queries.GetCart;

public record GetCartQuery(Guid UserId) : IRequest<CartDto>;
