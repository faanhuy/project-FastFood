using FluentAssertions;
using Moq;
using Xunit;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Features.Wishlist.Queries.GetWishlist;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Tests.Wishlist;

public class GetWishlistQueryHandlerTests
{
    private readonly Mock<IWishlistRepository> _wishlistRepo = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    private static readonly string UserId = Guid.NewGuid().ToString();

    public GetWishlistQueryHandlerTests()
    {
        _currentUser.Setup(s => s.UserId).Returns(UserId);
    }

    private GetWishlistQueryHandler CreateHandler() =>
        new(_wishlistRepo.Object, _currentUser.Object);

    [Fact]
    public async Task Handle_EmptyWishlist_ReturnsEmptyList()
    {
        _wishlistRepo.Setup(r => r.GetByUserIdAsync(UserId, default))
                     .ReturnsAsync(new List<WishlistItem>());

        var result = await CreateHandler().Handle(new GetWishlistQuery(), default);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WishlistWithItems_ReturnsDtoList()
    {
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        var item1 = WishlistItem.Create(UserId, productId1);
        var item2 = WishlistItem.Create(UserId, productId2);

        _wishlistRepo.Setup(r => r.GetByUserIdAsync(UserId, default))
                     .ReturnsAsync(new List<WishlistItem> { item1, item2 });

        var result = await CreateHandler().Handle(new GetWishlistQuery(), default);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WishlistItems_ContainCorrectProductIds()
    {
        var productId = Guid.NewGuid();
        var item = WishlistItem.Create(UserId, productId);

        _wishlistRepo.Setup(r => r.GetByUserIdAsync(UserId, default))
                     .ReturnsAsync(new List<WishlistItem> { item });

        var result = await CreateHandler().Handle(new GetWishlistQuery(), default);

        result.Data!.Should().ContainSingle()
            .Which.ProductId.Should().Be(productId);
    }

    [Fact]
    public async Task Handle_ItemWithNullProduct_ReturnsDefaults()
    {
        var productId = Guid.NewGuid();
        var item = WishlistItem.Create(UserId, productId);
        // Product navigation property is null (not eagerly loaded)

        _wishlistRepo.Setup(r => r.GetByUserIdAsync(UserId, default))
                     .ReturnsAsync(new List<WishlistItem> { item });

        var result = await CreateHandler().Handle(new GetWishlistQuery(), default);

        var dto = result.Data!.Single();
        dto.ProductName.Should().Be(string.Empty);
        dto.Price.Should().Be(0m);
        dto.ImageUrl.Should().BeNull();
        dto.IsInStock.Should().BeFalse();
        dto.Slug.Should().Be(string.Empty);
    }

    [Fact]
    public async Task Handle_CallsRepositoryWithCurrentUserId()
    {
        _wishlistRepo.Setup(r => r.GetByUserIdAsync(UserId, default))
                     .ReturnsAsync(new List<WishlistItem>());

        await CreateHandler().Handle(new GetWishlistQuery(), default);

        _wishlistRepo.Verify(r => r.GetByUserIdAsync(UserId, default), Times.Once);
    }
}
