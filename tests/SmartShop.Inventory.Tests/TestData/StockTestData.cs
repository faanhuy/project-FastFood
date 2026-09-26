using SmartShop.Inventory.Domain.Entities;
using SmartShop.Inventory.Domain.Enums;

namespace SmartShop.Inventory.Tests.TestData;

internal static class StockTestData
{
    public static readonly Guid StoreId = Guid.Parse("475a37fc-8a72-4a46-a4aa-b0df7dfc36c2");

    public static InventoryItem Item(
        int available = 10,
        Guid? productId = null,
        Guid? storeId = null,
        Guid? sizeId = null,
        int lowStockThreshold = 5)
        => InventoryItem.Create(productId ?? Guid.NewGuid(), storeId ?? StoreId, sizeId, "sku-test", available, lowStockThreshold);

    /// <summary>
    /// Tạo reservation gắn navigation <see cref="StockReservation.InventoryItem"/> (setter private — EF điền khi Include,
    /// test điền bằng reflection) và đưa về đúng trạng thái yêu cầu. Không tự sửa số lượng của item.
    /// </summary>
    public static StockReservation Reservation(
        InventoryItem item, Guid orderId, int quantity, ReservationStatus status = ReservationStatus.Reserved)
    {
        var reservation = StockReservation.Create(orderId, item.Id, quantity);

        typeof(StockReservation)
            .GetProperty(nameof(StockReservation.InventoryItem))!
            .SetValue(reservation, item);

        if (status == ReservationStatus.Confirmed) reservation.MarkConfirmed();
        else if (status == ReservationStatus.Released) reservation.MarkReleased();

        return reservation;
    }
}
