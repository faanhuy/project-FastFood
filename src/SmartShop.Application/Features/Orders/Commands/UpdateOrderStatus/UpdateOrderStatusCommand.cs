using MediatR;
using SmartShop.Domain.Enums;

namespace SmartShop.Application.Features.Orders.Commands.UpdateOrderStatus;

public record UpdateOrderStatusCommand(Guid OrderId, OrderStatus NewStatus) : IRequest<OrderDto>;
