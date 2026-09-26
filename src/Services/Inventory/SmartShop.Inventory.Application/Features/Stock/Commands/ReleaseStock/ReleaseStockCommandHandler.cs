using MediatR;
using SmartShop.Inventory.Application.Common;
using SmartShop.Inventory.Application.Common.Interfaces;
using SmartShop.Inventory.Domain.Enums;
using SmartShop.Inventory.Domain.Interfaces;

namespace SmartShop.Inventory.Application.Features.Stock.Commands.ReleaseStock;

public class ReleaseStockCommandHandler(
    IStockReservationRepository reservationRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ReleaseStockCommand>
{
    public async Task Handle(ReleaseStockCommand request, CancellationToken cancellationToken)
    {
        await ConcurrencyRetry.RunAsync(async () =>
        {
            var reservations = await reservationRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);

            // Chỉ trả chỗ đang Reserved. Confirmed = hàng đã xuất kho, không đụng tới.
            var toRelease = reservations.Where(r => r.Status == ReservationStatus.Reserved).ToList();

            // Không có gì để trả (order chưa từng giữ chỗ / đã trả rồi) → thành công, không làm gì.
            // Idempotent: Kafka at-least-once có thể gửi cùng 1 event nhiều lần (Sprint 38).
            if (toRelease.Count == 0)
                return true;

            foreach (var reservation in toRelease)
            {
                reservation.InventoryItem!.Release(reservation.Quantity);
                reservation.MarkReleased();
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        });
    }
}
