using MediatR;

namespace SmartShop.Application.Features.Cart.Commands.RemoveCartItemById;

public record RemoveCartItemByIdCommand(Guid UserId, Guid CartItemId) : IRequest<CartDto>;
