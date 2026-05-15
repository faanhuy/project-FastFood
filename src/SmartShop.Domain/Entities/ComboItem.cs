using SmartShop.Domain.Common;

namespace SmartShop.Domain.Entities;

public class ComboItem : BaseEntity
{
    public Guid ComboId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public Guid? SizeId { get; private set; }
    public string? SizeLabel { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPriceSnapshot { get; private set; }

    public Combo? Combo { get; private set; }
    public Product? Product { get; private set; }

    private ComboItem() { }

    public static ComboItem Create(Guid comboId, Guid productId, string productName,
        Guid? sizeId, string? sizeLabel, int quantity, decimal unitPriceSnapshot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0.", nameof(quantity));

        if (unitPriceSnapshot < 0)
            throw new ArgumentException("UnitPriceSnapshot cannot be negative.", nameof(unitPriceSnapshot));

        return new ComboItem
        {
            ComboId = comboId,
            ProductId = productId,
            ProductName = productName,
            SizeId = sizeId,
            SizeLabel = sizeLabel,
            Quantity = quantity,
            UnitPriceSnapshot = unitPriceSnapshot
        };
    }
}
