using MediatR;
using SmartShop.Application.DTOs;

namespace SmartShop.Application.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string? ImageUrl,
    decimal? OriginalPrice = null
) : IRequest<ProductDto>;
