using FluentValidation;

namespace SmartShop.Application.Features.Stores.Commands.UpdateStore;

public class UpdateStoreCommandValidator : AbstractValidator<UpdateStoreCommand>
{
    public UpdateStoreCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("validation.store_id_invalid");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("validation.store_name_required")
            .MaximumLength(100).WithMessage("validation.store_name_max");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("validation.store_phone_required")
            .MaximumLength(20).WithMessage("validation.store_phone_max");
    }
}
