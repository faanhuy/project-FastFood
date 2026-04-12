using MediatR;
using SmartShop.Domain.Enums;

namespace SmartShop.Application.Features.Coupons.Commands.CreateCoupon;

public record CreateCouponCommand(
    string Code,
    DiscountType DiscountType,
    decimal DiscountValue,
    decimal MinOrderValue,
    int MaxUsage,
    DateTime ExpiresAt,
    string? Description
) : IRequest<CouponResponse>;
