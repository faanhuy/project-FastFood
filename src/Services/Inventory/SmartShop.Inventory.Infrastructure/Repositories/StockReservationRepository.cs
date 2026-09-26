using Microsoft.EntityFrameworkCore;
using SmartShop.Inventory.Domain.Entities;
using SmartShop.Inventory.Domain.Interfaces;
using SmartShop.Inventory.Infrastructure.Persistence;

namespace SmartShop.Inventory.Infrastructure.Repositories;

public class StockReservationRepository(InventoryDbContext context) : IStockReservationRepository
{
    // Include(InventoryItem) để Release/Confirm sửa item mà không phải truy vấn thêm
    public Task<List<StockReservation>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        => context.StockReservations
            .Include(r => r.InventoryItem)
            .Where(r => r.OrderId == orderId)
            .ToListAsync(ct);

    public Task AddRangeAsync(IEnumerable<StockReservation> reservations, CancellationToken ct = default)
        => context.StockReservations.AddRangeAsync(reservations, ct);
}
