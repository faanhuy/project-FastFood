using MediatR;
using SmartShop.Application.Features.FlashSales;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.FlashSales.Queries.GetUpcomingFlashSales;

public class GetUpcomingFlashSalesQueryHandler(
    IFlashSaleRepository flashSaleRepository,
    IProductRepository productRepository) : IRequestHandler<GetUpcomingFlashSalesQuery, PagedResult<FlashSaleDto>>
{
    public async Task<PagedResult<FlashSaleDto>> Handle(GetUpcomingFlashSalesQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var upcomingSales = await flashSaleRepository.GetUpcomingFlashSalesAsync(now, request.WithinDays, cancellationToken);

        var productIds = upcomingSales
            .SelectMany(fs => fs.Items)
            .Select(i => i.ProductId)
            .Distinct()
            .ToList();

        var products = productIds.Count > 0
            ? (await productRepository.GetByIdsAsync(productIds, cancellationToken))
                .ToDictionary(p => p.Id)
            : new Dictionary<Guid, Product>();

        var dtos = upcomingSales
            .Select(fs => MapToDto(fs, products, now))
            .ToList();

        var total = dtos.Count;
        var pagedDtos = dtos
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PagedResult<FlashSaleDto>(pagedDtos, total, request.Page, request.PageSize);
    }

    private static FlashSaleDto MapToDto(FlashSale fs, Dictionary<Guid, Product> productDict, DateTime now)
    {
        var secondsUntilStart = (int)Math.Max(0, (fs.StartAt - now).TotalSeconds);

        var itemDtos = fs.Items
            .Select(item =>
            {
                productDict.TryGetValue(item.ProductId, out var product);
                var sizeLabel = item.SizeId.HasValue
                    ? product?.Sizes.FirstOrDefault(s => s.Id == item.SizeId)?.SizeLabel
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
                    SoldCount = 0,
                    RemainingStock = item.StockLimit,
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
            IsActive = false,
            Status = fs.Status.ToString(),
            RemainingSeconds = secondsUntilStart,
            Items = itemDtos
        };
    }
}
