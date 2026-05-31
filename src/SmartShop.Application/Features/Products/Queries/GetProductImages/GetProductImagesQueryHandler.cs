using MediatR;
using SmartShop.Application.Features.Products.Commands.AddProductImage;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Products.Queries.GetProductImages;

public class GetProductImagesQueryHandler(IProductImageRepository imageRepository)
    : IRequestHandler<GetProductImagesQuery, List<ProductImageDto>>
{
    public async Task<List<ProductImageDto>> Handle(GetProductImagesQuery request, CancellationToken cancellationToken)
    {
        var images = await imageRepository.GetByProductIdAsync(request.ProductId, cancellationToken);
        return images
            .Select(img => new ProductImageDto(img.Id, img.ProductId, img.ImageUrl, img.IsPrimary, img.SortOrder))
            .ToList();
    }
}
