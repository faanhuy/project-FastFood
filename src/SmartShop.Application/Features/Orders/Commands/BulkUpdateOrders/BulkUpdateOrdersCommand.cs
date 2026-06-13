using MediatR;
using SmartShop.Application.Features.Common;

namespace SmartShop.Application.Features.Orders.Commands.BulkUpdateOrders;

public record BulkUpdateOrdersCommand(
    List<Guid> OrderIds,
    string Action,
    Guid AdminUserId
) : IRequest<BulkActionResult>;
