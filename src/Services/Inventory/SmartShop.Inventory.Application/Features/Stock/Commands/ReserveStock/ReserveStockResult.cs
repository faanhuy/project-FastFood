namespace SmartShop.Inventory.Application.Features.Stock.Commands.ReserveStock;

// AvailableQuantity = sellable (QuantityAvailable - QuantityReserved)
public record ReserveStockLineResult(Guid ProductId, Guid StoreId, Guid? SizeId, bool Reserved, int AvailableQuantity);

public record ReserveStockResult(bool Success, List<ReserveStockLineResult> Results);
