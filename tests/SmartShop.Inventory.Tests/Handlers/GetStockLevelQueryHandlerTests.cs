using FluentAssertions;
using Moq;
using SmartShop.Inventory.Application.Features.Stock.Queries.GetStockLevel;
using SmartShop.Inventory.Domain.Common.Exceptions;
using SmartShop.Inventory.Domain.Entities;
using SmartShop.Inventory.Domain.Interfaces;
using SmartShop.Inventory.Tests.TestData;
using Xunit;

namespace SmartShop.Inventory.Tests.Handlers;

public class GetStockLevelQueryHandlerTests
{
    private readonly Mock<IInventoryItemRepository> _items = new();

    private GetStockLevelQueryHandler Sut => new(_items.Object);

    [Fact]
    public async Task Handle_ItemExists_ReturnsSellableNotPhysicalStock()
    {
        var item = StockTestData.Item(available: 10);
        item.Reserve(3);
        _items
            .Setup(r => r.GetByKeyAsync(item.StoreId, item.ProductId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var level = await Sut.Handle(
            new GetStockLevelQuery(item.ProductId, item.StoreId, null), CancellationToken.None);

        level.Should().Be(7);   // available - reserved
    }

    [Fact]
    public async Task Handle_SizedItem_PassesTheSizeIdToTheRepository()
    {
        var sizeId = Guid.NewGuid();
        var item = StockTestData.Item(available: 4, sizeId: sizeId);
        _items
            .Setup(r => r.GetByKeyAsync(item.StoreId, item.ProductId, sizeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var level = await Sut.Handle(
            new GetStockLevelQuery(item.ProductId, item.StoreId, sizeId), CancellationToken.None);

        level.Should().Be(4);
    }

    [Fact]
    public async Task Handle_ItemMissing_ThrowsNotFoundNamingTheKey()
    {
        var productId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        _items
            .Setup(r => r.GetByKeyAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((InventoryItem?)null);

        var act = () => Sut.Handle(new GetStockLevelQuery(productId, storeId, null), CancellationToken.None);

        var ex = (await act.Should().ThrowAsync<NotFoundException>()).Which;
        ex.EntityName.Should().Be(nameof(InventoryItem));
        ex.Key.ToString().Should().Contain(productId.ToString()).And.Contain(storeId.ToString());
    }
}
