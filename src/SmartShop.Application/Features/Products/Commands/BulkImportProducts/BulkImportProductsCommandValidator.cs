using FluentValidation;

namespace SmartShop.Application.Features.Products.Commands.BulkImportProducts;

public class BulkImportProductsCommandValidator : AbstractValidator<BulkImportProductsCommand>
{
    public BulkImportProductsCommandValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File không được để trống.");

        RuleFor(x => x.File.Length)
            .LessThanOrEqualTo(10 * 1024 * 1024)
            .WithMessage("Kích thước file không được vượt quá 10MB.");

        RuleFor(x => x.File.ContentType)
            .Must(ct => ct == "text/csv" || ct == "application/csv")
            .WithMessage("File phải có định dạng CSV.");

        RuleFor(x => x.File.FileName)
            .Must(fn => fn != null && (fn.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)))
            .WithMessage("File phải có extension .csv.");
    }
}
