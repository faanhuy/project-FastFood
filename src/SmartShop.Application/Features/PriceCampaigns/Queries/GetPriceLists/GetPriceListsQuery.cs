using MediatR;
using SmartShop.Application.Common.Models;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.PriceCampaigns.Queries.GetPriceLists;

public record GetPriceListsQuery(int Page = 1, int PageSize = 20)
    : IRequest<ApiResponse<PagedResult<PriceCampaignSummaryDto>>>;
