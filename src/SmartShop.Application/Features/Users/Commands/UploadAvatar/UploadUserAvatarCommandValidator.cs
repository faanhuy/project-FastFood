using FluentValidation;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Common.Utilities;

namespace SmartShop.Application.Features.Users.Commands.UploadAvatar;

public class UploadUserAvatarCommandValidator : AbstractValidator<UploadUserAvatarCommand>
{
    public UploadUserAvatarCommandValidator()
    {
        var config = UploadCategoryConfigProvider.Get(UploadCategory.UserAvatar);

        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.File)
            .NotNull().WithMessage("validation.file_required")
            .Must(f => f != null && f.Length > 0).WithMessage("validation.file_empty")
            .Must(f => f == null || f.Length <= config.MaxSizeBytes)
                .WithMessage($"Kích thước file không được vượt quá {config.MaxSizeBytes / (1024 * 1024)} MB.")
            .Must(f => f == null || FileSecurityHelper.ValidateMagicBytes(f.OpenReadStream(), config.AllowedMimeTypes))
                .WithMessage("validation.file_format_invalid");
    }
}
