using MediatR;
using SmartShop.Application.DTOs;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Products.Queries.GetProducts;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    private readonly IProductRepository _repository;

    public GetProductsQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            request.Page, request.PageSize, request.CategoryId, cancellationToken);

        var dtos = items.Select(p => new ProductDto(
            p.Id, p.Name, p.Description, p.Price, p.OriginalPrice,
            p.Stock, p.Slug, p.ImageUrl, p.IsActive, p.CategoryId, p.CreatedAt));

        return new PagedResult<ProductDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}
