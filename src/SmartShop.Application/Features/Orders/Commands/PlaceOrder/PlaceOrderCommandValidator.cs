using FluentValidation;

namespace SmartShop.Application.Features.Orders.Commands.PlaceOrder;

public class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("validation.user_id_invalid");

        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("validation.store_required");

        RuleFor(x => x.AddressId)
            .NotEmpty().WithMessage("validation.address_required");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("validation.notes_max_length")
            .When(x => x.Notes is not null);

        RuleFor(x => x.CouponCode)
            .MaximumLength(50).WithMessage("validation.coupon_code_max_50")
            .When(x => x.CouponCode is not null);
    }
}
