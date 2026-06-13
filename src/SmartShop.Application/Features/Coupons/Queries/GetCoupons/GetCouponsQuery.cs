using MediatR;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Coupons.Queries.GetCoupons;

public record GetCouponsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    bool? IsExpired = null
) : IRequest<PagedResult<CouponResponse>>;
