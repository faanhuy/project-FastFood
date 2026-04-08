using MediatR;
using SmartShop.Application.DTOs;

namespace SmartShop.Application.Products.Queries.GetProductBySlug;

public record GetProductBySlugQuery(string Slug) : IRequest<ProductDto>;
