using MediatR;
using SmartShop.Inventory.Domain.Common.Exceptions;
using SmartShop.Inventory.Domain.Entities;
using SmartShop.Inventory.Domain.Interfaces;

namespace SmartShop.Inventory.Application.Features.Stock.Queries.GetStockLevel;

public class GetStockLevelQueryHandler(IInventoryItemRepository itemRepository)
    : IRequestHandler<GetStockLevelQuery, int>
{
    public async Task<int> Handle(GetStockLevelQuery request, CancellationToken cancellationToken)
    {
        var item = await itemRepository.GetByKeyAsync(
                request.StoreId, request.ProductId, request.SizeId, cancellationToken)
            ?? throw new NotFoundException(nameof(InventoryItem),
                $"{request.ProductId}/{request.StoreId}/{request.SizeId}");

        return item.Sellable;
    }
}
