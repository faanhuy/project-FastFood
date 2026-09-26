using FluentValidation;

namespace SmartShop.Inventory.Application.Features.Stock.Commands.ReserveStock;

public class ReserveStockCommandValidator : AbstractValidator<ReserveStockCommand>
{
    public ReserveStockCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("validation.order_id_invalid");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("validation.required_field");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("validation.product_id_invalid");
            item.RuleFor(i => i.StoreId)
                .NotEmpty().WithMessage("validation.store_id_invalid");
            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("validation.quantity_positive");
        });
    }
}
