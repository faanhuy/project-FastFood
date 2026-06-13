using SmartShop.Domain.Common;
using SmartShop.Domain.Common.Exceptions;

namespace SmartShop.Domain.Entities;

public class FlashSaleItem : BaseAuditableEntity
{
    private FlashSaleItem() { }

    public Guid FlashSaleId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? SizeId { get; private set; }
    public decimal SalePrice { get; private set; }
    public decimal OriginalPrice { get; private set; }
    public int StockLimit { get; private set; }
    public int SoldCount { get; private set; }

    public FlashSale? FlashSale { get; private set; }
    public Product? Product { get; private set; }
    public ProductSize? ProductSize { get; private set; }

    public static FlashSaleItem Create(Guid flashSaleId, Guid productId, Guid? sizeId,
        decimal salePrice, decimal originalPrice, int stockLimit)
    {
        if (salePrice <= 0 || salePrice >= originalPrice)
            throw new ConflictException("Sale price must be positive and less than original price.");
        if (stockLimit <= 0)
            throw new ConflictException("Stock limit must be positive.");

        return new FlashSaleItem
        {
            FlashSaleId = flashSaleId,
            ProductId = productId,
            SizeId = sizeId,
            SalePrice = salePrice,
            OriginalPrice = originalPrice,
            StockLimit = stockLimit,
            SoldCount = 0
        };
    }

    public bool HasStock(int quantity = 1) => SoldCount + quantity <= StockLimit;
    public int GetRemainingStock() => Math.Max(0, StockLimit - SoldCount);
    public void IncrementSoldCount(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.");
        SoldCount += quantity;
    }
}
