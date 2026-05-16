using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Common.Utilities;

namespace SmartShop.Application.Features.Combos.Commands.UploadComboImage;

public class UploadComboImageCommandHandler(
    IFileStorageService fileStorage
) : IRequestHandler<UploadComboImageCommand, UploadComboImageResult>
{
    public async Task<UploadComboImageResult> Handle(
        UploadComboImageCommand request,
        CancellationToken cancellationToken)
    {
        var safeFileName = FileSecurityHelper.BuildStorageFileName(request.File.FileName);

        var url = await fileStorage.UploadAsync(
            request.File.OpenReadStream(),
            safeFileName,
            UploadCategory.ComboImage,
            cancellationToken
        );

        return new UploadComboImageResult(url);
    }
}
