using FluentAssertions;
using SmartShop.Inventory.Application.Features.Stock.Commands.ConfirmStock;
using SmartShop.Inventory.Application.Features.Stock.Commands.ReleaseStock;
using SmartShop.Inventory.Application.Features.Stock.Commands.ReserveStock;
using SmartShop.Inventory.Application.Features.Stock.Queries.GetStockLevel;

namespace SmartShop.Inventory.Tests.Validators;

public class StockValidatorTests
{
    private static ReserveStockLine ValidLine(int quantity = 1) =>
        new(Guid.NewGuid(), Guid.NewGuid(), null, quantity);

    // ── ReserveStockCommandValidator ──────────────────────────────────────────

    [Fact]
    public void ReserveStock_ValidCommand_Passes()
    {
        var command = new ReserveStockCommand(Guid.NewGuid(), [ValidLine(2)]);

        new ReserveStockCommandValidator().Validate(command).IsValid.Should().BeTrue();
    }

    [Fact]
    public void ReserveStock_EmptyOrderId_UsesOrderIdMessageKey()
    {
        var command = new ReserveStockCommand(Guid.Empty, [ValidLine()]);

        var result = new ReserveStockCommandValidator().Validate(command);

        result.Errors.Should().Contain(e => e.ErrorMessage == "validation.order_id_invalid");
    }

    [Fact]
    public void ReserveStock_NoItems_Fails()
    {
        var command = new ReserveStockCommand(Guid.NewGuid(), []);

        var result = new ReserveStockCommandValidator().Validate(command);

        result.Errors.Should().Contain(e => e.ErrorMessage == "validation.required_field");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void ReserveStock_NonPositiveQuantity_UsesQuantityMessageKey(int quantity)
    {
        var command = new ReserveStockCommand(Guid.NewGuid(), [ValidLine(quantity)]);

        var result = new ReserveStockCommandValidator().Validate(command);

        result.Errors.Should().Contain(e => e.ErrorMessage == "validation.quantity_positive");
    }

    [Fact]
    public void ReserveStock_EmptyProductOrStoreId_Fails()
    {
        var command = new ReserveStockCommand(Guid.NewGuid(),
        [
            new ReserveStockLine(Guid.Empty, Guid.NewGuid(), null, 1),
            new ReserveStockLine(Guid.NewGuid(), Guid.Empty, null, 1)
        ]);

        var result = new ReserveStockCommandValidator().Validate(command);

        result.Errors.Should().Contain(e => e.ErrorMessage == "validation.product_id_invalid");
        result.Errors.Should().Contain(e => e.ErrorMessage == "validation.store_id_invalid");
    }

    // ── Release / Confirm / GetStockLevel ─────────────────────────────────────

    [Fact]
    public void ReleaseStock_EmptyOrderId_Fails()
    {
        new ReleaseStockCommandValidator().Validate(new ReleaseStockCommand(Guid.Empty))
            .IsValid.Should().BeFalse();
        new ReleaseStockCommandValidator().Validate(new ReleaseStockCommand(Guid.NewGuid()))
            .IsValid.Should().BeTrue();
    }

    [Fact]
    public void ConfirmStock_EmptyOrderId_Fails()
    {
        new ConfirmStockCommandValidator().Validate(new ConfirmStockCommand(Guid.Empty))
            .IsValid.Should().BeFalse();
        new ConfirmStockCommandValidator().Validate(new ConfirmStockCommand(Guid.NewGuid()))
            .IsValid.Should().BeTrue();
    }

    [Fact]
    public void GetStockLevel_EmptyProductOrStoreId_Fails()
    {
        var validator = new GetStockLevelQueryValidator();

        validator.Validate(new GetStockLevelQuery(Guid.Empty, Guid.NewGuid(), null)).IsValid.Should().BeFalse();
        validator.Validate(new GetStockLevelQuery(Guid.NewGuid(), Guid.Empty, null)).IsValid.Should().BeFalse();
        validator.Validate(new GetStockLevelQuery(Guid.NewGuid(), Guid.NewGuid(), null)).IsValid.Should().BeTrue();
    }
}
