using MediatR;
using SmartShop.Application.Common.Models;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.PriceCampaigns.Queries.GetBulkEffectivePrices;

public class GetBulkEffectivePricesQueryHandler(
    IPriceCampaignRepository priceCampaignRepo,
    IProductRepository productRepo,
    IStoreInventoryRepository inventoryRepo
) : IRequestHandler<GetBulkEffectivePricesQuery, ApiResponse<List<BulkEffectivePriceResult>>>
{
    public async Task<ApiResponse<List<BulkEffectivePriceResult>>> Handle(
        GetBulkEffectivePricesQuery request, CancellationToken ct)
    {
        var at = request.At ?? DateTime.UtcNow;

        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var basePrices = new Dictionary<Guid, decimal>();

        foreach (var pid in productIds)
        {
            var product = await productRepo.GetByIdAsync(pid, ct);
            if (product is not null)
                basePrices[pid] = product.Price;
        }

        var keys = request.Items.Select(i => (i.ProductId, i.SizeId)).ToList();
        var effectiveRules = await priceCampaignRepo.GetEffectivePriceItemsAsync(
            request.StoreId, keys, at, ct);

        // Bulk load inventory for the selected store
        var inventories = await inventoryRepo.GetByStoreAndProductsAsync(request.StoreId, productIds, ct);
        var stockMap = inventories.ToDictionary(inv => inv.ProductId, inv => inv.Quantity);

        var results = request.Items.Select(item =>
        {
            var basePrice = basePrices.GetValueOrDefault(item.ProductId, 0m);
            var key = (item.ProductId, item.SizeId);
            var stock = stockMap.GetValueOrDefault(item.ProductId, 0);

            if (effectiveRules.TryGetValue(key, out var rule))
            {
                var effectivePrice = (PriceRuleType)rule.ruleType switch
                {
                    PriceRuleType.Coefficient => basePrice * rule.discountValue,
                    PriceRuleType.FixedPrice => rule.discountValue,
                    _ => basePrice
                };

                return new BulkEffectivePriceResult(
                    item.ProductId, item.SizeId, basePrice, effectivePrice, HasPromotion: true, AvailableStock: stock);
            }

            return new BulkEffectivePriceResult(
                item.ProductId, item.SizeId, basePrice, basePrice, HasPromotion: false, AvailableStock: stock);
        }).ToList();

        return ApiResponse<List<BulkEffectivePriceResult>>.Ok(results);
    }
}
