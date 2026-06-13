using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Features.FlashSales;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.FlashSales.Commands.SubmitFlashSaleForApproval;

public class SubmitFlashSaleForApprovalCommandHandler(
    IFlashSaleRepository flashSaleRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ICacheService cache) : IRequestHandler<SubmitFlashSaleForApprovalCommand, FlashSaleDto>
{
    public async Task<FlashSaleDto> Handle(SubmitFlashSaleForApprovalCommand request, CancellationToken cancellationToken)
    {
        var flashSale = await flashSaleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(FlashSale), request.Id);

        flashSale.SubmitForApproval();

        flashSaleRepository.Update(flashSale);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload to get updated data
        flashSale = await flashSaleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(FlashSale), request.Id);

        // Collect all product IDs from items
        var productIds = flashSale.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = productIds.Count > 0
            ? (await productRepository.GetByIdsAsync(productIds, cancellationToken)).ToDictionary(p => p.Id)
            : new Dictionary<Guid, Product>();

        await cache.RemoveByPrefixAsync("flashsales:active:", cancellationToken);

        return MapToDto(flashSale, products, DateTime.UtcNow);
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
