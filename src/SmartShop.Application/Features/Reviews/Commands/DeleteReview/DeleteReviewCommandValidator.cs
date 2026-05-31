using FluentValidation;

namespace SmartShop.Application.Features.Reviews.Commands.DeleteReview;

public class DeleteReviewCommandValidator : AbstractValidator<DeleteReviewCommand>
{
    public DeleteReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty().WithMessage("validation.review_id_invalid");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("validation.user_id_invalid");
    }
}
