using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using SmartShop.Application.Features.Products.Commands.BulkImportProducts;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Products.Commands.PreviewCsvImport;

public class PreviewCsvImportCommandHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<PreviewCsvImportCommand, PreviewCsvImportResult>
{
    public async Task<PreviewCsvImportResult> Handle(
        PreviewCsvImportCommand request,
        CancellationToken cancellationToken)
    {
        var errors = new List<ImportRowError>();
        var preview = new List<CsvProductPreviewRow>();
        var validRows = 0;
        var invalidRows = 0;

        using (var reader = new StreamReader(request.File.OpenReadStream()))
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null
            };

            using (var csv = new CsvReader(reader, config))
            {
                var rowNumber = 2; // Start from 2 (header is row 1)

                try
                {
                    await foreach (var record in csv.GetRecordsAsync<ProductCsvRow>(cancellationToken))
                    {
                        try
                        {
                            var isValid = true;

                            // Validate Name
                            if (string.IsNullOrWhiteSpace(record.Name))
                            {
                                errors.Add(new ImportRowError(
                                    rowNumber,
                                    nameof(record.Name),
                                    "Tên sản phẩm không được để trống."));
                                isValid = false;
                            }

                            // Validate Price
                            if (record.Price <= 0)
                            {
                                errors.Add(new ImportRowError(
                                    rowNumber,
                                    nameof(record.Price),
                                    "Giá phải lớn hơn 0."));
                                isValid = false;
                            }

                            // Validate and parse CategoryId
                            if (!Guid.TryParse(record.CategoryId, out var categoryId))
                            {
                                errors.Add(new ImportRowError(
                                    rowNumber,
                                    nameof(record.CategoryId),
                                    "CategoryId không hợp lệ."));
                                isValid = false;
                            }
                            else
                            {
                                // Check if category exists
                                var category = await categoryRepository.GetByIdAsync(categoryId, cancellationToken);
                                if (category is null)
                                {
                                    errors.Add(new ImportRowError(
                                        rowNumber,
                                        nameof(record.CategoryId),
                                        "Danh mục không tồn tại."));
                                    isValid = false;
                                }
                            }

                            if (isValid)
                                validRows++;
                            else
                                invalidRows++;

                            // Add to preview regardless of validity
                            preview.Add(new CsvProductPreviewRow(
                                rowNumber,
                                record.Name ?? string.Empty,
                                record.Price,
                                record.CategoryId ?? string.Empty,
                                isValid));
                        }
                        catch (Exception ex)
                        {
                            errors.Add(new ImportRowError(
                                rowNumber,
                                "General",
                                $"Không thể đọc dòng này: {ex.Message}"));
                            invalidRows++;
                            preview.Add(new CsvProductPreviewRow(
                                rowNumber,
                                record?.Name ?? string.Empty,
                                record?.Price ?? 0m,
                                record?.CategoryId ?? string.Empty,
                                false));
                        }

                        rowNumber++;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(new ImportRowError(
                        2,
                        "File",
                        $"Lỗi đọc file CSV: {ex.Message}"));
                    return new PreviewCsvImportResult(0, 0, 1, errors, new List<CsvProductPreviewRow>());
                }
            }
        }

        return new PreviewCsvImportResult(
            validRows + invalidRows,
            validRows,
            invalidRows,
            errors,
            preview);
    }
}
