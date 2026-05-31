using FluentValidation;

namespace SmartShop.Application.Features.Stores.Commands.CreateStore;

public class CreateStoreCommandValidator : AbstractValidator<CreateStoreCommand>
{
    public CreateStoreCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("validation.store_name_required")
            .MaximumLength(100).WithMessage("validation.store_name_max");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("validation.store_phone_required")
            .MaximumLength(20).WithMessage("validation.store_phone_max");
    }
}
