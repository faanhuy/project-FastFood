using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Common.Utilities;

namespace SmartShop.Application.Features.Products.Commands.UploadProductImage;

public class UploadProductImageCommandHandler(
    IFileStorageService fileStorage
) : IRequestHandler<UploadProductImageCommand, UploadProductImageResult>
{
    public async Task<UploadProductImageResult> Handle(
        UploadProductImageCommand request,
        CancellationToken cancellationToken)
    {
        var safeFileName = FileSecurityHelper.BuildStorageFileName(request.File.FileName);

        var url = await fileStorage.UploadAsync(
            request.File.OpenReadStream(),
            safeFileName,
            UploadCategory.ProductImage,
            cancellationToken
        );

        return new UploadProductImageResult(url);
    }
}
