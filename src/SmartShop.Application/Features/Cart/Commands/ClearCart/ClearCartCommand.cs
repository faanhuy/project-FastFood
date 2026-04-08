using MediatR;

namespace SmartShop.Application.Features.Cart.Commands.ClearCart;

public record ClearCartCommand(Guid UserId) : IRequest;
