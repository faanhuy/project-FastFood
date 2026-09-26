using MediatR;

namespace SmartShop.Inventory.Application.Features.Stock.Queries.GetStockLevel;

// Trả về số lượng sellable (QuantityAvailable - QuantityReserved)
public record GetStockLevelQuery(Guid ProductId, Guid StoreId, Guid? SizeId) : IRequest<int>;
