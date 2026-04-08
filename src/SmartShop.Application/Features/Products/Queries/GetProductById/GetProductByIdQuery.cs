using MediatR;
using SmartShop.Application.DTOs;

namespace SmartShop.Application.Products.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;
