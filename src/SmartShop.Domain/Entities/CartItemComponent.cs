using SmartShop.Domain.Common;

namespace SmartShop.Domain.Entities;

public class CartItemComponent : BaseEntity
{
    public Guid CartItemId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public Guid? SizeId { get; private set; }
    public string? SizeLabel { get; private set; }
    public int QuantityPerCombo { get; private set; }
    public int TotalQuantity { get; private set; }
    public decimal UnitPriceSnapshot { get; private set; }

    public CartItem? CartItem { get; private set; }

    private CartItemComponent() { }

    public static CartItemComponent Create(
        Guid cartItemId, Guid productId, string productName,
        Guid? sizeId, string? sizeLabel,
        int quantityPerCombo, int comboQuantity, decimal unitPriceSnapshot)
    {
        return new CartItemComponent
        {
            CartItemId = cartItemId,
            ProductId = productId,
            ProductName = productName,
            SizeId = sizeId,
            SizeLabel = sizeLabel,
            QuantityPerCombo = quantityPerCombo,
            TotalQuantity = quantityPerCombo * comboQuantity,
            UnitPriceSnapshot = unitPriceSnapshot
        };
    }

    public void RecalculateTotalQuantity(int comboQuantity)
    {
        TotalQuantity = QuantityPerCombo * comboQuantity;
    }
}
