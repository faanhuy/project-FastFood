using FluentValidation;

namespace SmartShop.Application.Products.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("validation.product_name_required")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("validation.product_description_required");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("validation.price_positive");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("validation.product_slug_required")
            .Matches("^[a-z0-9-]+$").WithMessage("validation.product_slug_format");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("validation.category_required");
    }
}
