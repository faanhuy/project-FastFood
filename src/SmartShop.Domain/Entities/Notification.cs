using SmartShop.Domain.Common;

namespace SmartShop.Domain.Entities;

public class Notification : BaseAuditableEntity
{
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }
    public Guid? OrderId { get; private set; }

    private Notification() { }

    public static Notification Create(Guid userId, string title, string message, Guid? orderId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        return new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            IsRead = false,
            OrderId = orderId
        };
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
