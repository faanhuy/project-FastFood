using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using SmartShop.Domain.Interfaces;
using SmartShop.WebAPI.Services;
using Xunit;

namespace SmartShop.Application.Tests.Infrastructure;

public class CurrentLanguageServiceTests
{
    private static CurrentLanguageService CreateService(
        string? acceptLanguageHeader,
        string detectedLanguage = "vi")
    {
        var httpContext = new DefaultHttpContext();
        if (acceptLanguageHeader is not null)
            httpContext.Request.Headers["Accept-Language"] = acceptLanguageHeader;

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var localization = new Mock<ILocalizationService>();
        localization
            .Setup(x => x.DetectLanguage(It.IsAny<string?>()))
            .Returns(detectedLanguage);

        return new CurrentLanguageService(httpContextAccessor.Object, localization.Object);
    }

    [Fact]
    public void Language_WithViHeader_CallsDetectLanguageWithHeader()
    {
        var localization = new Mock<ILocalizationService>();
        localization.Setup(x => x.DetectLanguage("vi")).Returns("vi");

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Accept-Language"] = "vi";

        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(x => x.HttpContext).Returns(httpContext);

        var svc = new CurrentLanguageService(accessor.Object, localization.Object);

        var lang = svc.Language;

        lang.Should().Be("vi");
        localization.Verify(x => x.DetectLanguage("vi"), Times.Once);
    }

    [Fact]
    public void Language_WithEnHeader_CallsDetectLanguageWithHeader()
    {
        var localization = new Mock<ILocalizationService>();
        localization.Setup(x => x.DetectLanguage("en")).Returns("en");

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Accept-Language"] = "en";

        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(x => x.HttpContext).Returns(httpContext);

        var svc = new CurrentLanguageService(accessor.Object, localization.Object);

        svc.Language.Should().Be("en");
    }

    [Fact]
    public void Language_NoHeader_PassesNullToDetectLanguage()
    {
        var localization = new Mock<ILocalizationService>();
        localization.Setup(x => x.DetectLanguage(null)).Returns("vi");

        var httpContext = new DefaultHttpContext();

        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(x => x.HttpContext).Returns(httpContext);

        var svc = new CurrentLanguageService(accessor.Object, localization.Object);

        var lang = svc.Language;

        lang.Should().Be("vi");
        localization.Verify(x => x.DetectLanguage(null), Times.Once);
    }

    [Fact]
    public void Language_NoHttpContext_PassesNullToDetectLanguage()
    {
        var localization = new Mock<ILocalizationService>();
        localization.Setup(x => x.DetectLanguage(null)).Returns("vi");

        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var svc = new CurrentLanguageService(accessor.Object, localization.Object);

        var lang = svc.Language;

        lang.Should().Be("vi");
        localization.Verify(x => x.DetectLanguage(null), Times.Once);
    }
}
