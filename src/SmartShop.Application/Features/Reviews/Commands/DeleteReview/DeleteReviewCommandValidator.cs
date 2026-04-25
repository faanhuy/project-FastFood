using FluentValidation;

namespace SmartShop.Application.Features.Reviews.Commands.DeleteReview;

public class DeleteReviewCommandValidator : AbstractValidator<DeleteReviewCommand>
{
    public DeleteReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty().WithMessage("ReviewId không hợp lệ.");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("UserId không hợp lệ.");
    }
}
