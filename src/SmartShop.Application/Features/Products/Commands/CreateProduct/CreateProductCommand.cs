using MediatR;
using SmartShop.Application.DTOs;

namespace SmartShop.Application.Products.Commands.CreateProduct;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    int Stock,
    Guid CategoryId,
    string Slug
) : IRequest<ProductDto>;
