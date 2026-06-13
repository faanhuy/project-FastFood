using MediatR;
using Microsoft.AspNetCore.Http;

namespace SmartShop.Application.Features.Products.Commands.PreviewCsvImport;

public record PreviewCsvImportCommand(IFormFile File) : IRequest<PreviewCsvImportResult>;

public record PreviewCsvImportResult(
    int TotalRows,
    int ValidRows,
    int InvalidRows,
    List<ImportRowError> Errors,
    List<CsvProductPreviewRow> Preview);

public record CsvProductPreviewRow(
    int RowNumber,
    string Name,
    decimal Price,
    string CategoryId,
    bool IsValid);

public record ImportRowError(int Row, string Field, string Message);
