using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.AI.Queries.SemanticSearch;

public class SemanticSearchQueryHandler(
    ISemanticKernelService semanticKernel,
    IProductRepository productRepository,
    IAppSettingRepository settings,
    IPriceCampaignRepository priceCampaignRepository,
    ICacheService cache
) : IRequestHandler<SemanticSearchQuery, IReadOnlyList<SemanticSearchResultDto>>
{
    public async Task<IReadOnlyList<SemanticSearchResultDto>> Handle(
        SemanticSearchQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"ai:search:{request.Query.ToLower().Trim()}:top{request.TopN}:store{request.StoreId}";

        var cached = await cache.GetAsync<List<SemanticSearchResultDto>>(cacheKey, cancellationToken);
        if (cached is not null) return cached;

        var (allProducts, _) = await productRepository.GetPagedAsync(1, int.MaxValue, ct: cancellationToken);

        var minScore     = await settings.GetDoubleAsync("AI:Search:MinScore", defaultValue: 0.3, cancellationToken);
        var activeProducts = allProducts.Where(p => p.IsActive).ToList();

        var candidates = activeProducts.Select(p =>
        {
            var enrichedDesc = $"{p.Description ?? string.Empty} [Giá: {p.Price:N0}đ]";
            return (p.Id, p.Name, enrichedDesc);
        });
        var ranked = await semanticKernel.SemanticSearchAsync(
            request.Query, candidates, request.TopN, cancellationToken);

        var productById = activeProducts.ToDictionary(p => p.Id);
        var matchedProducts = ranked
            .Where(r => r.Score >= minScore && productById.ContainsKey(r.Id))
            .Select(r => (r, productById[r.Id]))
            .ToList();

        // Tính effectivePrice từ promo campaign nếu có storeId
        Dictionary<(Guid, Guid?), (int, decimal)> priceRules = [];
        if (request.StoreId.HasValue && matchedProducts.Count > 0)
        {
            var keys = matchedProducts.Select(x => (x.Item2.Id, (Guid?)null)).ToList();
            priceRules = await priceCampaignRepository.GetEffectivePriceItemsAsync(
                request.StoreId.Value, keys, DateTime.UtcNow, cancellationToken);
        }

        var results = matchedProducts.Select(x =>
        {
            var (r, p) = x;
            decimal? effectivePrice = null;
            if (priceRules.TryGetValue((p.Id, null), out var rule))
            {
                var computed = (PriceRuleType)rule.Item1 switch
                {
                    PriceRuleType.Coefficient => p.Price * rule.Item2,
                    PriceRuleType.FixedPrice  => rule.Item2,
                    _                         => p.Price
                };
                if (computed < p.Price) effectivePrice = computed;
            }

            return new SemanticSearchResultDto(
                p.Id, p.Name, p.Description,
                p.Price, p.OriginalPrice, effectivePrice,
                p.Slug, p.ImageUrl, p.CategoryId, r.Score);
        }).ToList();

        _ = cache.SetAsync(cacheKey, results, TimeSpan.FromMinutes(15), cancellationToken);
        return results;
    }
}
