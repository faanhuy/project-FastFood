using SmartShop.Domain.Common;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Enums;

namespace SmartShop.Domain.Entities;

public class CartItem : BaseEntity
{
    public Guid CartId { get; private set; }
    public CartItemType ItemType { get; private set; } = CartItemType.Product;

    public Guid? ProductId { get; private set; }
    public Guid? ComboId { get; private set; }

    public string DisplayName { get; private set; } = string.Empty;
    public string? ImageUrl { get; private set; }

    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    public Guid? SizeId { get; private set; }
    public string? SizeLabel { get; private set; }

    public Cart? Cart { get; private set; }
    public Product? Product { get; private set; }

    private readonly List<CartItemComponent> _components = [];
    public IReadOnlyCollection<CartItemComponent> Components => _components.AsReadOnly();

    public decimal SubTotal => UnitPrice * Quantity;

    private CartItem() { }

    public static CartItem CreateProduct(
        Guid cartId, Guid productId, string displayName, string? imageUrl,
        int quantity, decimal unitPrice, Guid? sizeId = null, string? sizeLabel = null)
    {
        return new CartItem
        {
            CartId = cartId,
            ItemType = CartItemType.Product,
            ProductId = productId,
            DisplayName = displayName,
            ImageUrl = imageUrl,
            Quantity = quantity,
            UnitPrice = unitPrice,
            SizeId = sizeId,
            SizeLabel = sizeLabel
        };
    }

    public static CartItem CreateCombo(
        Guid cartId, Guid comboId, string displayName, string? imageUrl,
        int quantity, decimal unitPrice)
    {
        return new CartItem
        {
            CartId = cartId,
            ItemType = CartItemType.Combo,
            ComboId = comboId,
            DisplayName = displayName,
            ImageUrl = imageUrl,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }

    public void AddComponent(CartItemComponent component) => _components.Add(component);

    public void IncreaseQuantity(int amount) => Quantity += amount;

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ConflictException("validation.quantity_positive", null);
        Quantity = quantity;
        foreach (var c in _components)
            c.RecalculateTotalQuantity(quantity);
    }
}
