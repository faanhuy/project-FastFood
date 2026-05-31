using MediatR;
using SmartShop.Application.Features.Products.Commands.AddProductImage;

namespace SmartShop.Application.Features.Products.Queries.GetProductImages;

public record GetProductImagesQuery(Guid ProductId) : IRequest<List<ProductImageDto>>;
