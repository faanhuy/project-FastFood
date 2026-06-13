using MediatR;
using SmartShop.Application.Features.Common;

namespace SmartShop.Application.Features.Products.Commands.BulkUpdateProducts;

public record BulkUpdateProductsCommand(
    List<Guid> ProductIds,
    string Action,
    Guid AdminUserId
) : IRequest<BulkActionResult>;
