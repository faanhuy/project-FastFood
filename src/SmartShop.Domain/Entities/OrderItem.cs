using SmartShop.Domain.Common;
using SmartShop.Domain.Enums;

namespace SmartShop.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public CartItemType ItemType { get; private set; } = CartItemType.Product;

    public Guid? ProductId { get; private set; }
    public Guid? ComboId { get; private set; }

    public string ProductName { get; private set; } = string.Empty;
    public string? ImageUrl { get; private set; }

    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public Guid? SizeId { get; private set; }
    public string? SizeLabel { get; private set; }
    public decimal? OriginalUnitPrice { get; private set; }
    public Guid? PromotionalPriceId { get; private set; }

    public Order? Order { get; private set; }
    public Product? Product { get; private set; }

    private readonly List<OrderItemComponent> _components = [];
    public IReadOnlyCollection<OrderItemComponent> Components => _components.AsReadOnly();

    public decimal SubTotal => UnitPrice * Quantity;

    private OrderItem() { }

    public static OrderItem Create(
        Guid orderId, Guid productId, string productName, int quantity, decimal unitPrice,
        Guid? sizeId = null, string? sizeLabel = null,
        decimal? originalUnitPrice = null, Guid? promotionalPriceId = null,
        string? imageUrl = null)
    {
        return new OrderItem
        {
            OrderId = orderId,
            ItemType = CartItemType.Product,
            ProductId = productId,
            ProductName = productName,
            ImageUrl = imageUrl,
            Quantity = quantity,
            UnitPrice = unitPrice,
            SizeId = sizeId,
            SizeLabel = sizeLabel,
            OriginalUnitPrice = originalUnitPrice,
            PromotionalPriceId = promotionalPriceId
        };
    }

    public static OrderItem CreateCombo(
        Guid orderId, Guid comboId, string comboTitle, string? imageUrl,
        int quantity, decimal unitPrice, decimal originalUnitPrice)
    {
        return new OrderItem
        {
            OrderId = orderId,
            ItemType = CartItemType.Combo,
            ComboId = comboId,
            ProductName = comboTitle,
            ImageUrl = imageUrl,
            Quantity = quantity,
            UnitPrice = unitPrice,
            OriginalUnitPrice = originalUnitPrice
        };
    }

    public void AddComponent(OrderItemComponent component) => _components.Add(component);
}
