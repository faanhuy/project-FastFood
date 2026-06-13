using SmartShop.Domain.Common;

namespace SmartShop.Domain.Entities;

public class OrderFlashSaleUsage : BaseAuditableEntity
{
    private OrderFlashSaleUsage() { }

    public Guid OrderId { get; private set; }
    public Guid FlashSaleId { get; private set; }
    public Guid FlashSaleItemId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? SizeId { get; private set; }
    public int Quantity { get; private set; }
    public decimal SalePrice { get; private set; }
    public decimal OriginalPrice { get; private set; }

    // Navigation
    public Order? Order { get; private set; }
    public FlashSale? FlashSale { get; private set; }
    public FlashSaleItem? FlashSaleItem { get; private set; }

    public static OrderFlashSaleUsage Create(
        Guid orderId, Guid flashSaleId, Guid flashSaleItemId,
        Guid productId, Guid? sizeId, int quantity,
        decimal salePrice, decimal originalPrice)
    {
        return new OrderFlashSaleUsage
        {
            OrderId = orderId,
            FlashSaleId = flashSaleId,
            FlashSaleItemId = flashSaleItemId,
            ProductId = productId,
            SizeId = sizeId,
            Quantity = quantity,
            SalePrice = salePrice,
            OriginalPrice = originalPrice
        };
    }
}
