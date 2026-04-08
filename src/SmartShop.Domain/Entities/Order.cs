using SmartShop.Domain.Common;
using SmartShop.Domain.Enums;

namespace SmartShop.Domain.Entities;

public class Order : BaseAuditableEntity
{
    public Guid UserId { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; private set; }
    public string ShippingAddress { get; private set; } = string.Empty;
    public string? Notes { get; private set; }

    public User? User { get; private set; }

    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    public static Order Create(Guid userId, string shippingAddress, string? notes = null)
    {
        return new Order
        {
            UserId = userId,
            ShippingAddress = shippingAddress,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddItem(OrderItem item)
    {
        _items.Add(item);
        RecalculateTotal();
    }

    private void RecalculateTotal()
    {
        TotalAmount = _items.Sum(i => i.UnitPrice * i.Quantity);
    }

    public void UpdateStatus(OrderStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Shipped || Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Không thể hủy đơn hàng đã giao.");

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}
