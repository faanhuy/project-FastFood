using FluentValidation;

namespace SmartShop.Inventory.Application.Features.Stock.Queries.GetStockLevel;

public class GetStockLevelQueryValidator : AbstractValidator<GetStockLevelQuery>
{
    public GetStockLevelQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("validation.product_id_invalid");
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("validation.store_id_invalid");
    }
}
