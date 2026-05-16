using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Features.Products.Commands.UploadProductImage;
using Xunit;

namespace SmartShop.Application.Tests.Products;

public class UploadProductImageCommandHandlerTests
{
    private readonly Mock<IFileStorageService> _fileStorageMock = new();

    private UploadProductImageCommandHandler CreateHandler() =>
        new(_fileStorageMock.Object);

    private static IFormFile CreateMockFormFile(string fileName = "product.jpg")
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[] { 0xFF, 0xD8, 0xFF }));
        return mockFile.Object;
    }

    [Fact]
    public async Task Handle_ValidFile_UploadsAndReturnsUrl()
    {
        // Arrange
        var file = CreateMockFormFile("product.jpg");
        var expectedUrl = "/uploads/products/abc123.jpg";
        var command = new UploadProductImageCommand(file);

        _fileStorageMock
            .Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), UploadCategory.ProductImage, default))
            .ReturnsAsync(expectedUrl);

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.Url.Should().Be(expectedUrl);
        _fileStorageMock.Verify(
            s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), UploadCategory.ProductImage, default),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DifferentFileName_PassesSafeFileName()
    {
        // Arrange
        var file = CreateMockFormFile("my product image.jpg");
        var command = new UploadProductImageCommand(file);

        _fileStorageMock
            .Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), UploadCategory.ProductImage, default))
            .ReturnsAsync("/uploads/products/test.jpg");

        // Act
        await CreateHandler().Handle(command, default);

        // Assert
        _fileStorageMock.Verify(
            s => s.UploadAsync(
                It.IsAny<Stream>(),
                It.Is<string>(name => !name.Contains(" ")), // Verify spaces are removed
                UploadCategory.ProductImage,
                default),
            Times.Once);
    }

    [Fact]
    public async Task Handle_MultipleFiles_EachUploadsIndependently()
    {
        // Arrange
        var file1 = CreateMockFormFile("image1.jpg");
        var file2 = CreateMockFormFile("image2.png");

        _fileStorageMock
            .Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), UploadCategory.ProductImage, default))
            .ReturnsAsync("/uploads/products/file.jpg");

        // Act
        var result1 = await CreateHandler().Handle(new UploadProductImageCommand(file1), default);
        var result2 = await CreateHandler().Handle(new UploadProductImageCommand(file2), default);

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        _fileStorageMock.Verify(
            s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), UploadCategory.ProductImage, default),
            Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_FileWithoutExtension_StoresWithBinExtension()
    {
        // Arrange
        var file = CreateMockFormFile("productimage");
        var command = new UploadProductImageCommand(file);

        _fileStorageMock
            .Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), UploadCategory.ProductImage, default))
            .ReturnsAsync("/uploads/products/test.bin");

        // Act
        await CreateHandler().Handle(command, default);

        // Assert
        _fileStorageMock.Verify(
            s => s.UploadAsync(
                It.IsAny<Stream>(),
                It.Is<string>(name => name.EndsWith(".bin")),
                UploadCategory.ProductImage,
                default),
            Times.Once);
    }

    [Fact]
    public async Task Handle_UploadCategoryAlwaysProductImage()
    {
        // Arrange
        var file = CreateMockFormFile("product.jpg");
        var command = new UploadProductImageCommand(file);

        _fileStorageMock
            .Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), UploadCategory.ProductImage, default))
            .ReturnsAsync("/uploads/products/test.jpg");

        // Act
        await CreateHandler().Handle(command, default);

        // Assert
        _fileStorageMock.Verify(
            s => s.UploadAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                UploadCategory.ProductImage,
                default),
            Times.Once);
    }
}
