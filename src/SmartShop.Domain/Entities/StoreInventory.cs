using SmartShop.Domain.Common;
using SmartShop.Domain.Common.Exceptions;

namespace SmartShop.Domain.Entities;

public class StoreInventory : BaseAuditableEntity
{
    public Guid StoreId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public int LowStockThreshold { get; private set; } = 5;
    public byte[]? RowVersion { get; private set; }

    public Store? Store { get; private set; }
    public Product? Product { get; private set; }

    private StoreInventory() { }

    public static StoreInventory Create(Guid storeId, Guid productId, int quantity)
    {
        if (storeId == Guid.Empty)
            throw new ConflictException("validation.store_id_invalid", null);
        if (productId == Guid.Empty)
            throw new ConflictException("validation.product_id_invalid", null);
        if (quantity < 0)
            throw new ConflictException("validation.inventory_non_negative", null);

        return new StoreInventory
        {
            StoreId = storeId,
            ProductId = productId,
            Quantity = quantity,
            LowStockThreshold = 5
        };
    }

    public void DeductStock(int quantity)
    {
        if (quantity <= 0)
            throw new ConflictException("validation.quantity_positive", null);
        if (quantity > Quantity)
            throw new ConflictException("error.inventory_insufficient_stock",
                new Dictionary<string, string> { ["available"] = Quantity.ToString(), ["required"] = quantity.ToString() });

        Quantity -= quantity;
    }

    public void RestoreStock(int quantity)
    {
        if (quantity <= 0)
            throw new ConflictException("validation.quantity_positive", null);

        Quantity += quantity;
    }

    public void SetQuantity(int quantity)
    {
        if (quantity < 0)
            throw new ConflictException("validation.inventory_non_negative", null);

        Quantity = quantity;
    }
}
