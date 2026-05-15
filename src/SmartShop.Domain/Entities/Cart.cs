using SmartShop.Domain.Common;
using SmartShop.Domain.Enums;

namespace SmartShop.Domain.Entities;

public class Cart : BaseAuditableEntity
{
    public Guid UserId { get; private set; }

    public User? User { get; private set; }

    private readonly List<CartItem> _items = [];
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    public decimal TotalAmount => _items.Sum(i => i.UnitPrice * i.Quantity);

    private Cart() { }

    public static Cart Create(Guid userId)
    {
        return new Cart { UserId = userId };
    }

    public void AddItem(Guid productId, string displayName, string? imageUrl,
        int quantity, decimal unitPrice, Guid? sizeId = null, string? sizeLabel = null)
    {
        var existing = _items.FirstOrDefault(i =>
            i.ItemType == CartItemType.Product && i.ProductId == productId && i.SizeId == sizeId);
        if (existing != null)
            existing.IncreaseQuantity(quantity);
        else
            _items.Add(CartItem.CreateProduct(Id, productId, displayName, imageUrl,
                quantity, unitPrice, sizeId, sizeLabel));
    }

    public void AddComboItem(Guid comboId, string displayName, string? imageUrl,
        int quantity, decimal unitPrice,
        IEnumerable<CartItemComponent> components)
    {
        var existing = _items.FirstOrDefault(i =>
            i.ItemType == CartItemType.Combo && i.ComboId == comboId);
        if (existing != null)
        {
            existing.IncreaseQuantity(quantity);
        }
        else
        {
            var item = CartItem.CreateCombo(Id, comboId, displayName, imageUrl, quantity, unitPrice);
            foreach (var c in components)
                item.AddComponent(c);
            _items.Add(item);
        }
    }

    public void RemoveItem(Guid productId, Guid? sizeId = null)
    {
        var item = _items.FirstOrDefault(i =>
            i.ItemType == CartItemType.Product && i.ProductId == productId && i.SizeId == sizeId);
        if (item != null)
            _items.Remove(item);
    }

    public void RemoveItemById(Guid cartItemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == cartItemId);
        if (item != null)
            _items.Remove(item);
    }

    public void UpdateItemQuantity(Guid productId, int quantity, Guid? sizeId = null)
    {
        var item = _items.FirstOrDefault(i =>
                i.ItemType == CartItemType.Product && i.ProductId == productId && i.SizeId == sizeId)
            ?? throw new InvalidOperationException("Sản phẩm không có trong giỏ hàng.");
        item.UpdateQuantity(quantity);
    }

    public void UpdateItemQuantityById(Guid cartItemId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.Id == cartItemId)
            ?? throw new InvalidOperationException("Dòng giỏ hàng không tồn tại.");
        item.UpdateQuantity(quantity);
    }

    public void Clear()
    {
        _items.Clear();
    }
}
