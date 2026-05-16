using FluentAssertions;
using Moq;
using SmartShop.Domain.Common.Exceptions;
using Xunit;
using SmartShop.Application.Features.Combos.Commands.CreateCombo;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Tests.Combos;

public class CreateComboCommandHandlerTests
{
    private readonly Mock<IComboRepository> _comboRepo = new();
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private CreateComboCommandHandler CreateHandler() =>
        new(_comboRepo.Object, _productRepo.Object, _uow.Object);

    private static CreateComboCommand ValidCommand(Guid productId) =>
        new(
            "Test Combo",
            "Test Title",
            "Description",
            "image.jpg",
            99.99m,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(30),
            new List<CreateComboItemRequest>
            {
                new(productId, null, 1)
            }
        );

    private static Product CreateTestProduct(Guid categoryId)
    {
        return Product.Create(
            "Test Product",
            "Description",
            50m,
            categoryId,
            "test-product"
        );
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsComboDto()
    {
        var categoryId = Guid.NewGuid();
        var product = CreateTestProduct(categoryId);
        var command = ValidCommand(product.Id);

        _productRepo.Setup(r => r.GetByIdWithSizesAsync(product.Id, default))
            .ReturnsAsync(product);
        _comboRepo.Setup(r => r.AddAsync(It.IsAny<Combo>(), default))
            .Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await CreateHandler().Handle(command, default);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        var data = result.Data;
        data.Name.Should().Be("Test Combo");
        data.Title.Should().Be("Test Title");
        data.Items.Count.Should().Be(1);
    }

    [Fact]
    public async Task Handle_EmptyItems_ThrowsConflictException()
    {
        var command = new CreateComboCommand(
            "Test Combo",
            "Test Title",
            "Description",
            "image.jpg",
            99.99m,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(30),
            new List<CreateComboItemRequest>() // Empty
        );

        var act = () => CreateHandler().Handle(command, default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*Combo phải có ít nhất 1 sản phẩm*");
    }

    [Fact]
    public async Task Handle_ProductNotFound_ThrowsNotFoundException()
    {
        var productId = Guid.NewGuid();
        var command = ValidCommand(productId);

        _productRepo.Setup(r => r.GetByIdWithSizesAsync(productId, default))
            .ReturnsAsync((Product?)null);

        var act = () => CreateHandler().Handle(command, default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_InactiveProduct_ThrowsConflictException()
    {
        var categoryId = Guid.NewGuid();
        var product = CreateTestProduct(categoryId);
        product.Deactivate();
        var command = ValidCommand(product.Id);

        _productRepo.Setup(r => r.GetByIdWithSizesAsync(product.Id, default))
            .ReturnsAsync(product);

        var act = () => CreateHandler().Handle(command, default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*không hoạt động*");
    }

    [Fact]
    public async Task Handle_SizeNotFound_ThrowsNotFoundException()
    {
        var categoryId = Guid.NewGuid();
        var product = CreateTestProduct(categoryId);
        var sizeId = Guid.NewGuid();

        var command = new CreateComboCommand(
            "Test Combo",
            "Test Title",
            "Description",
            "image.jpg",
            99.99m,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(30),
            new List<CreateComboItemRequest>
            {
                new(product.Id, sizeId, 1) // Size doesn't exist
            }
        );

        _productRepo.Setup(r => r.GetByIdWithSizesAsync(product.Id, default))
            .ReturnsAsync(product);

        var act = () => CreateHandler().Handle(command, default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ValidRequest_CalculatesOriginalPrice()
    {
        var categoryId = Guid.NewGuid();
        var product = CreateTestProduct(categoryId);
        var command = new CreateComboCommand(
            "Test Combo",
            "Test Title",
            "Description",
            "image.jpg",
            99.99m,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(30),
            new List<CreateComboItemRequest>
            {
                new(product.Id, null, 2)
            }
        );

        _productRepo.Setup(r => r.GetByIdWithSizesAsync(product.Id, default))
            .ReturnsAsync(product);
        _comboRepo.Setup(r => r.AddAsync(It.IsAny<Combo>(), default))
            .Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await CreateHandler().Handle(command, default);

        result.Data.Should().NotBeNull();
        result.Data.OriginalPrice.Should().Be(100m); // 50 * 2
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsRepository()
    {
        var categoryId = Guid.NewGuid();
        var product = CreateTestProduct(categoryId);
        var command = ValidCommand(product.Id);

        _productRepo.Setup(r => r.GetByIdWithSizesAsync(product.Id, default))
            .ReturnsAsync(product);
        _comboRepo.Setup(r => r.AddAsync(It.IsAny<Combo>(), default))
            .Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        await CreateHandler().Handle(command, default);

        _comboRepo.Verify(r => r.AddAsync(It.IsAny<Combo>(), default), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_MultipleItems_CalculatesOriginalPriceCorrectly()
    {
        var categoryId = Guid.NewGuid();
        var product1 = CreateTestProduct(categoryId); // price = 50m
        var product2 = Product.Create("Product2", "Desc", 75m, categoryId, "product2-slug"); // price = 75m

        var command = new CreateComboCommand(
            "Multi Combo",
            "Multi Title",
            "Description",
            "image.jpg",
            199.99m,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(30),
            new List<CreateComboItemRequest>
            {
                new(product1.Id, null, 2), // 50 * 2 = 100
                new(product2.Id, null, 3)  // 75 * 3 = 225
            }
        );

        _productRepo.Setup(r => r.GetByIdWithSizesAsync(product1.Id, default))
            .ReturnsAsync(product1);
        _productRepo.Setup(r => r.GetByIdWithSizesAsync(product2.Id, default))
            .ReturnsAsync(product2);
        _comboRepo.Setup(r => r.AddAsync(It.IsAny<Combo>(), default))
            .Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await CreateHandler().Handle(command, default);

        result.Data.Should().NotBeNull();
        result.Data.OriginalPrice.Should().Be(325m); // 100 + 225
    }

    [Fact]
    public async Task Handle_EndsAtBeforeStartsAt_ThrowsException()
    {
        var categoryId = Guid.NewGuid();
        var product = CreateTestProduct(categoryId);
        var startsAt = DateTime.UtcNow.AddDays(5);
        var endsAt = DateTime.UtcNow.AddDays(2); // Before startsAt

        var command = new CreateComboCommand(
            "Test Combo",
            "Test Title",
            "Description",
            "image.jpg",
            99.99m,
            startsAt,
            endsAt,
            new List<CreateComboItemRequest>
            {
                new(product.Id, null, 1)
            }
        );

        var act = () => CreateHandler().Handle(command, default);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*EndsAt must be greater than StartsAt*");
    }
}
