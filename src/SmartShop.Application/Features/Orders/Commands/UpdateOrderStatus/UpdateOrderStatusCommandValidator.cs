using FluentValidation;

namespace SmartShop.Application.Features.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId không hợp lệ.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Trạng thái đơn hàng không hợp lệ.");
    }
}
