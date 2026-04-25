using FluentAssertions;
using Moq;
using SmartShop.Application.Features.Admin.Analytics.Queries.GetRevenueSummary;
using SmartShop.Domain.Interfaces;
using Xunit;

namespace SmartShop.Application.Tests.Admin.Analytics;

public class GetRevenueSummaryQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly GetRevenueSummaryQueryHandler _handler;

    private static readonly DateTime From = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime To = new DateTime(2025, 1, 31, 23, 59, 59, DateTimeKind.Utc);

    public GetRevenueSummaryQueryHandlerTests()
    {
        _handler = new GetRevenueSummaryQueryHandler(_orderRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ValidDateRange_ReturnsCorrectRevenueSummary()
    {
        _orderRepoMock
            .Setup(r => r.GetRevenueSummaryAsync(From, To, default))
            .ReturnsAsync((1_000_000m, 10, 100_000m));

        _orderRepoMock
            .Setup(r => r.GetPrevPeriodSummaryAsync(From, To, default))
            .ReturnsAsync((800_000m, 8));

        _orderRepoMock
            .Setup(r => r.GetNewCustomersCountAsync(From, To, default))
            .ReturnsAsync(5);

        var query = new GetRevenueSummaryQuery(From, To);
        var result = await _handler.Handle(query, default);

        result.Should().NotBeNull();
        result.TotalRevenue.Should().Be(1_000_000m);
        result.TotalOrders.Should().Be(10);
        result.AverageOrderValue.Should().Be(100_000m);
        result.NewCustomers.Should().Be(5);
        result.RevenueGrowthPercent.Should().Be(25m); // (1000000 - 800000) / 800000 * 100
    }

    [Fact]
    public async Task Handle_NoPreviousPeriodRevenue_GrowthIs100WhenCurrentPositive()
    {
        _orderRepoMock
            .Setup(r => r.GetRevenueSummaryAsync(From, To, default))
            .ReturnsAsync((500_000m, 5, 100_000m));

        _orderRepoMock
            .Setup(r => r.GetPrevPeriodSummaryAsync(From, To, default))
            .ReturnsAsync((0m, 0)); // no previous period data

        _orderRepoMock
            .Setup(r => r.GetNewCustomersCountAsync(From, To, default))
            .ReturnsAsync(3);

        var result = await _handler.Handle(new GetRevenueSummaryQuery(From, To), default);

        result.RevenueGrowthPercent.Should().Be(100m);
    }

    [Fact]
    public async Task Handle_NoPreviousPeriodAndNoCurrentRevenue_GrowthIsZero()
    {
        _orderRepoMock
            .Setup(r => r.GetRevenueSummaryAsync(From, To, default))
            .ReturnsAsync((0m, 0, 0m));

        _orderRepoMock
            .Setup(r => r.GetPrevPeriodSummaryAsync(From, To, default))
            .ReturnsAsync((0m, 0));

        _orderRepoMock
            .Setup(r => r.GetNewCustomersCountAsync(From, To, default))
            .ReturnsAsync(0);

        var result = await _handler.Handle(new GetRevenueSummaryQuery(From, To), default);

        result.TotalRevenue.Should().Be(0m);
        result.TotalOrders.Should().Be(0);
        result.NewCustomers.Should().Be(0);
        result.RevenueGrowthPercent.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_DateRangePassedCorrectlyToRepository()
    {
        var customFrom = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        var customTo = new DateTime(2025, 3, 31, 23, 59, 59, DateTimeKind.Utc);

        _orderRepoMock
            .Setup(r => r.GetRevenueSummaryAsync(customFrom, customTo, default))
            .ReturnsAsync((200_000m, 2, 100_000m));

        _orderRepoMock
            .Setup(r => r.GetPrevPeriodSummaryAsync(customFrom, customTo, default))
            .ReturnsAsync((150_000m, 2));

        _orderRepoMock
            .Setup(r => r.GetNewCustomersCountAsync(customFrom, customTo, default))
            .ReturnsAsync(1);

        await _handler.Handle(new GetRevenueSummaryQuery(customFrom, customTo), default);

        _orderRepoMock.Verify(r => r.GetRevenueSummaryAsync(customFrom, customTo, default), Times.Once);
        _orderRepoMock.Verify(r => r.GetPrevPeriodSummaryAsync(customFrom, customTo, default), Times.Once);
        _orderRepoMock.Verify(r => r.GetNewCustomersCountAsync(customFrom, customTo, default), Times.Once);
    }

    [Fact]
    public async Task Handle_RevenueDecline_ReturnsNegativeGrowthPercent()
    {
        _orderRepoMock
            .Setup(r => r.GetRevenueSummaryAsync(From, To, default))
            .ReturnsAsync((600_000m, 6, 100_000m));

        _orderRepoMock
            .Setup(r => r.GetPrevPeriodSummaryAsync(From, To, default))
            .ReturnsAsync((1_000_000m, 10));

        _orderRepoMock
            .Setup(r => r.GetNewCustomersCountAsync(From, To, default))
            .ReturnsAsync(2);

        var result = await _handler.Handle(new GetRevenueSummaryQuery(From, To), default);

        result.RevenueGrowthPercent.Should().Be(-40m); // (600000 - 1000000) / 1000000 * 100
    }
}
