using FluentAssertions;
using Moq;
using SmartShop.Application.Features.Admin.Analytics.Queries.GetOrderStatusBreakdown;
using SmartShop.Domain.Interfaces;
using Xunit;

namespace SmartShop.Application.Tests.Admin.Analytics;

public class GetOrderStatusBreakdownQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly GetOrderStatusBreakdownQueryHandler _handler;

    public GetOrderStatusBreakdownQueryHandlerTests()
    {
        _handler = new GetOrderStatusBreakdownQueryHandler(_orderRepoMock.Object);
    }

    [Fact]
    public async Task Handle_MultipleStatuses_ReturnsMappedBreakdownDtos()
    {
        var rawRows = new List<(string Status, int Count)>
        {
            ("Pending", 10),
            ("Processing", 5),
            ("Shipped", 20),
            ("Delivered", 50),
            ("Cancelled", 3),
        };

        _orderRepoMock
            .Setup(r => r.GetOrderStatusBreakdownAsync(default))
            .ReturnsAsync(rawRows);

        var query = new GetOrderStatusBreakdownQuery();
        var result = await _handler.Handle(query, default);

        result.Should().HaveCount(5);
        result.Should().ContainSingle(r => r.Status == "Pending" && r.Count == 10);
        result.Should().ContainSingle(r => r.Status == "Processing" && r.Count == 5);
        result.Should().ContainSingle(r => r.Status == "Shipped" && r.Count == 20);
        result.Should().ContainSingle(r => r.Status == "Delivered" && r.Count == 50);
        result.Should().ContainSingle(r => r.Status == "Cancelled" && r.Count == 3);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyList()
    {
        _orderRepoMock
            .Setup(r => r.GetOrderStatusBreakdownAsync(default))
            .ReturnsAsync(Enumerable.Empty<(string, int)>());

        var result = await _handler.Handle(new GetOrderStatusBreakdownQuery(), default);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_RepositoryCalledOnce()
    {
        _orderRepoMock
            .Setup(r => r.GetOrderStatusBreakdownAsync(default))
            .ReturnsAsync(Enumerable.Empty<(string, int)>());

        await _handler.Handle(new GetOrderStatusBreakdownQuery(), default);

        _orderRepoMock.Verify(r => r.GetOrderStatusBreakdownAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_SingleStatus_ReturnsSingleItemList()
    {
        var rawRows = new List<(string Status, int Count)>
        {
            ("Delivered", 100),
        };

        _orderRepoMock
            .Setup(r => r.GetOrderStatusBreakdownAsync(default))
            .ReturnsAsync(rawRows);

        var result = await _handler.Handle(new GetOrderStatusBreakdownQuery(), default);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be("Delivered");
        result[0].Count.Should().Be(100);
    }

    [Fact]
    public async Task Handle_IncludesCancelledStatus_NotFilteredOut()
    {
        // Unlike revenue queries, order status breakdown includes ALL statuses including Cancelled
        var rawRows = new List<(string Status, int Count)>
        {
            ("Delivered", 80),
            ("Cancelled", 20),
        };

        _orderRepoMock
            .Setup(r => r.GetOrderStatusBreakdownAsync(default))
            .ReturnsAsync(rawRows);

        var result = await _handler.Handle(new GetOrderStatusBreakdownQuery(), default);

        result.Should().HaveCount(2);
        result.Should().ContainSingle(r => r.Status == "Cancelled" && r.Count == 20);
    }

    [Fact]
    public async Task Handle_PreservesOrderReturnedByRepository()
    {
        var rawRows = new List<(string Status, int Count)>
        {
            ("Delivered", 50),
            ("Pending", 10),
            ("Cancelled", 5),
        };

        _orderRepoMock
            .Setup(r => r.GetOrderStatusBreakdownAsync(default))
            .ReturnsAsync(rawRows);

        var result = await _handler.Handle(new GetOrderStatusBreakdownQuery(), default);

        result.Select(r => r.Status).Should().ContainInOrder("Delivered", "Pending", "Cancelled");
    }
}
