using MediatR;
using SmartShop.Application.Features.Coupons;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Coupons.Queries;

public class ValidateCouponQueryHandler : IRequestHandler<ValidateCouponQuery, ValidateCouponResponse>
{
    private readonly ICouponRepository _couponRepository;

    public ValidateCouponQueryHandler(ICouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public async Task<ValidateCouponResponse> Handle(ValidateCouponQuery request, CancellationToken cancellationToken)
    {
        var coupon = await _couponRepository.GetByCodeAsync(request.Code, cancellationToken)
            ?? throw new NotFoundException(nameof(Coupon), request.Code);

        if (coupon.IsExpired())
            throw new ConflictException("error.coupon_expired", null);

        if (!coupon.HasRemaining())
            throw new ConflictException("error.coupon_used_up", null);

        if (!coupon.MeetsMinOrderValue(request.OrderTotal))
            throw new ConflictException("error.coupon_min_value", null);

        var usedByUser = await _couponRepository.HasUsageByUserAsync(coupon.Id, request.UserId, cancellationToken);
        if (usedByUser)
            throw new ConflictException("error.coupon_already_used", null);

        var discount = coupon.CalculateDiscount(request.OrderTotal);
        var finalAmount = request.OrderTotal - discount;
        return new ValidateCouponResponse(discount, finalAmount, request.OrderTotal);
    }
}
