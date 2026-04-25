using FluentValidation;

namespace SmartShop.Application.Features.Admin.Analytics.Queries.GetRevenueByDate;

public class GetRevenueByDateQueryValidator : AbstractValidator<GetRevenueByDateQuery>
{
    public GetRevenueByDateQueryValidator()
    {
        RuleFor(x => x.From)
            .LessThan(x => x.To).WithMessage("Ngày bắt đầu phải nhỏ hơn ngày kết thúc.");

        RuleFor(x => x.To)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("Ngày kết thúc không được ở tương lai.");
    }
}
