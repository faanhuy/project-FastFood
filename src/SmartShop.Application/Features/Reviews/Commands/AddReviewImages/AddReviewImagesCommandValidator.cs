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
            .NotEmpty().WithMessage("ReviewId không hợp lệ.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId không hợp lệ.");

        RuleFor(x => x.Files)
            .NotEmpty().WithMessage("Phải chọn ít nhất một tệp.");

        RuleFor(x => x.Files)
            .Must(files => files.Count <= MaxFiles)
            .WithMessage($"Tối đa {MaxFiles} ảnh được phép.");

        RuleForEach(x => x.Files)
            .Must(file => file.Length <= MaxFileSize)
            .WithMessage($"Mỗi ảnh phải nhỏ hơn {MaxFileSize / (1024 * 1024)}MB.");

        RuleForEach(x => x.Files)
            .Must(file => AllowedMimeTypes.Contains(file.ContentType))
            .WithMessage("Chỉ chấp nhận JPEG, PNG, WebP.");
    }
}
