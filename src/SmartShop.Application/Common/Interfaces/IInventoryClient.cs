namespace SmartShop.Application.Common.Interfaces;

/// <summary>
/// Cổng gọi Inventory Service. Application chỉ biết các record thuần bên dưới —
/// proto/gRPC nằm hoàn toàn trong Infrastructure (<c>InventoryGrpcClient</c>).
/// </summary>
public interface IInventoryClient
{
    /// <summary>All-or-nothing: thiếu bất kỳ dòng nào thì không giữ chỗ dòng nào (Success = false).</summary>
    Task<ReserveStockOutcome> CheckAndReserveStockAsync(
        Guid orderId, IReadOnlyList<InventoryStockLine> items, CancellationToken ct = default);

    Task ReleaseStockAsync(Guid orderId, CancellationToken ct = default);

    Task ConfirmStockAsync(Guid orderId, CancellationToken ct = default);

    Task<int> GetStockLevelAsync(Guid productId, Guid storeId, Guid? sizeId, CancellationToken ct = default);
}

public record InventoryStockLine(Guid ProductId, Guid StoreId, Guid? SizeId, int Quantity);

/// <param name="AvailableQuantity">Sellable = available - reserved (sau khi giữ chỗ nếu Reserved = true).</param>
public record InventoryStockLineResult(Guid ProductId, Guid? SizeId, bool Reserved, int AvailableQuantity);

public record ReserveStockOutcome(bool Success, IReadOnlyList<InventoryStockLineResult> Results);
