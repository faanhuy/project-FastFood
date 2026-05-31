using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SmartShop.Domain.Interfaces;
using SmartShop.Infrastructure.Data;

namespace SmartShop.Infrastructure.Services;

public class LocalizationService(ApplicationDbContext db, IMemoryCache cache) : ILocalizationService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    public string GetMessage(string messageKey, string language, Dictionary<string, string>? parameters = null)
    {
        var cacheKey = $"loc:msg:{language}:{messageKey}";
        if (!cache.TryGetValue(cacheKey, out string? text))
        {
            text = db.LocalizedMessages
                .AsNoTracking()
                .Where(m => m.MessageKey == messageKey && m.Language == language)
                .Select(m => m.MessageText)
                .FirstOrDefault();

            // Fallback to VI if language not found
            if (text == null && language != "vi")
            {
                text = db.LocalizedMessages
                    .AsNoTracking()
                    .Where(m => m.MessageKey == messageKey && m.Language == "vi")
                    .Select(m => m.MessageText)
                    .FirstOrDefault();
            }

            text ??= messageKey;
            cache.Set(cacheKey, text, CacheTtl);
        }

        return FormatMessage(text!, parameters);
    }

    public string GetFieldName(string fieldKey, string language)
    {
        var cacheKey = $"loc:field:{language}:{fieldKey}";
        if (!cache.TryGetValue(cacheKey, out string? name))
        {
            name = db.LocalizedFieldNames
                .AsNoTracking()
                .Where(f => f.FieldKey == fieldKey && f.Language == language)
                .Select(f => f.DisplayName)
                .FirstOrDefault();

            // Fallback to VI if language not found
            if (name == null && language != "vi")
            {
                name = db.LocalizedFieldNames
                    .AsNoTracking()
                    .Where(f => f.FieldKey == fieldKey && f.Language == "vi")
                    .Select(f => f.DisplayName)
                    .FirstOrDefault();
            }

            name ??= fieldKey;
            cache.Set(cacheKey, name, CacheTtl);
        }

        return name!;
    }

    public string DetectLanguage(string? acceptLanguageHeader)
    {
        if (string.IsNullOrWhiteSpace(acceptLanguageHeader)) return "vi";

        var primary = acceptLanguageHeader
            .Split(',')[0]                          // Take first language preference
            .Trim()
            .Split('-')[0]                          // Take language code only (ignore region)
            .ToLower();

        return primary is "vi" or "en" ? primary : "vi";  // Default to VI
    }

    public string FormatMessage(string template, Dictionary<string, string>? parameters)
    {
        if (parameters == null || parameters.Count == 0) return template;

        var result = template;
        foreach (var (key, value) in parameters)
        {
            result = result.Replace($"{{{key}}}", value);
        }

        return result;
    }
}
