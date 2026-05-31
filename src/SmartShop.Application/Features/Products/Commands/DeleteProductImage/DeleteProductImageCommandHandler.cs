using MediatR;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Products.Commands.DeleteProductImage;

public class DeleteProductImageCommandHandler(
    IProductImageRepository imageRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteProductImageCommand>
{
    public async Task Handle(DeleteProductImageCommand request, CancellationToken cancellationToken)
    {
        var image = await imageRepository.GetByIdAsync(request.ImageId, cancellationToken)
            ?? throw new NotFoundException(nameof(ProductImage), request.ImageId);

        if (image.ProductId != request.ProductId)
            throw new UnauthorizedException("error.image_product_mismatch");

        imageRepository.Remove(image);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
