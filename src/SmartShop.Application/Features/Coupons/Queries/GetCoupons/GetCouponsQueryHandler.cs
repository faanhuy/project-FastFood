using MediatR;
using SmartShop.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartShop.Application.Features.Coupons.Queries.GetCoupons;

public class GetCouponsQueryHandler : IRequestHandler<GetCouponsQuery, IEnumerable<CouponResponse>>
{
    private readonly ICouponRepository _repository;

    public GetCouponsQueryHandler(ICouponRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CouponResponse>> Handle(
        GetCouponsQuery request, CancellationToken cancellationToken)
    {
        var coupons = await _repository.GetAllAsync(cancellationToken);

        return coupons.Select(c => new CouponResponse(
            c.Id, c.Code, c.DiscountType, c.DiscountValue,
            c.MinOrderValue, c.MaxUsage, c.UsedQuantity,
            c.ExpiresAt, c.Description));
    }
}
