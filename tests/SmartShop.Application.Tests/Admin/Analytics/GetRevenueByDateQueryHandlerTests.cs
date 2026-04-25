using FluentAssertions;
using Moq;
using SmartShop.Application.Features.Admin.Analytics.Queries.GetRevenueByDate;
using SmartShop.Domain.Interfaces;
using Xunit;

namespace SmartShop.Application.Tests.Admin.Analytics;

public class GetRevenueByDateQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly GetRevenueByDateQueryHandler _handler;

    private static readonly DateTime From = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime To = new DateTime(2025, 1, 7, 23, 59, 59, DateTimeKind.Utc);

    public GetRevenueByDateQueryHandlerTests()
    {
        _handler = new GetRevenueByDateQueryHandler(_orderRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ValidDateRange_ReturnsMappedRevenueByDateDtos()
    {
        var rawRows = new List<(DateTime Date, decimal Revenue, int OrderCount)>
        {
            (new DateTime(2025, 1, 1), 150_000m, 3),
            (new DateTime(2025, 1, 2), 200_000m, 4),
            (new DateTime(2025, 1, 3), 100_000m, 2),
        };

        _orderRepoMock
            .Setup(r => r.GetRevenueByDateAsync(From, To, default))
            .ReturnsAsync(rawRows);

        var query = new GetRevenueByDateQuery(From, To);
        var result = await _handler.Handle(query, default);

        result.Should().HaveCount(3);
        result[0].Date.Should().Be("2025-01-01");
        result[0].Revenue.Should().Be(150_000m);
        result[0].OrderCount.Should().Be(3);
        result[1].Date.Should().Be("2025-01-02");
        result[2].Date.Should().Be("2025-01-03");
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyList()
    {
        _orderRepoMock
            .Setup(r => r.GetRevenueByDateAsync(From, To, default))
            .ReturnsAsync(Enumerable.Empty<(DateTime, decimal, int)>());

        var result = await _handler.Handle(new GetRevenueByDateQuery(From, To), default);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_DateRangePassedCorrectlyToRepository()
    {
        var customFrom = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var customTo = new DateTime(2025, 6, 30, 23, 59, 59, DateTimeKind.Utc);

        _orderRepoMock
            .Setup(r => r.GetRevenueByDateAsync(customFrom, customTo, default))
            .ReturnsAsync(Enumerable.Empty<(DateTime, decimal, int)>());

        await _handler.Handle(new GetRevenueByDateQuery(customFrom, customTo), default);

        _orderRepoMock.Verify(r => r.GetRevenueByDateAsync(customFrom, customTo, default), Times.Once);
    }

    [Fact]
    public async Task Handle_DateFormattedAsIso8601()
    {
        var date = new DateTime(2025, 12, 5);
        _orderRepoMock
            .Setup(r => r.GetRevenueByDateAsync(From, To, default))
            .ReturnsAsync(new List<(DateTime, decimal, int)> { (date, 50_000m, 1) });

        var result = await _handler.Handle(new GetRevenueByDateQuery(From, To), default);

        result[0].Date.Should().Be("2025-12-05");
    }

    [Fact]
    public async Task Handle_MultipleRows_PreservesOrderFromRepository()
    {
        var rawRows = new List<(DateTime Date, decimal Revenue, int OrderCount)>
        {
            (new DateTime(2025, 1, 5), 500_000m, 5),
            (new DateTime(2025, 1, 6), 300_000m, 3),
            (new DateTime(2025, 1, 7), 700_000m, 7),
        };

        _orderRepoMock
            .Setup(r => r.GetRevenueByDateAsync(From, To, default))
            .ReturnsAsync(rawRows);

        var result = await _handler.Handle(new GetRevenueByDateQuery(From, To), default);

        result.Should().HaveCount(3);
        result.Select(r => r.Date).Should().ContainInOrder("2025-01-05", "2025-01-06", "2025-01-07");
        result.Select(r => r.Revenue).Should().ContainInOrder(500_000m, 300_000m, 700_000m);
    }
}
