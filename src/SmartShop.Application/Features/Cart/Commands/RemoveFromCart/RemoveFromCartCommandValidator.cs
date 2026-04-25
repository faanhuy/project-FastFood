using FluentValidation;

namespace SmartShop.Application.Features.Cart.Commands.RemoveFromCart;

public class RemoveFromCartCommandValidator : AbstractValidator<RemoveFromCartCommand>
{
    public RemoveFromCartCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId không hợp lệ.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId không hợp lệ.");
    }
}
