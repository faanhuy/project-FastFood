using MediatR;
using SmartShop.Application.Features.Coupons;

namespace SmartShop.Application.Features.Coupons.Queries;

public record ValidateCouponQuery(
    string Code,
    decimal OrderTotal,
    Guid UserId
) : IRequest<ValidateCouponResponse>;
