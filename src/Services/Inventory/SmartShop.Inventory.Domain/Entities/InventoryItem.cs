using SmartShop.Inventory.Domain.Common;
using SmartShop.Inventory.Domain.Common.Exceptions;

namespace SmartShop.Inventory.Domain.Entities;

public class InventoryItem : BaseAuditableEntity
{
    private InventoryItem() { }

    public Guid ProductId { get; private set; }
    public Guid StoreId { get; private set; }
    public Guid? SizeId { get; private set; }            // null = sản phẩm không có size
    public string Sku { get; private set; } = string.Empty;
    public int QuantityAvailable { get; private set; }   // tồn vật lý
    public int QuantityReserved { get; private set; }    // đang giữ cho đơn
    public int LowStockThreshold { get; private set; }

    // Số lượng còn có thể đặt = tồn vật lý - đang giữ chỗ
    public int Sellable => QuantityAvailable - QuantityReserved;
    public bool IsLowStock => Sellable <= LowStockThreshold;

    public static InventoryItem Create(Guid productId, Guid storeId, Guid? sizeId, string sku, int quantityAvailable, int lowStockThreshold = 5)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId is required.", nameof(productId));
        if (storeId == Guid.Empty)
            throw new ArgumentException("StoreId is required.", nameof(storeId));
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentOutOfRangeException.ThrowIfNegative(quantityAvailable);
        ArgumentOutOfRangeException.ThrowIfNegative(lowStockThreshold);

        return new InventoryItem
        {
            ProductId = productId,
            StoreId = storeId,
            SizeId = sizeId,
            Sku = sku,
            QuantityAvailable = quantityAvailable,
            LowStockThreshold = lowStockThreshold
        };
    }

    // Giữ chỗ: chưa trừ tồn vật lý, chỉ tăng QuantityReserved
    public void Reserve(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        if (quantity > Sellable)
            throw new ConflictException("error.inventory_insufficient_stock",
                new Dictionary<string, string>
                {
                    ["available"] = Sellable.ToString(),
                    ["required"] = quantity.ToString()
                });

        QuantityReserved += quantity;
    }

    // Trả chỗ đã giữ (đơn bị hủy / thanh toán thất bại)
    public void Release(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        if (quantity > QuantityReserved)
            throw new ConflictException("error.inventory_release_exceeds_reserved",
                new Dictionary<string, string>
                {
                    ["reserved"] = QuantityReserved.ToString(),
                    ["required"] = quantity.ToString()
                });

        QuantityReserved -= quantity;
    }

    // Xác nhận xuất kho: hàng thật sự rời kho nên giảm cả tồn vật lý lẫn phần đang giữ
    public void Confirm(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        if (quantity > QuantityReserved)
            throw new ConflictException("error.inventory_release_exceeds_reserved",
                new Dictionary<string, string>
                {
                    ["reserved"] = QuantityReserved.ToString(),
                    ["required"] = quantity.ToString()
                });

        QuantityReserved -= quantity;
        QuantityAvailable -= quantity;
    }
}
