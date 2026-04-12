using FluentAssertions;
using Moq;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Application.Features.Orders.Queries.GetOrderById;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;
using Xunit;

namespace SmartShop.Application.Tests.Orders;

public class GetOrderByIdQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepo = new();

    private GetOrderByIdQueryHandler CreateHandler() => new(_orderRepo.Object);

    [Fact]
    public async Task Handle_OrderNotFound_ThrowsNotFoundException()
    {
        var orderId = Guid.NewGuid();
        _orderRepo.Setup(r => r.GetByIdWithItemsAsync(orderId, default)).ReturnsAsync((Order?)null);

        var act = () => CreateHandler().Handle(
            new GetOrderByIdQuery(Guid.NewGuid(), orderId), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WrongUser_ThrowsUnauthorizedException()
    {
        var orderId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var order = Order.Create(ownerId, "123 Main St");
        _orderRepo.Setup(r => r.GetByIdWithItemsAsync(orderId, default)).ReturnsAsync(order);

        var act = () => CreateHandler().Handle(
            new GetOrderByIdQuery(Guid.NewGuid(), orderId), default); // different userId

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsOrderDto()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var order = Order.Create(userId, "123 Main St", "Please deliver fast");
        _orderRepo.Setup(r => r.GetByIdWithItemsAsync(orderId, default)).ReturnsAsync(order);

        var result = await CreateHandler().Handle(new GetOrderByIdQuery(userId, orderId), default);

        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.ShippingAddress.Should().Be("123 Main St");
        result.Notes.Should().Be("Please deliver fast");
    }
}
