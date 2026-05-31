using FluentValidation;

namespace SmartShop.Application.Features.Cart.Commands.UpdateCartItem;

public class UpdateCartItemCommandValidator : AbstractValidator<UpdateCartItemCommand>
{
    public UpdateCartItemCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("validation.user_id_invalid");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("validation.product_id_invalid");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("validation.quantity_positive");
    }
}
