using FluentValidation;
using SmartShop.Domain.Enums;

namespace SmartShop.Application.Features.Coupons.Commands.CreateCoupon;

public class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("validation.coupon_code_required")
            .MaximumLength(50).WithMessage("validation.coupon_code_max_length");

        RuleFor(x => x.DiscountType)
            .IsInEnum().WithMessage("validation.discount_type_invalid");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("validation.discount_value_positive");

        RuleFor(x => x.DiscountValue)
            .LessThanOrEqualTo(100).WithMessage("validation.discount_percent_max")
            .When(x => x.DiscountType == DiscountType.Percentage);

        RuleFor(x => x.MinOrderValue)
            .GreaterThanOrEqualTo(0).WithMessage("validation.min_order_non_negative");

        RuleFor(x => x.MaxUsage)
            .GreaterThan(0).WithMessage("validation.max_usage_positive");

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("validation.coupon_expires_future");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("validation.description_max_length")
            .When(x => x.Description is not null);
    }
}
