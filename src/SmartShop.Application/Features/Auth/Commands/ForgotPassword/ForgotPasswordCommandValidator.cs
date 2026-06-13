using FluentValidation;

namespace SmartShop.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("validation.email_required")
            .EmailAddress().WithMessage("validation.email_invalid");
    }
}
