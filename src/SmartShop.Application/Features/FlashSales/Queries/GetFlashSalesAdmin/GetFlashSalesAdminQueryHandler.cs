using MediatR;
using SmartShop.Application.Features.FlashSales;
using SmartShop.Application.Features.FlashSales.Queries.GetFlashSalesAdmin;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.FlashSales.Queries.GetFlashSalesAdmin;

public class GetFlashSalesAdminQueryHandler(
    IFlashSaleRepository flashSaleRepository,
    IProductRepository productRepository) : IRequestHandler<GetFlashSalesAdminQuery, SmartShop.Domain.Interfaces.PagedResult<FlashSaleDto>>
{
    public async Task<PagedResult<FlashSaleDto>> Handle(GetFlashSalesAdminQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await flashSaleRepository.GetPagedAsync(
            request.Page, request.PageSize, request.IsActive, request.Status, cancellationToken);

        // Collect all product IDs from items
        var productIds = items
            .SelectMany(fs => fs.Items)
            .Select(i => i.ProductId)
            .Distinct()
            .ToList();

        var products = productIds.Count > 0
            ? (await productRepository.GetByIdsAsync(productIds, cancellationToken))
                .ToDictionary(p => p.Id)
            : new Dictionary<Guid, Product>();

        var dtos = items
            .Select(fs => MapToDto(fs, products, DateTime.UtcNow))
            .ToList();

        return new PagedResult<FlashSaleDto>(dtos, totalCount, request.Page, request.PageSize);
    }

    private static FlashSaleDto MapToDto(FlashSale fs, Dictionary<Guid, Product> productDict, DateTime now)
    {
        var remaining = (int)Math.Max(0, (fs.EndAt - now).TotalSeconds);

        var itemDtos = fs.Items.Select(item =>
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
        }).ToList();

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
