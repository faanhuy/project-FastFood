using MediatR;
using SmartShop.Application.Features.FlashSales;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.FlashSales.Queries.GetUpcomingFlashSales;

public record GetUpcomingFlashSalesQuery(
    int WithinDays = 7,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<FlashSaleDto>>;
