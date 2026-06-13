using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Features.FlashSales;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.FlashSales.Commands.UpdateFlashSale;

public class UpdateFlashSaleCommandHandler(
    IProductRepository productRepository,
    IFlashSaleRepository flashSaleRepository,
    IOrderFlashSaleUsageRepository orderFlashSaleUsageRepository,
    IUnitOfWork unitOfWork,
    ICacheService cache) : IRequestHandler<UpdateFlashSaleCommand, FlashSaleDto>
{
    public async Task<FlashSaleDto> Handle(UpdateFlashSaleCommand request, CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0)
            throw new ConflictException("error.flashsale_empty_items", null);

        var flashSale = await flashSaleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(FlashSale), request.Id);

        // Chỉ cho edit khi Draft hoặc Rejected
        if (flashSale.Status != FlashSaleStatus.Draft && flashSale.Status != FlashSaleStatus.Rejected)
            throw new ConflictException("error.flashsale_locked_for_editing", null);

        // Kiểm tra đã từng có OrderFlashSaleUsage chưa (đã dùng rồi thì không cho sửa)
        var usages = await orderFlashSaleUsageRepository.GetByFlashSaleIdAsync(flashSale.Id, cancellationToken);
        if (usages.Any())
            throw new ConflictException("error.flashsale_has_orders", null);

        // Load all products
        var productIds = request.Items.Select(x => x.ProductId).Distinct().ToList();
        var products = await productRepository.GetByIdsAsync(productIds, cancellationToken);
        var productDict = products.ToDictionary(p => p.Id);

        // Validate and create new items
        var newItems = new List<FlashSaleItem>();

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
                flashSale.Id,
                itemRequest.ProductId,
                itemRequest.SizeId,
                itemRequest.SalePrice,
                product.Price,
                itemRequest.StockLimit);

            newItems.Add(item);
        }

        // Check for duplicate items (same product + size combination)
        var duplicates = newItems
            .GroupBy(x => new { x.ProductId, x.SizeId })
            .Where(g => g.Count() > 1)
            .ToList();

        if (duplicates.Any())
            throw new ConflictException("error.flashsale_duplicate_items", null);

        // Update flash sale metadata
        flashSale.Update(request.Name, request.StartAt, request.EndAt);

        // Delete old items
        await flashSaleRepository.RemoveItemsByFlashSaleIdAsync(request.Id, cancellationToken);

        // Update flash sale
        flashSaleRepository.Update(flashSale);

        // Add new items
        foreach (var newItem in newItems)
        {
            await flashSaleRepository.AddItemAsync(newItem, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload to get the updated items
        flashSale = await flashSaleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(FlashSale), request.Id);

        await cache.RemoveByPrefixAsync("flashsales:active:", cancellationToken);

        return MapToDto(flashSale, productDict, DateTime.UtcNow);
    }

    private static FlashSaleDto MapToDto(FlashSale fs, Dictionary<Guid, Product> productDict, DateTime now)
    {
        var remaining = (int)Math.Max(0, (fs.EndAt - now).TotalSeconds);

        var itemDtos = fs.Items.Select(item =>
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
