using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Features.FlashSales;
using SmartShop.Application.Features.FlashSales.Queries.GetActiveFlashSales;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.FlashSales.Queries.GetActiveFlashSales;

public class GetActiveFlashSalesQueryHandler(
    IFlashSaleRepository flashSaleRepository,
    IProductRepository productRepository,
    ICacheService cache) : IRequestHandler<GetActiveFlashSalesQuery, SmartShop.Domain.Interfaces.PagedResult<FlashSaleDto>>
{
    public async Task<PagedResult<FlashSaleDto>> Handle(GetActiveFlashSalesQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"flashsales:active:p{request.Page}:ps{request.PageSize}";
        var cached = await cache.GetAsync<PagedResult<FlashSaleDto>>(cacheKey, cancellationToken);
        if (cached is not null) return cached;

        var now = DateTime.UtcNow;
        var activeFlashSales = await flashSaleRepository.GetActiveFlashSalesAsync(now, cancellationToken);

        // Filter only Approved sales with items that have stock
        activeFlashSales = activeFlashSales
            .Where(fs => fs.Status == FlashSaleStatus.Approved && fs.Items.Any(i => i.HasStock()))
            .ToList();

        // Collect all product IDs from items
        var productIds = activeFlashSales
            .SelectMany(fs => fs.Items)
            .Select(i => i.ProductId)
            .Distinct()
            .ToList();

        var products = productIds.Count > 0
            ? (await productRepository.GetByIdsAsync(productIds, cancellationToken))
                .ToDictionary(p => p.Id)
            : new Dictionary<Guid, Product>();

        var dtos = activeFlashSales
            .Select(fs => MapToDto(fs, products, now))
            .ToList();

        var total = dtos.Count;
        var pagedDtos = dtos
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var result = new PagedResult<FlashSaleDto>(pagedDtos, total, request.Page, request.PageSize);

        // Cache for 30 seconds (flash sales are time-sensitive)
        await cache.SetAsync(cacheKey, result, TimeSpan.FromSeconds(30), cancellationToken);

        return result;
    }

    private static FlashSaleDto MapToDto(FlashSale fs, Dictionary<Guid, Product> productDict, DateTime now)
    {
        var remaining = (int)Math.Max(0, (fs.EndAt - now).TotalSeconds);

        var itemDtos = fs.Items
            .Where(i => i.HasStock())
            .Select(item =>
            {
                productDict.TryGetValue(item.ProductId, out var product);
                var sizeLabel = item.SizeId.HasValue
                    ? product?.Sizes.FirstOrDefault(s => s.SizeId == item.SizeId)?.SizeLabel
                    : null;

                var pct = item.OriginalPrice > 0
                    ? Math.Round((item.OriginalPrice - item.SalePrice) / item.OriginalPrice * 100, 1)
                    : 0;

                return new FlashSaleItemDto
                {
                    FlashSaleId = fs.Id,
                    ProductId = item.ProductId,
                    ProductName = product?.Name ?? string.Empty,
                    ImageUrl = product?.ImageUrl,
                    SizeId = item.SizeId,
                    SizeLabel = sizeLabel,
                    SalePrice = item.SalePrice,
                    OriginalPrice = item.OriginalPrice,
                    StockLimit = item.StockLimit,
                    SoldCount = item.SoldCount,
                    RemainingStock = item.GetRemainingStock(),
                    PercentDiscount = pct
                };
            })
            .ToList();

        return new FlashSaleDto
        {
            Id = fs.Id,
            Name = fs.Name,
            StartAt = fs.StartAt,
            EndAt = fs.EndAt,
            IsActive = fs.IsActive,
            Status = fs.Status.ToString(),
            ApprovedBy = fs.ApprovedBy,
            ApprovedAt = fs.ApprovedAt,
            RejectedReason = fs.RejectedReason,
            RemainingSeconds = remaining,
            Items = itemDtos
        };
    }
}
