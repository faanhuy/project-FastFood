namespace SmartShop.Application.Features.Common;

public record BulkActionResult(
    int Succeeded,
    int Failed,
    List<BulkItemError> Errors
);

public record BulkItemError(Guid ItemId, string Message);
