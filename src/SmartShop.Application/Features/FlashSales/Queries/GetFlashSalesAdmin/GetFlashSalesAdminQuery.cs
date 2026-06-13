using MediatR;
using SmartShop.Application.Features.FlashSales;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.FlashSales.Queries.GetFlashSalesAdmin;

public record GetFlashSalesAdminQuery(
    int Page = 1,
    int PageSize = 20,
    bool? IsActive = null,
    string? Status = null) : IRequest<PagedResult<FlashSaleDto>>;
