using MediatR;
using SmartShop.Application.Features.FlashSales;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.FlashSales.Queries.GetActiveFlashSales;

public record GetActiveFlashSalesQuery(
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<FlashSaleDto>>;
