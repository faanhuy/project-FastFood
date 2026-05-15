using SmartShop.Domain.Common;

namespace SmartShop.Domain.Entities;

public class OrderItemComponent : BaseEntity
{
    public Guid OrderItemId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string? ProductImageUrl { get; private set; }
    public Guid? SizeId { get; private set; }
    public string? SizeLabel { get; private set; }
    public int QuantityPerCombo { get; private set; }
    public int TotalQuantity { get; private set; }
    public decimal UnitPriceSnapshot { get; private set; }

    public OrderItem? OrderItem { get; private set; }

    private OrderItemComponent() { }

    public static OrderItemComponent Create(
        Guid orderItemId, Guid productId, string productName, string? productImageUrl,
        Guid? sizeId, string? sizeLabel,
        int quantityPerCombo, int comboQuantity, decimal unitPriceSnapshot)
    {
        return new OrderItemComponent
        {
            OrderItemId = orderItemId,
            ProductId = productId,
            ProductName = productName,
            ProductImageUrl = productImageUrl,
            SizeId = sizeId,
            SizeLabel = sizeLabel,
            QuantityPerCombo = quantityPerCombo,
            TotalQuantity = quantityPerCombo * comboQuantity,
            UnitPriceSnapshot = unitPriceSnapshot
        };
    }
}
