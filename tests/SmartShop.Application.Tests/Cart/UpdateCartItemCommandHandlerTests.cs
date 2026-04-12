using FluentAssertions;
using Moq;
using SmartShop.Application.Features.Cart.Commands.UpdateCartItem;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;
using Xunit;
using CartEntity = SmartShop.Domain.Entities.Cart;

namespace SmartShop.Application.Tests.Cart;

public class UpdateCartItemCommandHandlerTests
{
    private readonly Mock<ICartRepository> _cartRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private UpdateCartItemCommandHandler CreateHandler() =>
        new(_cartRepo.Object, _uow.Object);

    [Fact]
    public async Task Handle_CartNotFound_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        _cartRepo.Setup(r => r.GetByUserIdAsync(userId, default)).ReturnsAsync((CartEntity?)null);

        var act = () => CreateHandler().Handle(
            new UpdateCartItemCommand(userId, Guid.NewGuid(), 3), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ValidRequest_SavesChanges()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cart = CartEntity.Create(userId);
        cart.AddItem(productId, 1, 50m);

        var callCount = 0;
        _cartRepo.Setup(r => r.GetByUserIdAsync(userId, default))
                 .ReturnsAsync(() => callCount++ == 0 ? cart : CartEntity.Create(userId));
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        await CreateHandler().Handle(new UpdateCartItemCommand(userId, productId, 5), default);

        _uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsUpdatedCartDto()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cart = CartEntity.Create(userId);
        cart.AddItem(productId, 1, 50m);

        var callCount = 0;
        _cartRepo.Setup(r => r.GetByUserIdAsync(userId, default))
                 .ReturnsAsync(() => callCount++ == 0 ? cart : CartEntity.Create(userId));
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await CreateHandler().Handle(
            new UpdateCartItemCommand(userId, productId, 5), default);

        result.Should().NotBeNull();
    }
}
