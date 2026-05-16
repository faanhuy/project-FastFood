using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Features.Products.Commands.BulkImportProducts;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;
using Xunit;

namespace SmartShop.Application.Tests.Products;

public class BulkImportProductsCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();

    private BulkImportProductsCommandHandler CreateHandler() =>
        new(_productRepoMock.Object, _categoryRepoMock.Object, _uowMock.Object, _cacheMock.Object);

    private static IFormFile CreateCsvFile(string csvContent)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(bytes);
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
        mockFile.Setup(f => f.FileName).Returns("import.csv");
        return mockFile.Object;
    }

    private static Category CreateCategory(string name = "Electronics") =>
        Category.Create(name, "electronics-slug", null);

    [Fact]
    public async Task Handle_ValidRows_CreatesProducts()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var csvContent = "Name,Description,Price,OriginalPrice,CategoryId,Slug,ImageUrl\n" +
                        $"Product1,Desc1,100,150,{categoryId},product1,\n" +
                        $"Product2,Desc2,200,250,{categoryId},product2,";
        var file = CreateCsvFile(csvContent);
        var command = new BulkImportProductsCommand(file);

        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(categoryId, default))
            .ReturnsAsync(CreateCategory());
        _productRepoMock
            .Setup(r => r.GetBySlugAsync(It.IsAny<string>(), default))
            .ReturnsAsync((Product?)null);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _cacheMock.Setup(c => c.RemoveByPrefixAsync(It.IsAny<string>(), default)).Returns(Task.CompletedTask);

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        result.Created.Should().Be(2);
        result.Failed.Should().Be(0);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_EmptyName_FailsRow()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var csvContent = "Name,Description,Price,OriginalPrice,CategoryId,Slug,ImageUrl\n" +
                        $",Desc1,100,150,{categoryId},product1,";
        var file = CreateCsvFile(csvContent);
        var command = new BulkImportProductsCommand(file);

        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(categoryId, default))
            .ReturnsAsync(CreateCategory());

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        result.Created.Should().Be(0);
        result.Failed.Should().Be(1);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Field.Should().Be("Name");
    }

    [Fact]
    public async Task Handle_ZeroPrice_FailsRow()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var csvContent = "Name,Description,Price,OriginalPrice,CategoryId,Slug,ImageUrl\n" +
                        $"Product1,Desc1,0,150,{categoryId},product1,";
        var file = CreateCsvFile(csvContent);
        var command = new BulkImportProductsCommand(file);

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        result.Created.Should().Be(0);
        result.Failed.Should().Be(1);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Field.Should().Be("Price");
    }

    [Fact]
    public async Task Handle_InvalidCategoryId_FailsRow()
    {
        // Arrange
        var csvContent = "Name,Description,Price,OriginalPrice,CategoryId,Slug,ImageUrl\n" +
                        "Product1,Desc1,100,150,not-a-guid,product1,";
        var file = CreateCsvFile(csvContent);
        var command = new BulkImportProductsCommand(file);

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        result.Created.Should().Be(0);
        result.Failed.Should().Be(1);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Field.Should().Be("CategoryId");
    }

    [Fact]
    public async Task Handle_CategoryNotFound_FailsRow()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var csvContent = "Name,Description,Price,OriginalPrice,CategoryId,Slug,ImageUrl\n" +
                        $"Product1,Desc1,100,150,{categoryId},product1,";
        var file = CreateCsvFile(csvContent);
        var command = new BulkImportProductsCommand(file);

        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(categoryId, default))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        result.Created.Should().Be(0);
        result.Failed.Should().Be(1);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Field.Should().Be("CategoryId");
    }

    [Fact]
    public async Task Handle_MixedValidInvalid_ReturnsSummary()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var csvContent = "Name,Description,Price,OriginalPrice,CategoryId,Slug,ImageUrl\n" +
                        $"Product1,Desc1,100,150,{categoryId},product1,\n" +
                        "Product2,Desc2,0,150,{invalid},product2,\n" +
                        $"Product3,Desc3,200,250,{categoryId},product3,";
        var file = CreateCsvFile(csvContent);
        var command = new BulkImportProductsCommand(file);

        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(categoryId, default))
            .ReturnsAsync(CreateCategory());
        _productRepoMock
            .Setup(r => r.GetBySlugAsync(It.IsAny<string>(), default))
            .ReturnsAsync((Product?)null);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _cacheMock.Setup(c => c.RemoveByPrefixAsync(It.IsAny<string>(), default)).Returns(Task.CompletedTask);

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        result.Created.Should().Be(2);
        result.Failed.Should().Be(1);
    }

    [Fact]
    public async Task Handle_EmptySlug_AutoGeneratesSlug()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var csvContent = "Name,Description,Price,OriginalPrice,CategoryId,Slug,ImageUrl\n" +
                        $"Product1,Desc1,100,150,{categoryId},,";
        var file = CreateCsvFile(csvContent);
        var command = new BulkImportProductsCommand(file);

        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(categoryId, default))
            .ReturnsAsync(CreateCategory());
        _productRepoMock
            .Setup(r => r.GetBySlugAsync(It.IsAny<string>(), default))
            .ReturnsAsync((Product?)null);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _cacheMock.Setup(c => c.RemoveByPrefixAsync(It.IsAny<string>(), default)).Returns(Task.CompletedTask);

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        result.Created.Should().Be(1);
        result.Failed.Should().Be(0);
        _productRepoMock.Verify(
            r => r.AddAsync(It.Is<Product>(p => !string.IsNullOrEmpty(p.Slug)), default),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoSuccessfulRows_NoSaveChanges()
    {
        // Arrange
        var csvContent = "Name,Description,Price,OriginalPrice,CategoryId,Slug,ImageUrl\n" +
                        "Product1,Desc1,0,150,invalid-guid,product1,";
        var file = CreateCsvFile(csvContent);
        var command = new BulkImportProductsCommand(file);

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        result.Created.Should().Be(0);
        result.Failed.Should().Be(1);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task Handle_SuccessfulImport_ClearsProductCache()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var csvContent = "Name,Description,Price,OriginalPrice,CategoryId,Slug,ImageUrl\n" +
                        $"Product1,Desc1,100,150,{categoryId},product1,";
        var file = CreateCsvFile(csvContent);
        var command = new BulkImportProductsCommand(file);

        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(categoryId, default))
            .ReturnsAsync(CreateCategory());
        _productRepoMock
            .Setup(r => r.GetBySlugAsync(It.IsAny<string>(), default))
            .ReturnsAsync((Product?)null);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _cacheMock.Setup(c => c.RemoveByPrefixAsync("products:list:", default)).Returns(Task.CompletedTask);

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        _cacheMock.Verify(
            c => c.RemoveByPrefixAsync("products:list:", default),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateSlugDetected_ModifiesSlug()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var csvContent = "Name,Description,Price,OriginalPrice,CategoryId,Slug,ImageUrl\n" +
                        $"Product1,Desc1,100,150,{categoryId},duplicate-slug,";
        var file = CreateCsvFile(csvContent);
        var command = new BulkImportProductsCommand(file);

        var existingProduct = Product.Create("Existing", "Desc", 100, categoryId, "duplicate-slug");
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(categoryId, default))
            .ReturnsAsync(CreateCategory());
        _productRepoMock
            .Setup(r => r.GetBySlugAsync("duplicate-slug", default))
            .ReturnsAsync(existingProduct);
        _productRepoMock
            .Setup(r => r.GetBySlugAsync(It.IsNotIn("duplicate-slug"), default))
            .ReturnsAsync((Product?)null);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _cacheMock.Setup(c => c.RemoveByPrefixAsync(It.IsAny<string>(), default)).Returns(Task.CompletedTask);

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        result.Created.Should().Be(1);
        result.Failed.Should().Be(0);
    }

    [Fact]
    public async Task Handle_NegativePrice_FailsRow()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var csvContent = "Name,Description,Price,OriginalPrice,CategoryId,Slug,ImageUrl\n" +
                        $"Product1,Desc1,-100,150,{categoryId},product1,";
        var file = CreateCsvFile(csvContent);
        var command = new BulkImportProductsCommand(file);

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        result.Created.Should().Be(0);
        result.Failed.Should().Be(1);
        result.Errors[0].Field.Should().Be("Price");
    }
}
