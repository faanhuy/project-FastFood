using FluentValidation;

namespace SmartShop.Application.Features.Reviews.Commands.AddReview;

public class AddReviewCommandValidator : AbstractValidator<AddReviewCommand>
{
    public AddReviewCommandValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Đánh giá phải từ 1 đến 5 sao.");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Nội dung đánh giá không được để trống.")
            .MaximumLength(2000).WithMessage("Đánh giá tối đa 2000 ký tự.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId không hợp lệ.");
    }
}
