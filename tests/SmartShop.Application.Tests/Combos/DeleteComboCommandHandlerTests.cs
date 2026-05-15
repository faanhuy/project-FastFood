using FluentAssertions;
using Moq;
using SmartShop.Domain.Common.Exceptions;
using Xunit;
using SmartShop.Application.Features.Combos.Commands.DeleteCombo;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Tests.Combos;

public class DeleteComboCommandHandlerTests
{
    private readonly Mock<IComboRepository> _comboRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private DeleteComboCommandHandler CreateHandler() =>
        new(_comboRepo.Object, _uow.Object);

    private static Combo CreateTestCombo()
    {
        var combo = Combo.Create("Test Combo", "Test Title", "Description", "image.jpg", 99.99m, DateTime.UtcNow.AddDays(1));
        var item = ComboItem.Create(combo.Id, Guid.NewGuid(), "Product", null, null, 1, 50m);
        combo.AddItem(item);
        return combo;
    }

    [Fact]
    public async Task Handle_ValidRequest_DeactivatesCombo()
    {
        var combo = CreateTestCombo();
        var command = new DeleteComboCommand(combo.Id);

        _comboRepo.Setup(r => r.GetByIdAsync(combo.Id, default))
            .ReturnsAsync(combo);
        _comboRepo.Setup(r => r.Update(It.IsAny<Combo>()));
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await CreateHandler().Handle(command, default);

        result.Success.Should().BeTrue();
        combo.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ComboNotFound_ThrowsNotFoundException()
    {
        var comboId = Guid.NewGuid();
        var command = new DeleteComboCommand(comboId);

        _comboRepo.Setup(r => r.GetByIdAsync(comboId, default))
            .ReturnsAsync((Combo?)null);

        var act = () => CreateHandler().Handle(command, default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsRepositoryUpdate()
    {
        var combo = CreateTestCombo();
        var command = new DeleteComboCommand(combo.Id);

        _comboRepo.Setup(r => r.GetByIdAsync(combo.Id, default))
            .ReturnsAsync(combo);
        _comboRepo.Setup(r => r.Update(It.IsAny<Combo>()));
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        await CreateHandler().Handle(command, default);

        _comboRepo.Verify(r => r.Update(It.IsAny<Combo>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccessMessage()
    {
        var combo = CreateTestCombo();
        var command = new DeleteComboCommand(combo.Id);

        _comboRepo.Setup(r => r.GetByIdAsync(combo.Id, default))
            .ReturnsAsync(combo);
        _comboRepo.Setup(r => r.Update(It.IsAny<Combo>()));
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await CreateHandler().Handle(command, default);

        result.Message.Should().Be("Combo đã được vô hiệu hóa.");
    }
}
