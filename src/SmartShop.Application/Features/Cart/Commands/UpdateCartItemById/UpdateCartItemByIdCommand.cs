using MediatR;

namespace SmartShop.Application.Features.Cart.Commands.UpdateCartItemById;

public record UpdateCartItemByIdCommand(Guid UserId, Guid CartItemId, int Quantity) : IRequest<CartDto>;
