using FluentValidation;

namespace SmartShop.Application.Features.Coupons.Queries;

public class ValidateCouponQueryValidator : AbstractValidator<ValidateCouponQuery>
{
    public ValidateCouponQueryValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("validation.coupon_code_required")
            .MaximumLength(50).WithMessage("validation.coupon_code_max_length");

        RuleFor(x => x.OrderTotal)
            .GreaterThan(0).WithMessage("validation.order_amount_positive");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("validation.user_id_invalid");
    }
}
