using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Features.FlashSales;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.FlashSales.Commands.CreateFlashSale;

public class CreateFlashSaleCommandHandler(
    IProductRepository productRepository,
    IFlashSaleRepository flashSaleRepository,
    IUnitOfWork unitOfWork,
    ICacheService cache) : IRequestHandler<CreateFlashSaleCommand, FlashSaleDto>
{
    public async Task<FlashSaleDto> Handle(CreateFlashSaleCommand request, CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0)
            throw new ConflictException("error.flashsale_empty_items", null);

        // Load all products
        var productIds = request.Items.Select(x => x.ProductId).Distinct().ToList();
        var products = await productRepository.GetByIdsAsync(productIds, cancellationToken);
        var productDict = products.ToDictionary(p => p.Id);

        // Validate and create items
        var items = new List<FlashSaleItem>();

        foreach (var itemRequest in request.Items)
        {
            if (!productDict.TryGetValue(itemRequest.ProductId, out var product))
                throw new NotFoundException(nameof(Product), itemRequest.ProductId);

            if (!product.IsActive)
                throw new ConflictException("error.flashsale_product_inactive",
                    new Dictionary<string, string> { ["product"] = product.Name });

            // Validate sale price
            if (itemRequest.SalePrice <= 0 || itemRequest.SalePrice >= product.Price)
                throw new ConflictException("error.flashsale_saleprice_invalid",
                    new Dictionary<string, string> { ["product"] = product.Name });

            // Validate size if provided
            if (itemRequest.SizeId.HasValue)
            {
                if (!product.HasSizes)
                    throw new ConflictException("error.flashsale_product_no_sizes",
                        new Dictionary<string, string> { ["product"] = product.Name });

                var size = product.Sizes.FirstOrDefault(s => s.Id == itemRequest.SizeId);
                if (size == null || !size.IsActive)
                    throw new ConflictException("error.flashsale_size_invalid",
                        new Dictionary<string, string> { ["product"] = product.Name });
            }

            var item = FlashSaleItem.Create(
                Guid.Empty, // Will be set when FlashSale is created
                itemRequest.ProductId,
                itemRequest.SizeId,
                itemRequest.SalePrice,
                product.Price,
                itemRequest.StockLimit);

            items.Add(item);
        }

        // Check for duplicate items (same product + size combination)
        var duplicates = items
            .GroupBy(x => new { x.ProductId, x.SizeId })
            .Where(g => g.Count() > 1)
            .ToList();

        if (duplicates.Any())
            throw new ConflictException("error.flashsale_duplicate_items", null);

        var hasOverlappingItems = await flashSaleRepository.HasOverlappingFlashSaleItemsAsync(
            items.Select(x => (x.ProductId, x.SizeId)),
            request.StartAt,
            request.EndAt,
            ct: cancellationToken);

        if (hasOverlappingItems)
            throw new ConflictException("error.flashsale_item_time_overlap", null);

        var flashSale = FlashSale.Create(request.Name, request.StartAt, request.EndAt, items);

        await flashSaleRepository.AddAsync(flashSale, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cache.RemoveByPrefixAsync("flashsales:active:", cancellationToken);

        return MapToDto(flashSale, productDict, DateTime.UtcNow);
    }

    private static FlashSaleDto MapToDto(FlashSale fs, Dictionary<Guid, Product> productDict, DateTime now)
    {
        var remaining = (int)Math.Max(0, (fs.EndAt - now).TotalSeconds);

        var itemDtos = fs.Items.Select(item =>
        {
#pragma warning disable CS8629
            productDict.TryGetValue(item.ProductId, out var product);
#pragma warning restore CS8629
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
            RemainingSeconds = remaining,
            Items = itemDtos
        };
    }
}
