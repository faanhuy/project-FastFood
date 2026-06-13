using SmartShop.Domain.Common;

namespace SmartShop.Domain.Entities;

public class Notification : BaseAuditableEntity
{
    public Guid UserId { get; private set; }

    /// <summary>i18n key cho tiêu đề — FE dịch theo locale người dùng.</summary>
    public string TitleKey { get; private set; } = string.Empty;

    /// <summary>i18n key cho nội dung — FE dịch theo locale người dùng.</summary>
    public string MessageKey { get; private set; } = string.Empty;

    /// <summary>Tham số nội suy dạng JSON, vd {"orderCode":"AB12CD34","status":"Confirmed"}.</summary>
    public string? Params { get; private set; }

    /// <summary>Legacy: text cũ trước khi chuyển sang key-based. Null với notification mới.</summary>
    public string? Title { get; private set; }

    /// <summary>Legacy: text cũ trước khi chuyển sang key-based. Null với notification mới.</summary>
    public string? Message { get; private set; }

    public bool IsRead { get; private set; }
    public Guid? OrderId { get; private set; }

    private Notification() { }

    public static Notification Create(
        Guid userId,
        string titleKey,
        string messageKey,
        string? paramsJson = null,
        Guid? orderId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(titleKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(messageKey);
        return new Notification
        {
            UserId = userId,
            TitleKey = titleKey,
            MessageKey = messageKey,
            Params = paramsJson,
            IsRead = false,
            OrderId = orderId
        };
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
