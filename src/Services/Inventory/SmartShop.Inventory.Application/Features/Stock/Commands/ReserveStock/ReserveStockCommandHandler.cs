using MediatR;
using SmartShop.Inventory.Application.Common;
using SmartShop.Inventory.Application.Common.Interfaces;
using SmartShop.Inventory.Domain.Common.Exceptions;
using SmartShop.Inventory.Domain.Entities;
using SmartShop.Inventory.Domain.Enums;
using SmartShop.Inventory.Domain.Interfaces;

namespace SmartShop.Inventory.Application.Features.Stock.Commands.ReserveStock;

public class ReserveStockCommandHandler(
    IInventoryItemRepository itemRepository,
    IStockReservationRepository reservationRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ReserveStockCommand, ReserveStockResult>
{
    public async Task<ReserveStockResult> Handle(ReserveStockCommand request, CancellationToken cancellationToken)
    {
        // 1. Gộp các dòng trùng khoá (StoreId, ProductId, SizeId) rồi cộng số lượng —
        //    giỏ hàng có thể chứa cùng 1 sản phẩm ở nhiều dòng (mua lẻ + nằm trong combo).
        var lines = request.Items
            .GroupBy(i => (i.StoreId, i.ProductId, i.SizeId))
            .Select(g => new ReserveStockLine(g.Key.ProductId, g.Key.StoreId, g.Key.SizeId, g.Sum(x => x.Quantity)))
            .ToList();

        // 2. Idempotency: order_id đã có reservation thì không giữ chỗ thêm lần nữa
        var existing = await reservationRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        if (existing.Count > 0)
            return ReplayExisting(existing);

        // 3-6. Tải → đánh giá → sửa → lưu; xung đột xmin thì tải lại và thử lần 2
        return await ConcurrencyRetry.RunAsync(() => TryReserveAsync(request.OrderId, lines, cancellationToken));
    }

    private static ReserveStockResult ReplayExisting(List<StockReservation> existing)
    {
        if (existing.All(r => r.Status == ReservationStatus.Released))
            throw new ConflictException("error.inventory_order_already_released", null);

        var results = existing
            .Where(r => r.Status != ReservationStatus.Released)
            .Select(r => new ReserveStockLineResult(
                r.InventoryItem!.ProductId,
                r.InventoryItem.StoreId,
                r.InventoryItem.SizeId,
                true,
                r.InventoryItem.Sellable))
            .ToList();

        return new ReserveStockResult(true, results);
    }

    private async Task<ReserveStockResult> TryReserveAsync(
        Guid orderId, List<ReserveStockLine> lines, CancellationToken cancellationToken)
    {
        // Mỗi lần thử đều tải lại item (sau xung đột, tracker đã bị Clear)
        var keys = lines.Select(l => (l.StoreId, l.ProductId, l.SizeId)).ToList();
        var items = (await itemRepository.GetByKeysAsync(keys, cancellationToken))
            .ToDictionary(i => (i.StoreId, i.ProductId, i.SizeId));

        // Đánh giá toàn bộ dòng TRƯỚC, chưa sửa item nào
        var evaluation = lines.Select(l =>
        {
            items.TryGetValue((l.StoreId, l.ProductId, l.SizeId), out var item);
            var sellable = item?.Sellable ?? 0;
            return new ReserveStockLineResult(
                l.ProductId, l.StoreId, l.SizeId,
                item is not null && sellable >= l.Quantity,
                sellable);
        }).ToList();

        // All-or-nothing: thiếu bất kỳ dòng nào → không giữ chỗ gì cả
        if (evaluation.Any(r => !r.Reserved))
            return new ReserveStockResult(false, evaluation);

        var reservations = new List<StockReservation>();
        foreach (var line in lines.OrderBy(l => items[(l.StoreId, l.ProductId, l.SizeId)].Id))
        {
            var item = items[(line.StoreId, line.ProductId, line.SizeId)];
            item.Reserve(line.Quantity);
            reservations.Add(StockReservation.Create(orderId, item.Id, line.Quantity));
        }

        await reservationRepository.AddRangeAsync(reservations, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);    // 1 transaction duy nhất

        // AvailableQuantity trả về là số còn lại SAU khi đã giữ chỗ
        var results = lines.Select(l =>
        {
            var item = items[(l.StoreId, l.ProductId, l.SizeId)];
            return new ReserveStockLineResult(l.ProductId, l.StoreId, l.SizeId, true, item.Sellable);
        }).ToList();

        return new ReserveStockResult(true, results);
    }
}
