using SmartShop.Domain.Common;
using SmartShop.Domain.Enums;

namespace SmartShop.Domain.Entities;

public class OrderStatusHistory : BaseAuditableEntity
{
    public Guid OrderId { get; private set; }
    public OrderStatus FromStatus { get; private set; }
    public OrderStatus ToStatus { get; private set; }
    public Guid? ChangedBy { get; private set; }
    public string? Reason { get; private set; }
    public DateTime ChangedAt { get; private set; }

    private OrderStatusHistory() { }

    public static OrderStatusHistory Create(
        Guid orderId,
        OrderStatus fromStatus,
        OrderStatus toStatus,
        Guid? changedBy = null,
        string? reason = null)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId không được để trống.", nameof(orderId));

        return new OrderStatusHistory
        {
            OrderId = orderId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ChangedBy = changedBy,
            Reason = reason,
            ChangedAt = DateTime.UtcNow
        };
    }
}
