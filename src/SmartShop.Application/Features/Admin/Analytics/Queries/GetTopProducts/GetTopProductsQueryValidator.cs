using FluentValidation;

namespace SmartShop.Application.Features.Admin.Analytics.Queries.GetTopProducts;

public class GetTopProductsQueryValidator : AbstractValidator<GetTopProductsQuery>
{
    public GetTopProductsQueryValidator()
    {
        RuleFor(x => x.From)
            .LessThan(x => x.To).WithMessage("validation.date_from_before_to");

        RuleFor(x => x.To)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("validation.date_to_not_future");

        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100).WithMessage("validation.limit_range");
    }
}
