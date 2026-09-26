using SmartShop.Inventory.Domain.Common;
using SmartShop.Inventory.Domain.Common.Exceptions;
using SmartShop.Inventory.Domain.Enums;

namespace SmartShop.Inventory.Domain.Entities;

public class StockReservation : BaseEntity
{
    private StockReservation() { }

    public Guid OrderId { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public int Quantity { get; private set; }
    public ReservationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public InventoryItem? InventoryItem { get; private set; }   // navigation, để Release/Confirm không phải query lại

    public static StockReservation Create(Guid orderId, Guid inventoryItemId, int quantity)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId is required.", nameof(orderId));
        if (inventoryItemId == Guid.Empty)
            throw new ArgumentException("InventoryItemId is required.", nameof(inventoryItemId));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        return new StockReservation
        {
            OrderId = orderId,
            InventoryItemId = inventoryItemId,
            Quantity = quantity,
            Status = ReservationStatus.Reserved,
            CreatedAt = DateTime.UtcNow      // UTC: Npgsql timestamptz bắt buộc Kind=Utc
        };
    }

    public void MarkConfirmed()
    {
        EnsureReserved();
        Status = ReservationStatus.Confirmed;
    }

    public void MarkReleased()
    {
        EnsureReserved();
        Status = ReservationStatus.Released;
    }

    // Chỉ có 2 đường chuyển hợp lệ: Reserved → Confirmed và Reserved → Released
    private void EnsureReserved()
    {
        if (Status != ReservationStatus.Reserved)
            throw new ConflictException("error.inventory_reservation_invalid_status",
                new Dictionary<string, string> { ["status"] = Status.ToString() });
    }
}
