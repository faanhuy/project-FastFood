using MediatR;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateOrderStatusCommand, OrderDto>
{
    public async Task<OrderDto> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdWithItemsAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException("Order", request.OrderId);

        order.UpdateStatus(request.NewStatus);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new OrderDto
        {
            Id              = order.Id,
            UserId          = order.UserId,
            Status          = order.Status.ToString(),
            TotalAmount     = order.TotalAmount,
            ShippingAddress = order.ShippingAddress,
            Notes           = order.Notes,
            Items           = order.Items.Select(i => new OrderItemDto
            {
                ProductId   = i.ProductId,
                ProductName = i.ProductName,
                Quantity    = i.Quantity,
                UnitPrice   = i.UnitPrice,
                SubTotal    = i.SubTotal
            }).ToList(),
            CreatedAt = order.CreatedAt
        };
    }
}
