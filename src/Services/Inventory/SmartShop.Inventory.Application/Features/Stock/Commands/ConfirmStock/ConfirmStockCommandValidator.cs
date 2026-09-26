using FluentValidation;

namespace SmartShop.Inventory.Application.Features.Stock.Commands.ConfirmStock;

public class ConfirmStockCommandValidator : AbstractValidator<ConfirmStockCommand>
{
    public ConfirmStockCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("validation.order_id_invalid");
    }
}
