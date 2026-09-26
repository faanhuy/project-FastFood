using FluentValidation;

namespace SmartShop.Inventory.Application.Features.Stock.Commands.ReleaseStock;

public class ReleaseStockCommandValidator : AbstractValidator<ReleaseStockCommand>
{
    public ReleaseStockCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("validation.order_id_invalid");
    }
}
