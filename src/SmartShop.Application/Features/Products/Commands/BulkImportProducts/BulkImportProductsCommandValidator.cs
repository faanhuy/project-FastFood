using FluentValidation;

namespace SmartShop.Application.Features.Products.Commands.BulkImportProducts;

public class BulkImportProductsCommandValidator : AbstractValidator<BulkImportProductsCommand>
{
    public BulkImportProductsCommandValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("validation.file_required");

        RuleFor(x => x.File.Length)
            .LessThanOrEqualTo(10 * 1024 * 1024)
            .WithMessage("validation.file_csv_size");

        RuleFor(x => x.File.ContentType)
            .Must(ct => ct == "text/csv" || ct == "application/csv")
            .WithMessage("validation.file_csv_required");

        RuleFor(x => x.File.FileName)
            .Must(fn => fn != null && (fn.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)))
            .WithMessage("validation.file_csv_extension");
    }
}
