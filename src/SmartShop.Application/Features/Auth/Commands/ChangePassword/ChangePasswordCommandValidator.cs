using FluentValidation;

namespace SmartShop.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("validation.password_required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("validation.password_required")
            .MinimumLength(6).WithMessage("validation.password_min_length")
            .NotEqual(x => x.CurrentPassword).WithMessage("validation.new_password_must_differ");
    }
}
