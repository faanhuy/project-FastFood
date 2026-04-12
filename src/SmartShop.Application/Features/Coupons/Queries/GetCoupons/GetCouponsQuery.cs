using MediatR;

namespace SmartShop.Application.Features.Coupons.Queries.GetCoupons;

public record GetCouponsQuery() : IRequest<IEnumerable<CouponResponse>>;
