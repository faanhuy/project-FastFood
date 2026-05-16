using MediatR;
using Microsoft.AspNetCore.Http;

namespace SmartShop.Application.Features.Products.Commands.BulkImportProducts;

public record BulkImportProductsCommand(IFormFile File) : IRequest<BulkImportProductsResult>;

public record BulkImportProductsResult(int Created, int Failed, List<ImportRowError> Errors);

public record ImportRowError(int Row, string Field, string Message);
