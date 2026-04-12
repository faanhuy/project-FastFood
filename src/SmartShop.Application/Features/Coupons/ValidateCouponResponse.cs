namespace SmartShop.Application.Features.Coupons;

public record ValidateCouponResponse(
    decimal DiscountAmount,
    decimal FinalAmount,
    decimal OriginalAmount
);
