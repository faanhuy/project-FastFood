using FluentValidation;

namespace SmartShop.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("validation.email_required")
            .EmailAddress().WithMessage("validation.email_invalid");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("validation.password_required")
            .MinimumLength(6).WithMessage("validation.password_min_length");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("validation.first_name_required")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("validation.last_name_required")
            .MaximumLength(100);
    }
}
