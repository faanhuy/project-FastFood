using MediatR;
using SmartShop.Application.DTOs;

namespace SmartShop.Application.Products.Queries.GetProductBySlug;

public record GetProductBySlugQuery(string Slug, Guid? StoreId = null) : IRequest<ProductDetailDto>;
