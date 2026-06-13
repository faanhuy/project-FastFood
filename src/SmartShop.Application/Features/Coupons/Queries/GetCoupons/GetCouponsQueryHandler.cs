using MediatR;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Coupons.Queries.GetCoupons;

public class GetCouponsQueryHandler(ICouponRepository repository)
    : IRequestHandler<GetCouponsQuery, PagedResult<CouponResponse>>
{
    public async Task<PagedResult<CouponResponse>> Handle(
        GetCouponsQuery request, CancellationToken cancellationToken)
    {
        var (coupons, totalCount) = await repository.GetPagedAsync(
            request.Page, request.PageSize, request.Search, request.IsExpired, cancellationToken);

        var dtos = coupons.Select(c => new CouponResponse(
            c.Id, c.Code, c.DiscountType, c.DiscountValue,
            c.MinOrderValue, c.MaxUsage, c.UsedQuantity,
            c.ExpiresAt, c.Description)).ToList();

        return new PagedResult<CouponResponse>(dtos, totalCount, request.Page, request.PageSize);
    }
}
