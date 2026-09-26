using SmartShop.Inventory.Domain.Entities;

namespace SmartShop.Inventory.Domain.Interfaces;
public interface IStockReservationRepository
{
    Task<List<StockReservation>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);   // Include(InventoryItem)
    Task AddRangeAsync(IEnumerable<StockReservation> reservations, CancellationToken ct = default);
}