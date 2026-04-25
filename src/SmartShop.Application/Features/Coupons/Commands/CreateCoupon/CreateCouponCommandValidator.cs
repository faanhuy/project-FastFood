using FluentValidation;
using SmartShop.Domain.Enums;

namespace SmartShop.Application.Features.Coupons.Commands.CreateCoupon;

public class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã coupon không được để trống.")
            .MaximumLength(50).WithMessage("Mã coupon tối đa 50 ký tự.");

        RuleFor(x => x.DiscountType)
            .IsInEnum().WithMessage("Loại giảm giá không hợp lệ.");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("Giá trị giảm phải lớn hơn 0.");

        RuleFor(x => x.DiscountValue)
            .LessThanOrEqualTo(100).WithMessage("Phần trăm giảm giá không được vượt quá 100%.")
            .When(x => x.DiscountType == DiscountType.Percentage);

        RuleFor(x => x.MinOrderValue)
            .GreaterThanOrEqualTo(0).WithMessage("Giá trị đơn hàng tối thiểu không được âm.");

        RuleFor(x => x.MaxUsage)
            .GreaterThan(0).WithMessage("Số lần sử dụng tối đa phải lớn hơn 0.");

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Ngày hết hạn phải ở tương lai.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Mô tả tối đa 500 ký tự.")
            .When(x => x.Description is not null);
    }
}
