namespace SmartShop.Domain.Interfaces;

/// <summary>
/// Service for retrieving and formatting localized messages and field names.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Get a localized message by key and language.
    /// Falls back to VI if the requested language is not found.
    /// Optionally formats message with parameter substitution.
    /// </summary>
    /// <param name="messageKey">Message key (e.g., "error.not_found")</param>
    /// <param name="language">Language code (e.g., "vi", "en")</param>
    /// <param name="parameters">Optional parameters for message formatting</param>
    /// <returns>Localized message text</returns>
    string GetMessage(string messageKey, string language, Dictionary<string, string>? parameters = null);

    /// <summary>
    /// Get a localized field name by key and language.
    /// Falls back to VI if the requested language is not found.
    /// </summary>
    /// <param name="fieldKey">Field key (e.g., "user.email")</param>
    /// <param name="language">Language code (e.g., "vi", "en")</param>
    /// <returns>Localized field display name</returns>
    string GetFieldName(string fieldKey, string language);

    /// <summary>
    /// Detect language from Accept-Language HTTP header.
    /// Supports "vi" and "en". Defaults to "vi" if not specified.
    /// </summary>
    /// <param name="acceptLanguageHeader">Accept-Language header value</param>
    /// <returns>Detected language code ("vi" or "en")</returns>
    string DetectLanguage(string? acceptLanguageHeader);

    /// <summary>
    /// Format message template with parameter substitution.
    /// Example: "Hello {name}" with {"name": "John"} -> "Hello John"
    /// </summary>
    /// <param name="template">Message template with {key} placeholders</param>
    /// <param name="parameters">Dictionary of {key: value} for substitution</param>
    /// <returns>Formatted message</returns>
    string FormatMessage(string template, Dictionary<string, string>? parameters);
}
