using MediatR;
using SmartShop.Application.Common.Exceptions;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Orders.Commands.CancelOrder;

public class CancelOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CancelOrderCommand>
{
    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException("Order", request.OrderId);

        if (order.UserId != request.UserId)
            throw new UnauthorizedException("Bạn không có quyền huỷ đơn hàng này.");

        if (order.Status != OrderStatus.Pending)
            throw new ConflictException("Chỉ có thể huỷ đơn hàng đang ở trạng thái Chờ xác nhận.");

        order.Cancel();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
