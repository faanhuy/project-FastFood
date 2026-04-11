using MediatR;

namespace SmartShop.Application.Features.Orders.Commands.CancelOrder;

public record CancelOrderCommand(Guid OrderId, Guid UserId) : IRequest;
