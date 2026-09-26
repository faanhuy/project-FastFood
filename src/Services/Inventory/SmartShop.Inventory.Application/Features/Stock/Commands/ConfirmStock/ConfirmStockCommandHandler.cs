using MediatR;
using SmartShop.Inventory.Application.Common;
using SmartShop.Inventory.Application.Common.Interfaces;
using SmartShop.Inventory.Domain.Common.Exceptions;
using SmartShop.Inventory.Domain.Entities;
using SmartShop.Inventory.Domain.Enums;
using SmartShop.Inventory.Domain.Interfaces;

namespace SmartShop.Inventory.Application.Features.Stock.Commands.ConfirmStock;

public class ConfirmStockCommandHandler(
    IStockReservationRepository reservationRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ConfirmStockCommand>
{
    public async Task Handle(ConfirmStockCommand request, CancellationToken cancellationToken)
    {
        await ConcurrencyRetry.RunAsync(async () =>
        {
            var reservations = await reservationRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);

            if (reservations.Count == 0)
                throw new NotFoundException(nameof(StockReservation), request.OrderId);

            var toConfirm = reservations.Where(r => r.Status == ReservationStatus.Reserved).ToList();
            if (toConfirm.Count == 0)
            {
                // Chỉ còn Released → không thể xác nhận đơn đã trả chỗ
                if (reservations.All(r => r.Status == ReservationStatus.Released))
                    throw new ConflictException("error.inventory_order_already_released", null);

                // Đã Confirmed hết → thành công, không làm gì (idempotent)
                return true;
            }

            foreach (var reservation in toConfirm)
            {
                reservation.InventoryItem!.Confirm(reservation.Quantity);
                reservation.MarkConfirmed();
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        });
    }
}
