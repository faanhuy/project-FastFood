using FluentValidation;

namespace SmartShop.Application.Features.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("validation.order_id_invalid");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("validation.order_status_invalid");
    }
}
