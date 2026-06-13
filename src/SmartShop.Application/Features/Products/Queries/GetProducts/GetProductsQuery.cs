using MediatR;
using SmartShop.Application.DTOs;

namespace SmartShop.Application.Products.Queries.GetProducts;

public enum ProductSortBy
{
    Newest      = 0, // CreatedAt desc (default)
    PriceAsc    = 1, // Price asc
    PriceDesc   = 2, // Price desc
    NameAsc     = 3, // Name A-Z
    NameDesc    = 4, // Name Z-A
    BestSelling = 5, // Total sold desc (join OrderItems)
}

public record GetProductsQuery(
    int Page = 1,
    int PageSize = 12,
    Guid? CategoryId = null,
    string? Search = null,
    ProductSortBy SortBy = ProductSortBy.Newest,
    Guid? StoreId = null,
    bool? IsActiveFilter = null,
    decimal? PriceMin = null,
    decimal? PriceMax = null
) : IRequest<PagedResult<ProductDto>>;

public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
};
