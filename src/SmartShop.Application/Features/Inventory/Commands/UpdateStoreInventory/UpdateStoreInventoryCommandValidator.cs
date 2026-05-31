using FluentValidation;

namespace SmartShop.Application.Features.Inventory.Commands.UpdateStoreInventory;

public class UpdateStoreInventoryCommandValidator : AbstractValidator<UpdateStoreInventoryCommand>
{
    public UpdateStoreInventoryCommandValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("validation.store_id_invalid");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("validation.product_id_invalid");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("validation.inventory_non_negative");
    }
}
