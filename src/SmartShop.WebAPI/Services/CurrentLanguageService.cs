using SmartShop.Application.Common.Interfaces;
using SmartShop.Domain.Interfaces;

namespace SmartShop.WebAPI.Services;

public class CurrentLanguageService(
    IHttpContextAccessor httpContextAccessor,
    ILocalizationService localization) : ICurrentLanguageService
{
    public string Language
    {
        get
        {
            var header = httpContextAccessor.HttpContext?
                .Request.Headers["Accept-Language"].FirstOrDefault();
            return localization.DetectLanguage(header);
        }
    }
}
