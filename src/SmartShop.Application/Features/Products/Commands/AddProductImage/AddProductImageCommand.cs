using MediatR;

namespace SmartShop.Application.Features.Products.Commands.AddProductImage;

public record AddProductImageCommand(
    Guid ProductId,
    string ImageUrl,
    bool IsPrimary = false,
    int SortOrder = 0) : IRequest<ProductImageDto>;
