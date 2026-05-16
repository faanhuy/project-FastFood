using MediatR;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.DTOs;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Products.Queries.GetProductBySlug;

public class GetProductBySlugQueryHandler(
    IProductRepository repository,
    IPriceCampaignRepository priceCampaignRepository,
    ICacheService cache
) : IRequestHandler<GetProductBySlugQuery, ProductDetailDto>
{
    public async Task<ProductDetailDto> Handle(GetProductBySlugQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"products:slug:{request.Slug}:{request.StoreId}";

        var cached = await cache.GetAsync<ProductDetailDto>(cacheKey, cancellationToken);
        if (cached is not null) return cached;

        var product = await repository.GetBySlugWithSizesAsync(request.Slug, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.Slug);

        decimal effectivePrice = product.Price;
        Dictionary<(Guid, Guid?), (int, decimal)> rules = [];

        if (request.StoreId.HasValue)
        {
            var at = DateTime.UtcNow;
            var keys = product.Sizes
                .Where(s => s.IsActive)
                .Select(s => (product.Id, (Guid?)s.Id))
                .Append((product.Id, null))
                .ToList();

            rules = await priceCampaignRepository.GetEffectivePriceItemsAsync(
                request.StoreId.Value, keys, at, cancellationToken);

            if (rules.TryGetValue((product.Id, null), out var productRule))
                effectivePrice = ComputePrice(product.Price, productRule);
            else if (rules.Count > 0)
                effectivePrice = ComputePrice(product.Price, rules.Values.First());
        }

        // Build sizes with per-size effectivePrice
        var sizes = product.Sizes
            .OrderBy(s => s.DisplayOrder)
            .Select(s =>
            {
                decimal? sizeEffective = null;
                if (request.StoreId.HasValue)
                {
                    if (rules.TryGetValue((product.Id, s.Id), out var sr))
                        sizeEffective = ComputePrice(product.Price, sr);
                    else if (rules.TryGetValue((product.Id, null), out var pr))
                        sizeEffective = ComputePrice(product.Price, pr);
                }
                return new SizeDto(s.Id, s.SizeLabel, s.DisplayOrder, s.IsActive, sizeEffective);
            })
            .ToList()
            .AsReadOnly();

        var dto = new ProductDetailDto(
            product.Id, product.Name, product.Description, product.Price, product.OriginalPrice,
            product.Slug, product.ImageUrl, product.IsActive, product.CategoryId, product.CreatedAt,
            HasSizes: product.HasSizes,
            SizeType: product.SizeType?.ToString(),
            Sizes: sizes,
            EffectivePrice: effectivePrice);

        await cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10), cancellationToken);

        return dto;
    }

    private static decimal ComputePrice(decimal basePrice, (int ruleType, decimal discountValue) rule) =>
        (PriceRuleType)rule.ruleType switch
        {
            PriceRuleType.Coefficient => basePrice * rule.discountValue,
            PriceRuleType.FixedPrice => rule.discountValue,
            _ => basePrice
        };
}
