using FluentValidation;

namespace SmartShop.Application.Features.Reviews.Commands.AddReview;

public class AddReviewCommandValidator : AbstractValidator<AddReviewCommand>
{
    public AddReviewCommandValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("validation.review_rating_range");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("validation.review_content_required")
            .MaximumLength(2000).WithMessage("validation.review_content_max");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("validation.product_id_invalid");
    }
}
