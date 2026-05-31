using FluentValidation;

namespace SmartShop.Application.Features.Admin.Analytics.Queries.GetRevenueByDate;

public class GetRevenueByDateQueryValidator : AbstractValidator<GetRevenueByDateQuery>
{
    public GetRevenueByDateQueryValidator()
    {
        RuleFor(x => x.From)
            .LessThan(x => x.To).WithMessage("validation.date_from_before_to");

        RuleFor(x => x.To)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("validation.date_to_not_future");
    }
}
