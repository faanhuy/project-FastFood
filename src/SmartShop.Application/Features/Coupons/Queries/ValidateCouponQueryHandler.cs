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
            throw new ConflictException("Coupon đã hết hạn.");

        if (!coupon.HasRemaining())
            throw new ConflictException("Coupon đã hết lượt sử dụng.");

        if (!coupon.MeetsMinOrderValue(request.OrderTotal))
            throw new ConflictException("Đơn hàng không đủ giá trị tối thiểu để áp dụng coupon.");

        var usedByUser = await _couponRepository.HasUsageByUserAsync(coupon.Id, request.UserId, cancellationToken);
        if (usedByUser)
            throw new ConflictException("Bạn đã sử dụng coupon này rồi.");

        var discount = coupon.CalculateDiscount(request.OrderTotal);
        var finalAmount = request.OrderTotal - discount;
        return new ValidateCouponResponse(discount, finalAmount, request.OrderTotal);
    }
}
