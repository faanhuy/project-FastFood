using FluentValidation;

namespace SmartShop.Application.Features.Cart.Commands.RemoveFromCart;

public class RemoveFromCartCommandValidator : AbstractValidator<RemoveFromCartCommand>
{
    public RemoveFromCartCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("validation.user_id_invalid");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("validation.product_id_invalid");
    }
}
