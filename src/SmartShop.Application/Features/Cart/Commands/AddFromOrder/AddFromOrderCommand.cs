using MediatR;

namespace SmartShop.Application.Features.Cart.Commands.AddFromOrder;

public record AddFromOrderCommand(Guid UserId, Guid OrderId) : IRequest<CartDto>;
