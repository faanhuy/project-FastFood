using FluentValidation;

namespace SmartShop.Application.Features.Coupons.Queries;

public class ValidateCouponQueryValidator : AbstractValidator<ValidateCouponQuery>
{
    public ValidateCouponQueryValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã coupon không được để trống.")
            .MaximumLength(50).WithMessage("Mã coupon tối đa 50 ký tự.");

        RuleFor(x => x.OrderTotal)
            .GreaterThan(0).WithMessage("Tổng đơn hàng phải lớn hơn 0.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId không hợp lệ.");
    }
}
