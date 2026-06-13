using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.DTOs;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Products.Queries.GetProducts;

public class GetProductsQueryHandler(
    IProductRepository repository,
    IPriceCampaignRepository priceCampaignRepository,
    ICacheService cache
) : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var sortByKey = request.SortBy.ToString().ToLower();
        var cacheKey = $"products:list:p{request.Page}:ps{request.PageSize}:cat{request.CategoryId}:q{request.Search}:s{sortByKey}:store{request.StoreId ?? Guid.Empty}";

        // Skip cache for admin filters (isActiveFilter, priceMin, priceMax)
        var isAdminQuery = request.IsActiveFilter.HasValue || request.PriceMin.HasValue || request.PriceMax.HasValue;

        if (!isAdminQuery)
        {
            var cached = await cache.GetAsync<PagedResult<ProductDto>>(cacheKey, cancellationToken);
            if (cached is not null) return cached;
        }

        var sortByStr = request.SortBy switch
        {
            ProductSortBy.PriceAsc    => "price_asc",
            ProductSortBy.PriceDesc   => "price_desc",
            ProductSortBy.NameAsc     => "name_asc",
            ProductSortBy.NameDesc    => "name_desc",
            ProductSortBy.BestSelling => "best_selling",
            _                         => "newest",
        };

        var (items, totalCount) = isAdminQuery
            ? await repository.GetPagedAsync(
                request.Page, request.PageSize, request.CategoryId, request.Search,
                sortByStr, request.IsActiveFilter, request.PriceMin, request.PriceMax, cancellationToken)
            : await repository.GetPagedAsync(
                request.Page, request.PageSize, request.CategoryId, request.Search, sortByStr, cancellationToken);

        List<ProductDto> dtos;

        // Load effective prices if storeId provided
        if (request.StoreId.HasValue)
        {
            var at = DateTime.UtcNow;
            var keys = items.Select(p => (p.Id, (Guid?)null)).ToList();

            var rules = await priceCampaignRepository.GetEffectivePriceItemsAsync(
                request.StoreId.Value, keys, at, cancellationToken);

            dtos = items.Select(p =>
            {
                var effectivePrice = rules.TryGetValue((p.Id, null), out var rule)
                    ? ComputePrice(p.Price, rule)
                    : p.Price;

                return new ProductDto(
                    p.Id, p.Name, p.Description, p.Price, p.OriginalPrice,
                    p.Slug, p.ImageUrl, p.IsActive, p.CategoryId, p.CreatedAt,
                    p.HasSizes, p.SizeType?.ToString(), effectivePrice);
            }).ToList();
        }
        else
        {
            dtos = items.Select(p => new ProductDto(
                p.Id, p.Name, p.Description, p.Price, p.OriginalPrice,
                p.Slug, p.ImageUrl, p.IsActive, p.CategoryId, p.CreatedAt,
                p.HasSizes, p.SizeType?.ToString())).ToList();
        }

        var result = new PagedResult<ProductDto>(dtos, totalCount, request.Page, request.PageSize);

        if (!isAdminQuery)
            await cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), cancellationToken);

        return result;
    }

    private static decimal ComputePrice(decimal basePrice, (int ruleType, decimal discountValue) rule) =>
        (PriceRuleType)rule.ruleType switch
        {
            PriceRuleType.Coefficient => basePrice * rule.discountValue,
            PriceRuleType.FixedPrice => rule.discountValue,
            _ => basePrice
        };
}
