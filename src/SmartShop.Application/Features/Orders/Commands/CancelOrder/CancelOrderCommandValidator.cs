using FluentValidation;

namespace SmartShop.Application.Features.Orders.Commands.CancelOrder;

public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId không hợp lệ.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId không hợp lệ.");
    }
}
