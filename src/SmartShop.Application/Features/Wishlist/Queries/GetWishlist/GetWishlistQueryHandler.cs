using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Common.Models;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Wishlist.Queries.GetWishlist;

public class GetWishlistQueryHandler(
    IWishlistRepository wishlistRepository,
    ICurrentUserService currentUserService,
    IPriceCampaignRepository priceCampaignRepository) : IRequestHandler<GetWishlistQuery, ApiResponse<List<WishlistItemDto>>>
{
    public async Task<ApiResponse<List<WishlistItemDto>>> Handle(GetWishlistQuery request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(currentUserService.UserId);
        var items = (await wishlistRepository.GetByUserIdAsync(userId, cancellationToken)).ToList();

        Dictionary<(Guid, Guid?), (int, decimal)> priceRules = [];
        if (request.StoreId.HasValue && items.Any())
        {
            var keys = items
                .Where(i => i.Product != null)
                .Select(i => (i.ProductId, (Guid?)null))
                .ToList();
            priceRules = await priceCampaignRepository.GetEffectivePriceItemsAsync(
                request.StoreId.Value, keys, DateTime.UtcNow, cancellationToken);
        }

        var dtos = items.Select(i =>
        {
            var basePrice = i.Product?.Price ?? 0;
            decimal? effectivePrice = null;
            if (priceRules.TryGetValue((i.ProductId, null), out var rule))
            {
                var computed = (PriceRuleType)rule.Item1 switch
                {
                    PriceRuleType.Coefficient => basePrice * rule.Item2,
                    PriceRuleType.FixedPrice  => rule.Item2,
                    _                         => basePrice
                };
                if (computed < basePrice) effectivePrice = computed;
            }

            return new WishlistItemDto(
                ProductId: i.ProductId,
                ProductName: i.Product?.Name ?? string.Empty,
                Price: basePrice,
                EffectivePrice: effectivePrice,
                ImageUrl: i.Product?.ImageUrl,
                IsInStock: i.Product?.IsActive ?? false,
                Slug: i.Product?.Slug ?? string.Empty
            );
        }).ToList();

        return ApiResponse<List<WishlistItemDto>>.Ok(dtos);
    }
}
