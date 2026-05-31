using MediatR;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Products.Commands.AddProductImage;

public class AddProductImageCommandHandler(
    IProductRepository productRepository,
    IProductImageRepository imageRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<AddProductImageCommand, ProductImageDto>
{
    public async Task<ProductImageDto> Handle(AddProductImageCommand request, CancellationToken cancellationToken)
    {
        // Verify product exists
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        // If this image is marked as primary, unset all other primary images
        if (request.IsPrimary)
        {
            var existingImages = await imageRepository.GetByProductIdAsync(request.ProductId, cancellationToken);
            foreach (var img in existingImages.Where(x => x.IsPrimary))
            {
                img.UnsetPrimary();
            }
        }

        var image = ProductImage.Create(request.ProductId, request.ImageUrl, request.IsPrimary, request.SortOrder);
        await imageRepository.AddAsync(image, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProductImageDto(image.Id, image.ProductId, image.ImageUrl, image.IsPrimary, image.SortOrder);
    }
}
