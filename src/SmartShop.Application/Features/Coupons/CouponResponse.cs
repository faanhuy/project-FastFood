using SmartShop.Domain.Enums;

namespace SmartShop.Application.Features.Coupons;

public record CouponResponse(
    Guid Id,
    string Code,
    DiscountType DiscountType,
    decimal DiscountValue,
    decimal MinOrderValue,
    int MaxUsage,
    int UsedQuantity,
    DateTime ExpiresAt,
    string? Description
);
