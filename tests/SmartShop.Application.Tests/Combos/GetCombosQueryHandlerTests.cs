using FluentAssertions;
using Moq;
using Xunit;
using SmartShop.Application.Features.Combos.Queries.GetCombos;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Tests.Combos;

public class GetCombosQueryHandlerTests
{
    private readonly Mock<IComboRepository> _comboRepo = new();

    private GetCombosQueryHandler CreateHandler() => new(_comboRepo.Object);

    private static Combo CreateTestCombo(int itemCount = 2)
    {
        var combo = Combo.Create("Test Combo", "Test Title", "Description", "image.jpg", 99.99m, DateTime.UtcNow.AddDays(1));
        for (int i = 0; i < itemCount; i++)
        {
            var item = ComboItem.Create(combo.Id, Guid.NewGuid(), $"Product {i}", null, null, 1, 50m);
            combo.AddItem(item);
        }
        return combo;
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsPaginatedCombos()
    {
        var combo1 = CreateTestCombo(2);
        var combo2 = CreateTestCombo(3);
        var combos = new List<Combo> { combo1, combo2 };

        _comboRepo.Setup(r => r.GetAllAsync(1, 20, default)).ReturnsAsync(combos);
        _comboRepo.Setup(r => r.CountAsync(default)).ReturnsAsync(2);

        var result = await CreateHandler().Handle(new GetCombosQuery(1, 20), default);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Items.Count.Should().Be(2);
        result.Data.TotalCount.Should().Be(2);
        result.Data.Page.Should().Be(1);
        result.Data.PageSize.Should().Be(20);
        result.Data.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ValidRequest_MapsComboToSummaryDto()
    {
        var combo = CreateTestCombo(3);
        var combos = new List<Combo> { combo };

        _comboRepo.Setup(r => r.GetAllAsync(1, 20, default)).ReturnsAsync(combos);
        _comboRepo.Setup(r => r.CountAsync(default)).ReturnsAsync(1);

        var result = await CreateHandler().Handle(new GetCombosQuery(1, 20), default);

        result.Data.Should().NotBeNull();
        var dto = result.Data.Items[0];
        dto.Id.Should().Be(combo.Id);
        dto.Name.Should().Be(combo.Name);
        dto.Title.Should().Be(combo.Title);
        dto.ImageUrl.Should().Be(combo.ImageUrl);
        dto.OriginalPrice.Should().Be(combo.OriginalPrice);
        dto.SalePrice.Should().Be(combo.SalePrice);
        dto.IsActive.Should().Be(combo.IsActive);
        dto.ItemCount.Should().Be(3);
        dto.CreatedAt.Should().Be(combo.CreatedAt);
    }

    [Fact]
    public async Task Handle_EmptyCombos_ReturnsEmptyList()
    {
        var combos = new List<Combo>();

        _comboRepo.Setup(r => r.GetAllAsync(1, 20, default)).ReturnsAsync(combos);
        _comboRepo.Setup(r => r.CountAsync(default)).ReturnsAsync(0);

        var result = await CreateHandler().Handle(new GetCombosQuery(1, 20), default);

        result.Data!.Items.Count.Should().Be(0);
        result.Data.TotalCount.Should().Be(0);
        result.Data.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_PaginationPage2_CalculatesTotalPagesCorrectly()
    {
        var combos = new List<Combo> { CreateTestCombo() };

        _comboRepo.Setup(r => r.GetAllAsync(2, 10, default)).ReturnsAsync(combos);
        _comboRepo.Setup(r => r.CountAsync(default)).ReturnsAsync(25);

        var result = await CreateHandler().Handle(new GetCombosQuery(2, 10), default);

        result.Data!.TotalPages.Should().Be(3); // (25 + 10 - 1) / 10 = 3
        result.Data.Page.Should().Be(2);
        result.Data.PageSize.Should().Be(10);
    }
}
