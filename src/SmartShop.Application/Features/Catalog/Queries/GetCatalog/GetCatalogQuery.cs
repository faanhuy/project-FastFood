using MediatR;
using SmartShop.Application.Common.Models;

namespace SmartShop.Application.Features.Catalog.Queries.GetCatalog;

public record GetCatalogQuery(
    int Page = 1,
    int PageSize = 20
) : IRequest<ApiResponse<GetCatalogResult>>;
