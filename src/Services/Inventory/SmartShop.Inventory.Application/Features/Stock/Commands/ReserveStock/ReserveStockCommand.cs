using MediatR;

namespace SmartShop.Inventory.Application.Features.Stock.Commands.ReserveStock;

public record ReserveStockLine(Guid ProductId, Guid StoreId, Guid? SizeId, int Quantity);

public record ReserveStockCommand(Guid OrderId, List<ReserveStockLine> Items) : IRequest<ReserveStockResult>;
