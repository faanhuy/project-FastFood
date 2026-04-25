using FluentValidation;

namespace SmartShop.Application.Features.Admin.Analytics.Queries.GetTopProducts;

public class GetTopProductsQueryValidator : AbstractValidator<GetTopProductsQuery>
{
    public GetTopProductsQueryValidator()
    {
        RuleFor(x => x.From)
            .LessThan(x => x.To).WithMessage("Ngày bắt đầu phải nhỏ hơn ngày kết thúc.");

        RuleFor(x => x.To)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("Ngày kết thúc không được ở tương lai.");

        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100).WithMessage("Limit phải từ 1 đến 100.");
    }
}
