using FluentValidation;
using Microsoft.AspNetCore.Http;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Common.Utilities;

namespace SmartShop.Application.Features.Products.Commands.UploadProductImage;

public class UploadProductImageCommandValidator : AbstractValidator<UploadProductImageCommand>
{
    public UploadProductImageCommandValidator()
    {
        var config = UploadCategoryConfigProvider.Get(UploadCategory.ProductImage);

        RuleFor(x => x.File)
            .NotNull().WithMessage("Vui lòng chọn file ảnh.")
            .Must(f => f != null && f.Length > 0).WithMessage("File không được rỗng.")
            .Must(f => f == null || f.Length <= config.MaxSizeBytes)
                .WithMessage($"Kích thước file không được vượt quá {config.MaxSizeBytes / (1024 * 1024)} MB.")
            .Must(f => f == null || FileSecurityHelper.ValidateMagicBytes(f.OpenReadStream(), config.AllowedMimeTypes))
                .WithMessage("Định dạng file không hợp lệ. Chỉ chấp nhận JPG, PNG, WebP.");
    }
}
