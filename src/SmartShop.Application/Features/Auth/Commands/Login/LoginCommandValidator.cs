using FluentValidation;

namespace SmartShop.Application.Features.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("validation.email_required")
            .EmailAddress().WithMessage("validation.email_invalid");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("validation.password_required");
    }
}
