using MediatR;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Products.Commands.SetPrimaryImage;

public class SetPrimaryImageCommandHandler(
    IProductImageRepository imageRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<SetPrimaryImageCommand>
{
    public async Task Handle(SetPrimaryImageCommand request, CancellationToken cancellationToken)
    {
        var targetImage = await imageRepository.GetByIdAsync(request.ImageId, cancellationToken)
            ?? throw new NotFoundException(nameof(ProductImage), request.ImageId);

        if (targetImage.ProductId != request.ProductId)
            throw new UnauthorizedException("error.image_product_mismatch", null);

        // Get all images for this product
        var allImages = await imageRepository.GetByProductIdAsync(request.ProductId, cancellationToken);

        // Unset all as primary
        foreach (var img in allImages)
        {
            img.UnsetPrimary();
        }

        // Set target as primary
        targetImage.SetAsPrimary();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
