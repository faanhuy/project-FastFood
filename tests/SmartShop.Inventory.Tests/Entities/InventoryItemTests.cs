using FluentAssertions;
using SmartShop.Inventory.Domain.Common.Exceptions;
using SmartShop.Inventory.Domain.Entities;
using SmartShop.Inventory.Tests.TestData;
using Xunit;

namespace SmartShop.Inventory.Tests.Entities;

public class InventoryItemTests
{
    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_ValidArguments_SetsFieldsAndStartsWithNothingReserved()
    {
        var productId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var sizeId = Guid.NewGuid();

        var item = InventoryItem.Create(productId, storeId, sizeId, "burger-M", 30, 7);

        item.ProductId.Should().Be(productId);
        item.StoreId.Should().Be(storeId);
        item.SizeId.Should().Be(sizeId);
        item.Sku.Should().Be("burger-M");
        item.QuantityAvailable.Should().Be(30);
        item.QuantityReserved.Should().Be(0);
        item.LowStockThreshold.Should().Be(7);
        item.Sellable.Should().Be(30);
    }

    [Fact]
    public void Create_WithoutSize_LeavesSizeIdNull()
    {
        var item = StockTestData.Item();

        item.SizeId.Should().BeNull();
    }

    [Fact]
    public void Create_EmptyProductId_Throws()
    {
        var act = () => InventoryItem.Create(Guid.Empty, Guid.NewGuid(), null, "sku", 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_EmptyStoreId_Throws()
    {
        var act = () => InventoryItem.Create(Guid.NewGuid(), Guid.Empty, null, "sku", 1);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankSku_Throws(string sku)
    {
        var act = () => InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), null, sku, 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_NegativeQuantity_Throws()
    {
        var act = () => InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), null, "sku", -1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_NegativeLowStockThreshold_Throws()
    {
        var act = () => InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), null, "sku", 1, -1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ── Sellable / IsLowStock ─────────────────────────────────────────────────

    [Fact]
    public void Sellable_IsPhysicalStockMinusReserved()
    {
        var item = StockTestData.Item(available: 10);

        item.Reserve(4);

        item.Sellable.Should().Be(6);
    }

    [Fact]
    public void IsLowStock_TrueWhenSellableAtOrBelowThreshold()
    {
        var item = StockTestData.Item(available: 10, lowStockThreshold: 5);

        item.Reserve(4);
        item.IsLowStock.Should().BeFalse();   // sellable 6 > 5

        item.Reserve(1);
        item.IsLowStock.Should().BeTrue();    // sellable 5 <= 5
    }

    // ── Reserve ───────────────────────────────────────────────────────────────

    [Fact]
    public void Reserve_WithinSellable_IncreasesReservedButKeepsPhysicalStock()
    {
        var item = StockTestData.Item(available: 10);

        item.Reserve(3);

        item.QuantityReserved.Should().Be(3);
        item.QuantityAvailable.Should().Be(10);
    }

    [Fact]
    public void Reserve_ExactlyTheSellableAmount_Succeeds()
    {
        var item = StockTestData.Item(available: 5);

        item.Reserve(5);

        item.Sellable.Should().Be(0);
    }

    [Fact]
    public void Reserve_MoreThanSellable_ThrowsConflictAndChangesNothing()
    {
        var item = StockTestData.Item(available: 10);
        item.Reserve(6);   // còn sellable 4

        var act = () => item.Reserve(5);

        var ex = act.Should().Throw<ConflictException>().Which;
        ex.MessageKey.Should().Be("error.inventory_insufficient_stock");
        ex.Params.Should().Contain("available", "4").And.Contain("required", "5");
        item.QuantityReserved.Should().Be(6);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Reserve_NonPositiveQuantity_Throws(int quantity)
    {
        var item = StockTestData.Item();

        var act = () => item.Reserve(quantity);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ── Release ───────────────────────────────────────────────────────────────

    [Fact]
    public void Release_AfterReserve_RestoresSellable()
    {
        var item = StockTestData.Item(available: 10);
        item.Reserve(4);

        item.Release(4);

        item.QuantityReserved.Should().Be(0);
        item.Sellable.Should().Be(10);
        item.QuantityAvailable.Should().Be(10);
    }

    [Fact]
    public void Release_MoreThanReserved_ThrowsConflictAndChangesNothing()
    {
        var item = StockTestData.Item(available: 10);
        item.Reserve(2);

        var act = () => item.Release(3);

        act.Should().Throw<ConflictException>()
            .Which.MessageKey.Should().Be("error.inventory_release_exceeds_reserved");
        item.QuantityReserved.Should().Be(2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Release_NonPositiveQuantity_Throws(int quantity)
    {
        var item = StockTestData.Item();
        item.Reserve(1);

        var act = () => item.Release(quantity);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ── Confirm ───────────────────────────────────────────────────────────────

    [Fact]
    public void Confirm_ReducesBothReservedAndPhysicalStock()
    {
        var item = StockTestData.Item(available: 10);
        item.Reserve(4);

        item.Confirm(4);

        item.QuantityReserved.Should().Be(0);
        item.QuantityAvailable.Should().Be(6);   // hàng thật sự rời kho
        item.Sellable.Should().Be(6);
    }

    [Fact]
    public void Confirm_PartOfTheReservation_KeepsTheRemainderReserved()
    {
        var item = StockTestData.Item(available: 10);
        item.Reserve(4);

        item.Confirm(3);

        item.QuantityReserved.Should().Be(1);
        item.QuantityAvailable.Should().Be(7);
        item.Sellable.Should().Be(6);
    }

    [Fact]
    public void Confirm_MoreThanReserved_ThrowsConflictAndChangesNothing()
    {
        var item = StockTestData.Item(available: 10);
        item.Reserve(2);

        var act = () => item.Confirm(3);

        act.Should().Throw<ConflictException>()
            .Which.MessageKey.Should().Be("error.inventory_release_exceeds_reserved");
        item.QuantityReserved.Should().Be(2);
        item.QuantityAvailable.Should().Be(10);
    }

    // ── Bất biến 0 <= Reserved <= Available ───────────────────────────────────

    [Fact]
    public void AnySequenceOfValidOperations_KeepsReservedBetweenZeroAndAvailable()
    {
        var item = StockTestData.Item(available: 10);
        var steps = new List<Action>
        {
            () => item.Reserve(6),
            () => item.Confirm(2),
            () => item.Release(3),
            () => item.Reserve(1),
            () => item.Confirm(2),     // reserved 0, available 6
            () => item.Reserve(3)      // reserved 3 <= available 6
        };

        foreach (var step in steps)
        {
            step();

            item.QuantityReserved.Should().BeGreaterThanOrEqualTo(0);
            item.QuantityReserved.Should().BeLessThanOrEqualTo(item.QuantityAvailable);
        }
    }
}
