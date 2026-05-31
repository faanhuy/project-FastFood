using FluentValidation;
using SmartShop.Application.Common.Models;

namespace SmartShop.Application.Features.Reviews.Commands.AddReviewImages;

public class AddReviewImagesCommandValidator : AbstractValidator<AddReviewImagesCommand>
{
    private const int MaxFiles = 5;
    private const long MaxFileSize = 3 * 1024 * 1024; // 3 MB
    private static readonly string[] AllowedMimeTypes = ["image/jpeg", "image/png", "image/webp"];

    public AddReviewImagesCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty().WithMessage("validation.review_id_invalid");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("validation.user_id_invalid");

        RuleFor(x => x.Files)
            .NotEmpty().WithMessage("validation.review_images_required");

        RuleFor(x => x.Files)
            .Must(files => files.Count <= MaxFiles)
            .WithMessage("validation.review_images_max");

        RuleForEach(x => x.Files)
            .Must(file => file.Length <= MaxFileSize)
            .WithMessage("validation.review_image_size");

        RuleForEach(x => x.Files)
            .Must(file => AllowedMimeTypes.Contains(file.ContentType))
            .WithMessage("validation.review_image_format");
    }
}
