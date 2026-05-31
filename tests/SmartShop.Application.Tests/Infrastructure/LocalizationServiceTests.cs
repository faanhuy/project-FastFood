using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SmartShop.Domain.Entities;
using SmartShop.Infrastructure.Data;
using SmartShop.Infrastructure.Services;
using Xunit;

namespace SmartShop.Application.Tests.Infrastructure;

public class LocalizationServiceTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static LocalizationService CreateService(ApplicationDbContext db, IMemoryCache? cache = null)
    {
        cache ??= new MemoryCache(new MemoryCacheOptions());
        return new LocalizationService(db, cache);
    }

    // ─── DetectLanguage ─────────────────────────────────────────────────────

    [Theory]
    [InlineData(null, "vi")]
    [InlineData("", "vi")]
    [InlineData("   ", "vi")]
    public void DetectLanguage_NullOrEmpty_ReturnsVi(string? header, string expected)
    {
        var svc = CreateService(CreateDb());
        svc.DetectLanguage(header).Should().Be(expected);
    }

    [Theory]
    [InlineData("vi", "vi")]
    [InlineData("en", "en")]
    [InlineData("VI", "vi")]
    [InlineData("EN", "en")]
    public void DetectLanguage_SupportedLanguage_ReturnsIt(string header, string expected)
    {
        var svc = CreateService(CreateDb());
        svc.DetectLanguage(header).Should().Be(expected);
    }

    [Theory]
    [InlineData("fr")]
    [InlineData("zh")]
    [InlineData("ja")]
    public void DetectLanguage_UnsupportedLanguage_FallsBackToVi(string header)
    {
        var svc = CreateService(CreateDb());
        svc.DetectLanguage(header).Should().Be("vi");
    }

    [Fact]
    public void DetectLanguage_RegionSuffix_ExtractsLangCode()
    {
        var svc = CreateService(CreateDb());
        svc.DetectLanguage("en-US").Should().Be("en");
        svc.DetectLanguage("vi-VN").Should().Be("vi");
    }

    [Fact]
    public void DetectLanguage_MultiplePreferences_TakesFirst()
    {
        var svc = CreateService(CreateDb());
        svc.DetectLanguage("en,vi;q=0.9").Should().Be("en");
        svc.DetectLanguage("fr,en;q=0.9").Should().Be("vi");
    }

    // ─── FormatMessage ───────────────────────────────────────────────────────

    [Fact]
    public void FormatMessage_NoParams_ReturnsTemplate()
    {
        var svc = CreateService(CreateDb());
        svc.FormatMessage("Hello world", null).Should().Be("Hello world");
    }

    [Fact]
    public void FormatMessage_EmptyParams_ReturnsTemplate()
    {
        var svc = CreateService(CreateDb());
        svc.FormatMessage("Hello world", new Dictionary<string, string>()).Should().Be("Hello world");
    }

    [Fact]
    public void FormatMessage_WithParams_ReplacesPlaceholders()
    {
        var svc = CreateService(CreateDb());
        var result = svc.FormatMessage("Order {orderId} by {userName}", new Dictionary<string, string>
        {
            ["orderId"] = "ORD-001",
            ["userName"] = "Alice"
        });
        result.Should().Be("Order ORD-001 by Alice");
    }

    [Fact]
    public void FormatMessage_MissingParam_LeavesPlaceholderUnchanged()
    {
        var svc = CreateService(CreateDb());
        var result = svc.FormatMessage("Hello {name}", new Dictionary<string, string>());
        result.Should().Be("Hello {name}");
    }

    // ─── GetMessage — basic lookup ────────────────────────────────────────────

    [Fact]
    public void GetMessage_KeyExists_ReturnsLocalizedText()
    {
        var db = CreateDb();
        db.LocalizedMessages.Add(LocalizedMessage.Create("error.not_found", "vi", "Không tìm thấy"));
        db.SaveChanges();

        var svc = CreateService(db);
        svc.GetMessage("error.not_found", "vi").Should().Be("Không tìm thấy");
    }

    [Fact]
    public void GetMessage_KeyNotFoundForLanguage_FallsBackToVi()
    {
        var db = CreateDb();
        db.LocalizedMessages.Add(LocalizedMessage.Create("error.not_found", "vi", "Không tìm thấy"));
        db.SaveChanges();

        var svc = CreateService(db);
        svc.GetMessage("error.not_found", "en").Should().Be("Không tìm thấy");
    }

    [Fact]
    public void GetMessage_KeyNotFoundAtAll_ReturnsKeyItself()
    {
        var svc = CreateService(CreateDb());
        svc.GetMessage("error.unknown_key", "vi").Should().Be("error.unknown_key");
    }

    [Fact]
    public void GetMessage_WithParams_ReturnsFormattedText()
    {
        var db = CreateDb();
        db.LocalizedMessages.Add(LocalizedMessage.Create("order.placed", "vi", "Đơn hàng {orderId} đã được đặt"));
        db.SaveChanges();

        var svc = CreateService(db);
        var result = svc.GetMessage("order.placed", "vi", new Dictionary<string, string> { ["orderId"] = "ORD-999" });
        result.Should().Be("Đơn hàng ORD-999 đã được đặt");
    }

    [Fact]
    public void GetMessage_EnglishKeyExists_DoesNotFallBack()
    {
        var db = CreateDb();
        db.LocalizedMessages.Add(LocalizedMessage.Create("error.not_found", "vi", "Không tìm thấy"));
        db.LocalizedMessages.Add(LocalizedMessage.Create("error.not_found", "en", "Not found"));
        db.SaveChanges();

        var svc = CreateService(db);
        svc.GetMessage("error.not_found", "en").Should().Be("Not found");
    }

    // ─── GetMessage — caching ─────────────────────────────────────────────────

    [Fact]
    public void GetMessage_SecondCall_ReturnsCachedValue()
    {
        var db = CreateDb();
        db.LocalizedMessages.Add(LocalizedMessage.Create("cache.test", "vi", "Cached text"));
        db.SaveChanges();

        var cache = new MemoryCache(new MemoryCacheOptions());
        var svc = CreateService(db, cache);

        var first = svc.GetMessage("cache.test", "vi");
        db.LocalizedMessages.RemoveRange(db.LocalizedMessages);
        db.SaveChanges();

        var second = svc.GetMessage("cache.test", "vi");

        first.Should().Be("Cached text");
        second.Should().Be("Cached text");
    }

    // ─── GetFieldName ─────────────────────────────────────────────────────────

    [Fact]
    public void GetFieldName_KeyExists_ReturnsDisplayName()
    {
        var db = CreateDb();
        db.LocalizedFieldNames.Add(LocalizedFieldName.Create("user.email", "vi", "Email"));
        db.SaveChanges();

        var svc = CreateService(db);
        svc.GetFieldName("user.email", "vi").Should().Be("Email");
    }

    [Fact]
    public void GetFieldName_KeyNotFoundForLanguage_FallsBackToVi()
    {
        var db = CreateDb();
        db.LocalizedFieldNames.Add(LocalizedFieldName.Create("user.email", "vi", "Email"));
        db.SaveChanges();

        var svc = CreateService(db);
        svc.GetFieldName("user.email", "en").Should().Be("Email");
    }

    [Fact]
    public void GetFieldName_KeyNotFoundAtAll_ReturnsKeyItself()
    {
        var svc = CreateService(CreateDb());
        svc.GetFieldName("user.unknown_field", "vi").Should().Be("user.unknown_field");
    }
}
