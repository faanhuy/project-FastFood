using FluentValidation;

namespace SmartShop.Application.Features.Orders.Commands.CancelOrder;

public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("validation.order_id_invalid");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("validation.user_id_invalid");
    }
}
