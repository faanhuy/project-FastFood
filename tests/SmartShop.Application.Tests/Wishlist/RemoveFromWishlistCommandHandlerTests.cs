using FluentAssertions;
using Moq;
using Xunit;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Features.Wishlist.Commands.RemoveFromWishlist;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Tests.Wishlist;

public class RemoveFromWishlistCommandHandlerTests
{
    private readonly Mock<IWishlistRepository> _wishlistRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    private static readonly string UserId = Guid.NewGuid().ToString();

    public RemoveFromWishlistCommandHandlerTests()
    {
        _currentUser.Setup(s => s.UserId).Returns(UserId);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
    }

    private RemoveFromWishlistCommandHandler CreateHandler() =>
        new(_wishlistRepo.Object, _uow.Object, _currentUser.Object);

    private static WishlistItem CreateWishlistItem(Guid productId) =>
        WishlistItem.Create(UserId, productId);

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        var productId = Guid.NewGuid();
        var item = CreateWishlistItem(productId);

        _wishlistRepo.Setup(r => r.GetByUserAndProductAsync(UserId, productId, default))
                     .ReturnsAsync(item);

        var result = await CreateHandler().Handle(new RemoveFromWishlistCommand(productId), default);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidCommand_RemovesItemAndSaves()
    {
        var productId = Guid.NewGuid();
        var item = CreateWishlistItem(productId);

        _wishlistRepo.Setup(r => r.GetByUserAndProductAsync(UserId, productId, default))
                     .ReturnsAsync(item);

        await CreateHandler().Handle(new RemoveFromWishlistCommand(productId), default);

        _wishlistRepo.Verify(r => r.RemoveAsync(item), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_ItemNotFound_ThrowsNotFoundException()
    {
        var productId = Guid.NewGuid();
        _wishlistRepo.Setup(r => r.GetByUserAndProductAsync(UserId, productId, default))
                     .ReturnsAsync((WishlistItem?)null);

        var act = () => CreateHandler().Handle(new RemoveFromWishlistCommand(productId), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ItemNotFound_DoesNotCallSave()
    {
        var productId = Guid.NewGuid();
        _wishlistRepo.Setup(r => r.GetByUserAndProductAsync(UserId, productId, default))
                     .ReturnsAsync((WishlistItem?)null);

        var act = () => CreateHandler().Handle(new RemoveFromWishlistCommand(productId), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }
}
