using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Features.Reviews.Commands.AddReviewImages;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;
using Xunit;

namespace SmartShop.Application.Tests.Reviews;

public class AddReviewImagesCommandHandlerTests
{
    private readonly Mock<IReviewRepository> _reviewRepo = new();
    private readonly Mock<IReviewImageRepository> _reviewImageRepo = new();
    private readonly Mock<IFileStorageService> _fileStorage = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private AddReviewImagesCommandHandler CreateHandler() =>
        new(_reviewRepo.Object, _reviewImageRepo.Object, _fileStorage.Object, _uow.Object);

    private static Review CreateReview(Guid userId, Guid productId) =>
        Review.Create(userId, productId, 5, "Great!");

    private static IFormFile CreateMockFormFile(string fileName = "image.jpg")
    {
        var mock = new Mock<IFormFile>();
        mock.Setup(f => f.FileName).Returns(fileName);
        mock.Setup(f => f.Length).Returns(1024);
        mock.Setup(f => f.ContentType).Returns("image/jpeg");
        mock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream([0xFF, 0xD8, 0xFF])); // JPEG magic bytes
        return mock.Object;
    }

    [Fact]
    public async Task Handle_ReviewNotFound_ThrowsNotFoundException()
    {
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var files = new List<IFormFile> { CreateMockFormFile() };

        _reviewRepo.Setup(r => r.GetByIdAsync(reviewId, default))
            .ReturnsAsync((Review?)null);

        var act = () => CreateHandler().Handle(
            new AddReviewImagesCommand(reviewId, userId, files),
            default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_UserNotOwner_ThrowsUnauthorizedException()
    {
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var review = CreateReview(ownerUserId, productId);
        var files = new List<IFormFile> { CreateMockFormFile() };

        _reviewRepo.Setup(r => r.GetByIdAsync(reviewId, default))
            .ReturnsAsync(review);

        var act = () => CreateHandler().Handle(
            new AddReviewImagesCommand(reviewId, userId, files),
            default);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_ExceedMaxImages_ThrowsConflictException()
    {
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var review = CreateReview(userId, productId);
        var files = new List<IFormFile> { CreateMockFormFile(), CreateMockFormFile("img2.jpg") };

        _reviewRepo.Setup(r => r.GetByIdAsync(reviewId, default))
            .ReturnsAsync(review);
        _reviewImageRepo.Setup(r => r.CountByReviewIdAsync(reviewId, default))
            .ReturnsAsync(5); // Already at max

        var act = () => CreateHandler().Handle(
            new AddReviewImagesCommand(reviewId, userId, files),
            default);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_ValidRequest_UploadAndSaveImages()
    {
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var review = CreateReview(userId, productId);
        var files = new List<IFormFile>
        {
            CreateMockFormFile("img1.jpg"),
            CreateMockFormFile("img2.jpg")
        };

        _reviewRepo.Setup(r => r.GetByIdAsync(reviewId, default))
            .ReturnsAsync(review);
        _reviewImageRepo.Setup(r => r.CountByReviewIdAsync(reviewId, default))
            .ReturnsAsync(0); // No existing images
        _fileStorage.Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<Application.Common.Models.UploadCategory>(), default))
            .ReturnsAsync("https://storage.example.com/image.jpg");
        _reviewImageRepo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<ReviewImage>>(), default))
            .Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);

        var result = await CreateHandler().Handle(
            new AddReviewImagesCommand(reviewId, userId, files),
            default);

        result.Should().NotBeNull();
        result.Urls.Should().HaveCount(2);
        _fileStorage.Verify(
            s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<Application.Common.Models.UploadCategory>(), default),
            Times.Exactly(2));
        _reviewImageRepo.Verify(
            r => r.AddRangeAsync(It.IsAny<IEnumerable<ReviewImage>>(), default),
            Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_UploadFailure_RollsbackAndThrows()
    {
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var review = CreateReview(userId, productId);
        var files = new List<IFormFile> { CreateMockFormFile() };

        _reviewRepo.Setup(r => r.GetByIdAsync(reviewId, default))
            .ReturnsAsync(review);
        _reviewImageRepo.Setup(r => r.CountByReviewIdAsync(reviewId, default))
            .ReturnsAsync(0);
        _fileStorage.Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<Application.Common.Models.UploadCategory>(), default))
            .ThrowsAsync(new Exception("Upload failed"));

        var act = () => CreateHandler().Handle(
            new AddReviewImagesCommand(reviewId, userId, files),
            default);

        await act.Should().ThrowAsync<Exception>().WithMessage("Upload failed");
    }
}
