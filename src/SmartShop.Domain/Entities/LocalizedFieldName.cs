using SmartShop.Domain.Common;

namespace SmartShop.Domain.Entities;

/// <summary>
/// Stores localized field names/display names for validation error messages.
/// Example: user.email, product.name, order.address, etc.
/// </summary>
public class LocalizedFieldName : BaseAuditableEntity
{
    private LocalizedFieldName() { }

    public string FieldKey { get; private set; } = string.Empty;
    public string Language { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;

    public static LocalizedFieldName Create(string fieldKey, string language, string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(language);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        return new LocalizedFieldName { FieldKey = fieldKey, Language = language, DisplayName = displayName };
    }
}
