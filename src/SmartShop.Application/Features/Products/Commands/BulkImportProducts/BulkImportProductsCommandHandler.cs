using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Products.Commands.BulkImportProducts;

public class BulkImportProductsCommandHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    ICacheService cache
) : IRequestHandler<BulkImportProductsCommand, BulkImportProductsResult>
{
    public async Task<BulkImportProductsResult> Handle(
        BulkImportProductsCommand request,
        CancellationToken cancellationToken)
    {
        var errors = new List<ImportRowError>();
        var created = 0;
        var failed = 0;
        var productsToAdd = new List<Product>();

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
                            // Validate Name
                            if (string.IsNullOrWhiteSpace(record.Name))
                            {
                                errors.Add(new ImportRowError(
                                    rowNumber,
                                    nameof(record.Name),
                                    "Tên sản phẩm không được để trống."));
                                failed++;
                                rowNumber++;
                                continue;
                            }

                            // Validate Price
                            if (record.Price <= 0)
                            {
                                errors.Add(new ImportRowError(
                                    rowNumber,
                                    nameof(record.Price),
                                    "Giá phải lớn hơn 0."));
                                failed++;
                                rowNumber++;
                                continue;
                            }

                            // Validate and parse CategoryId
                            if (!Guid.TryParse(record.CategoryId, out var categoryId))
                            {
                                errors.Add(new ImportRowError(
                                    rowNumber,
                                    nameof(record.CategoryId),
                                    "CategoryId không hợp lệ."));
                                failed++;
                                rowNumber++;
                                continue;
                            }

                            // Check if category exists
                            var category = await categoryRepository.GetByIdAsync(categoryId, cancellationToken);
                            if (category is null)
                            {
                                errors.Add(new ImportRowError(
                                    rowNumber,
                                    nameof(record.CategoryId),
                                    "Danh mục không tồn tại."));
                                failed++;
                                rowNumber++;
                                continue;
                            }

                            // Generate or validate slug
                            var slug = record.Slug;
                            if (string.IsNullOrWhiteSpace(slug))
                            {
                                slug = GenerateSlug(record.Name);
                            }

                            // Check for slug conflict
                            var slugCount = 0;
                            var originalSlug = slug;
                            while (await productRepository.GetBySlugAsync(slug, cancellationToken) is not null)
                            {
                                slugCount++;
                                var suffix = Guid.NewGuid().ToString("N")[..8];
                                slug = $"{originalSlug}-{suffix}";
                            }

                            // Create product
                            var product = Product.Create(
                                record.Name,
                                record.Description ?? string.Empty,
                                record.Price,
                                categoryId,
                                slug,
                                record.ImageUrl,
                                record.OriginalPrice);

                            productsToAdd.Add(product);
                            created++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add(new ImportRowError(
                                rowNumber,
                                "General",
                                $"Không thể đọc dòng này: {ex.Message}"));
                            failed++;
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
                    return new BulkImportProductsResult(0, 0, errors);
                }
            }
        }

        // Save all products at once if any were created successfully
        if (productsToAdd.Count > 0)
        {
            foreach (var product in productsToAdd)
            {
                await productRepository.AddAsync(product, cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await cache.RemoveByPrefixAsync("products:list:", cancellationToken);
        }

        return new BulkImportProductsResult(created, failed, errors);
    }

    private static string GenerateSlug(string name)
    {
        // Convert to lowercase and replace spaces with hyphens
        var slug = name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("đ", "d")
            .Replace("ă", "a")
            .Replace("â", "a")
            .Replace("ê", "e")
            .Replace("ô", "o")
            .Replace("ơ", "o")
            .Replace("ư", "u")
            .Replace("ơ", "o")
            // Remove any remaining Vietnamese diacritics
            .Replace("á", "a")
            .Replace("à", "a")
            .Replace("ả", "a")
            .Replace("ã", "a")
            .Replace("ạ", "a")
            .Replace("é", "e")
            .Replace("è", "e")
            .Replace("ẻ", "e")
            .Replace("ẽ", "e")
            .Replace("ẹ", "e")
            .Replace("í", "i")
            .Replace("ì", "i")
            .Replace("ỉ", "i")
            .Replace("ĩ", "i")
            .Replace("ị", "i")
            .Replace("ó", "o")
            .Replace("ò", "o")
            .Replace("ỏ", "o")
            .Replace("õ", "o")
            .Replace("ọ", "o")
            .Replace("ớ", "o")
            .Replace("ờ", "o")
            .Replace("ở", "o")
            .Replace("ỡ", "o")
            .Replace("ợ", "o")
            .Replace("ú", "u")
            .Replace("ù", "u")
            .Replace("ủ", "u")
            .Replace("ũ", "u")
            .Replace("ụ", "u")
            .Replace("ứ", "u")
            .Replace("ừ", "u")
            .Replace("ử", "u")
            .Replace("ữ", "u")
            .Replace("ự", "u")
            .Replace("ý", "y")
            .Replace("ỳ", "y")
            .Replace("ỷ", "y")
            .Replace("ỹ", "y")
            .Replace("ỵ", "y")
            // Remove non-alphanumeric characters except hyphens
            .Where(c => char.IsLetterOrDigit(c) || c == '-')
            .Aggregate("", (acc, c) => acc + c);

        // Add random suffix to ensure uniqueness
        var suffix = Guid.NewGuid().ToString("N")[..8];
        return $"{slug}-{suffix}".TrimEnd('-');
    }
}
