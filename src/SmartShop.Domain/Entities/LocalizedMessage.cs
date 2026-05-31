using SmartShop.Domain.Common;

namespace SmartShop.Domain.Entities;

/// <summary>
/// Stores localized messages/error messages for VI/EN languages.
/// Example: error.not_found, error.conflict, validation.required, etc.
/// </summary>
public class LocalizedMessage : BaseAuditableEntity
{
    private LocalizedMessage() { }

    public string MessageKey { get; private set; } = string.Empty;
    public string Language { get; private set; } = string.Empty;
    public string MessageText { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;

    public static LocalizedMessage Create(string messageKey, string language, string messageText, string category = "error")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(language);
        ArgumentException.ThrowIfNullOrWhiteSpace(messageText);
        return new LocalizedMessage
        {
            MessageKey = messageKey,
            Language = language,
            MessageText = messageText,
            Category = category
        };
    }
}
