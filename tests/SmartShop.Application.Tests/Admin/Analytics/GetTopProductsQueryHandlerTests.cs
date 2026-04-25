using FluentAssertions;
using Moq;
using SmartShop.Application.Features.Admin.Analytics.Queries.GetTopProducts;
using SmartShop.Domain.Interfaces;
using Xunit;

namespace SmartShop.Application.Tests.Admin.Analytics;

public class GetTopProductsQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly GetTopProductsQueryHandler _handler;

    private static readonly DateTime From = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime To = new DateTime(2025, 1, 31, 23, 59, 59, DateTimeKind.Utc);

    public GetTopProductsQueryHandlerTests()
    {
        _handler = new GetTopProductsQueryHandler(_orderRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ValidDateRange_ReturnsMappedTopProductDtos()
    {
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        var rawRows = new List<(Guid ProductId, string ProductName, int TotalQuantity, decimal TotalRevenue)>
        {
            (productId1, "Laptop Dell XPS", 20, 2_000_000m),
            (productId2, "iPhone 15 Pro", 15, 3_000_000m),
        };

        _orderRepoMock
            .Setup(r => r.GetTopProductsAsync(From, To, 5, default))
            .ReturnsAsync(rawRows);

        var query = new GetTopProductsQuery(From, To, Limit: 5);
        var result = await _handler.Handle(query, default);

        result.Should().HaveCount(2);
        result[0].ProductId.Should().Be(productId1);
        result[0].ProductName.Should().Be("Laptop Dell XPS");
        result[0].TotalQuantity.Should().Be(20);
        result[0].TotalRevenue.Should().Be(2_000_000m);
        result[1].ProductId.Should().Be(productId2);
        result[1].ProductName.Should().Be("iPhone 15 Pro");
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyList()
    {
        _orderRepoMock
            .Setup(r => r.GetTopProductsAsync(From, To, It.IsAny<int>(), default))
            .ReturnsAsync(Enumerable.Empty<(Guid, string, int, decimal)>());

        var result = await _handler.Handle(new GetTopProductsQuery(From, To), default);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_DateRangeAndLimitPassedCorrectlyToRepository()
    {
        var customFrom = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        var customTo = new DateTime(2025, 3, 31, 23, 59, 59, DateTimeKind.Utc);
        const int limit = 10;

        _orderRepoMock
            .Setup(r => r.GetTopProductsAsync(customFrom, customTo, limit, default))
            .ReturnsAsync(Enumerable.Empty<(Guid, string, int, decimal)>());

        await _handler.Handle(new GetTopProductsQuery(customFrom, customTo, Limit: limit), default);

        _orderRepoMock.Verify(
            r => r.GetTopProductsAsync(customFrom, customTo, limit, default),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DefaultLimit_UsesLimitOfFive()
    {
        _orderRepoMock
            .Setup(r => r.GetTopProductsAsync(From, To, 5, default))
            .ReturnsAsync(Enumerable.Empty<(Guid, string, int, decimal)>());

        // GetTopProductsQuery has default Limit = 5
        await _handler.Handle(new GetTopProductsQuery(From, To), default);

        _orderRepoMock.Verify(r => r.GetTopProductsAsync(From, To, 5, default), Times.Once);
    }

    [Fact]
    public async Task Handle_SingleProduct_ReturnsSingleItemList()
    {
        var productId = Guid.NewGuid();
        var rawRows = new List<(Guid, string, int, decimal)>
        {
            (productId, "Best Seller", 100, 10_000_000m),
        };

        _orderRepoMock
            .Setup(r => r.GetTopProductsAsync(From, To, 5, default))
            .ReturnsAsync(rawRows);

        var result = await _handler.Handle(new GetTopProductsQuery(From, To), default);

        result.Should().HaveCount(1);
        result[0].ProductId.Should().Be(productId);
        result[0].TotalQuantity.Should().Be(100);
        result[0].TotalRevenue.Should().Be(10_000_000m);
    }
}
