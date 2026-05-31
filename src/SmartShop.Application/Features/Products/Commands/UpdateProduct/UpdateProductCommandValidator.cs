using FluentValidation;

namespace SmartShop.Application.Products.Commands.UpdateProduct;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("validation.product_name_required")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("validation.product_description_required");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("validation.price_positive");
    }
}
