using MediatR;
using Microsoft.AspNetCore.Http;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Common.Utilities;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Reviews.Commands.AddReviewImages;

public class AddReviewImagesCommandHandler(
    IReviewRepository reviewRepository,
    IReviewImageRepository reviewImageRepository,
    IFileStorageService fileStorage,
    IUnitOfWork unitOfWork
) : IRequestHandler<AddReviewImagesCommand, AddReviewImagesResult>
{
    private const int MaxImagesPerReview = 5;

    public async Task<AddReviewImagesResult> Handle(
        AddReviewImagesCommand request,
        CancellationToken cancellationToken)
    {
        // Kiểm tra review tồn tại
        var review = await reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken)
            ?? throw new NotFoundException(nameof(Review), request.ReviewId);

        // Kiểm tra ownership
        if (review.UserId != request.UserId)
            throw new UnauthorizedException("error.review_image_unauthorized", null);

        // Kiểm tra giới hạn ảnh (hiện có + mới thêm)
        var existingCount = await reviewImageRepository.CountByReviewIdAsync(request.ReviewId, cancellationToken);
        if (existingCount + request.Files.Count > MaxImagesPerReview)
            throw new ConflictException("error.review_image_max_exceeded",
                new Dictionary<string, string>
                {
                    ["existing"] = existingCount.ToString(),
                    ["max"] = MaxImagesPerReview.ToString()
                });

        var config = UploadCategoryConfigProvider.Get(UploadCategory.ReviewImage);
        var uploadedUrls = new List<string>();
        var newImages = new List<ReviewImage>();

        try
        {
            // Upload từng file
            foreach (var file in request.Files)
            {
                // Validate magic bytes
                if (!FileSecurityHelper.ValidateMagicBytes(file.OpenReadStream(), config.AllowedMimeTypes))
                    throw new ConflictException("error.invalid_file_type", new Dictionary<string, string> { ["fileName"] = file.FileName });

                var safeFileName = FileSecurityHelper.BuildStorageFileName(file.FileName, request.ReviewId.ToString());

                var url = await fileStorage.UploadAsync(
                    file.OpenReadStream(),
                    safeFileName,
                    UploadCategory.ReviewImage,
                    cancellationToken
                );

                uploadedUrls.Add(url);
                var displayOrder = existingCount + newImages.Count;
                newImages.Add(ReviewImage.Create(request.ReviewId, url, displayOrder));
            }

            // Lưu tất cả images vào DB
            await reviewImageRepository.AddRangeAsync(newImages, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new AddReviewImagesResult(uploadedUrls);
        }
        catch
        {
            // Nếu có lỗi, xóa các file đã upload (best effort)
            foreach (var url in uploadedUrls)
            {
                try
                {
                    await fileStorage.DeleteAsync(url, UploadCategory.ReviewImage, cancellationToken);
                }
                catch
                {
                    // Silent fail — log nếu cần
                }
            }

            throw;
        }
    }
}
