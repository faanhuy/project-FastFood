using FluentAssertions;
using SmartShop.Domain.Entities;
using Xunit;

namespace SmartShop.Domain.Tests;

public class ComboTests
{
    private static Combo MakeCombo(
        DateTime? startsAt = null,
        DateTime? endsAt = null,
        bool isActive = true)
    {
        var starts = startsAt ?? DateTime.UtcNow.AddDays(-1);
        var combo = Combo.Create(
            "Test Combo",
            "Test Title",
            "Description",
            "image.jpg",
            99.99m,
            starts,
            endsAt
        );

        if (!isActive)
            combo.Deactivate();

        return combo;
    }

    // ── IsCurrentlyActive ─────────────────────────────────────────────────────

    [Fact]
    public void IsCurrentlyActive_NotYetStarted_ReturnsFalse()
    {
        var startsAt = DateTime.UtcNow.AddDays(1); // Starts tomorrow
        var combo = MakeCombo(startsAt: startsAt);

        combo.IsCurrentlyActive().Should().BeFalse();
    }

    [Fact]
    public void IsCurrentlyActive_Expired_ReturnsFalse()
    {
        var startsAt = DateTime.UtcNow.AddDays(-5);
        var endsAt = DateTime.UtcNow.AddDays(-1); // Expired yesterday
        var combo = MakeCombo(startsAt: startsAt, endsAt: endsAt);

        combo.IsCurrentlyActive().Should().BeFalse();
    }

    [Fact]
    public void IsCurrentlyActive_Active_ReturnsTrue()
    {
        var startsAt = DateTime.UtcNow.AddDays(-1);
        var endsAt = DateTime.UtcNow.AddDays(5);
        var combo = MakeCombo(startsAt: startsAt, endsAt: endsAt);

        combo.IsCurrentlyActive().Should().BeTrue();
    }

    [Fact]
    public void IsCurrentlyActive_NoEndsAt_ActiveForever()
    {
        var startsAt = DateTime.UtcNow.AddDays(-1);
        var combo = MakeCombo(startsAt: startsAt, endsAt: null);

        combo.IsCurrentlyActive().Should().BeTrue();
    }

    [Fact]
    public void IsCurrentlyActive_Deactivated_ReturnsFalse()
    {
        var startsAt = DateTime.UtcNow.AddDays(-1);
        var endsAt = DateTime.UtcNow.AddDays(5);
        var combo = MakeCombo(startsAt: startsAt, endsAt: endsAt, isActive: false);

        combo.IsCurrentlyActive().Should().BeFalse();
    }

    [Fact]
    public void IsCurrentlyActive_JustStarted_ReturnsTrue()
    {
        var startsAt = DateTime.UtcNow; // Starts now (boundary test)
        var combo = MakeCombo(startsAt: startsAt);

        combo.IsCurrentlyActive().Should().BeTrue();
    }

    [Fact]
    public void IsCurrentlyActive_JustExpired_ReturnsFalse()
    {
        var startsAt = DateTime.UtcNow.AddDays(-1);
        var endsAt = DateTime.UtcNow; // Ends now (boundary test)
        var combo = MakeCombo(startsAt: startsAt, endsAt: endsAt);

        combo.IsCurrentlyActive().Should().BeFalse();
    }

    // ── RecalculateOriginalPrice ──────────────────────────────────────────────

    [Fact]
    public void RecalculateOriginalPrice_SingleItem_CalculatesCorrectly()
    {
        var combo = MakeCombo();
        var comboItem = ComboItem.Create(combo.Id, Guid.NewGuid(), "Product", null, null, 2, 50m);

        combo.AddItem(comboItem);

        combo.OriginalPrice.Should().Be(100m); // 50 * 2
    }

    [Fact]
    public void RecalculateOriginalPrice_MultipleItems_SumsCorrectly()
    {
        var combo = MakeCombo();
        var item1 = ComboItem.Create(combo.Id, Guid.NewGuid(), "Product1", null, null, 2, 50m);
        var item2 = ComboItem.Create(combo.Id, Guid.NewGuid(), "Product2", null, null, 3, 75m);

        combo.AddItem(item1);
        combo.AddItem(item2);

        combo.OriginalPrice.Should().Be(325m); // 50*2 + 75*3
    }

    [Fact]
    public void RecalculateOriginalPrice_AfterReplaceItems_UpdatesCorrectly()
    {
        var combo = MakeCombo();
        var item1 = ComboItem.Create(combo.Id, Guid.NewGuid(), "Product1", null, null, 2, 50m);
        combo.AddItem(item1);
        combo.OriginalPrice.Should().Be(100m);

        var item2 = ComboItem.Create(combo.Id, Guid.NewGuid(), "Product2", null, null, 3, 75m);
        combo.ReplaceItems(new List<ComboItem> { item2 });

        combo.OriginalPrice.Should().Be(225m); // 75*3
    }

    // ── Activation/Deactivation ──────────────────────────────────────────────

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var combo = MakeCombo();
        combo.IsActive.Should().BeTrue();

        combo.Deactivate();

        combo.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_SetsIsActiveTrue()
    {
        var combo = MakeCombo(isActive: false);
        combo.IsActive.Should().BeFalse();

        combo.Activate();

        combo.IsActive.Should().BeTrue();
    }
}
