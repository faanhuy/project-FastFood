using FluentAssertions;
using Moq;
using SmartShop.Application.Features.Orders;
using SmartShop.Application.Features.Orders.Queries.GetMyOrders;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;
using Xunit;

namespace SmartShop.Application.Tests.Orders;

public class GetMyOrdersQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepo = new();

    private GetMyOrdersQueryHandler CreateHandler() => new(_orderRepo.Object);

    [Fact]
    public async Task Handle_WithOrders_ReturnsPagedResult()
    {
        var userId = Guid.NewGuid();
        var order = Order.Create(userId, "123 Main St");
        _orderRepo.Setup(r => r.GetPagedByUserIdAsync(userId, 1, 10, default))
                  .ReturnsAsync((new List<Order> { order }, 1));

        var result = await CreateHandler().Handle(new GetMyOrdersQuery(userId, 1, 10), default);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_NoOrders_ReturnsEmptyPagedResult()
    {
        var userId = Guid.NewGuid();
        _orderRepo.Setup(r => r.GetPagedByUserIdAsync(userId, 1, 10, default))
                  .ReturnsAsync((new List<Order>(), 0));

        var result = await CreateHandler().Handle(new GetMyOrdersQuery(userId, 1, 10), default);

        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MapsOrderStatusCorrectly()
    {
        var userId = Guid.NewGuid();
        var order = Order.Create(userId, "123 Main St");
        _orderRepo.Setup(r => r.GetPagedByUserIdAsync(userId, 1, 10, default))
                  .ReturnsAsync((new List<Order> { order }, 1));

        var result = await CreateHandler().Handle(new GetMyOrdersQuery(userId, 1, 10), default);

        result.Items.First().Status.Should().Be("Pending");
    }
}
