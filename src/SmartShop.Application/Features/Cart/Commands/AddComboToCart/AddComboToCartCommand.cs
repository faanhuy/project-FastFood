using MediatR;

namespace SmartShop.Application.Features.Cart.Commands.AddComboToCart;

public record AddComboToCartCommand(Guid UserId, Guid ComboId, int Quantity = 1) : IRequest<CartDto>;
