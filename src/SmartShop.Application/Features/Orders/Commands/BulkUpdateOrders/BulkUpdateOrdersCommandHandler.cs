using MediatR;
using SmartShop.Application.Features.Common;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Orders.Commands.BulkUpdateOrders;

public class BulkUpdateOrdersCommandHandler(
    IOrderRepository repository,
    IUnitOfWork unitOfWork
) : IRequestHandler<BulkUpdateOrdersCommand, BulkActionResult>
{
    public async Task<BulkActionResult> Handle(BulkUpdateOrdersCommand request, CancellationToken cancellationToken)
    {
        // Max 100 items per batch
        if (request.OrderIds.Count > 100)
            throw new ConflictException("error.bulk_max_items", null);

        var orders = await repository.GetByIdsAsync(request.OrderIds, cancellationToken);
        var errors = new List<BulkItemError>();
        var succeeded = 0;

        foreach (var id in request.OrderIds)
        {
            var order = orders.FirstOrDefault(o => o.Id == id);
            if (order is null)
            {
                errors.Add(new BulkItemError(id, "Order not found"));
                continue;
            }

            try
            {
                switch (request.Action.ToLowerInvariant())
                {
                    case "confirm":
                        order.UpdateStatus(OrderStatus.Confirmed);
                        succeeded++;
                        break;
                    case "ship":
                        order.UpdateStatus(OrderStatus.Shipped);
                        succeeded++;
                        break;
                    case "deliver":
                        order.UpdateStatus(OrderStatus.Delivered);
                        order.SetDeliveredAt(DateTime.UtcNow);
                        succeeded++;
                        break;
                    case "cancel":
                        order.Cancel();
                        succeeded++;
                        break;
                    default:
                        errors.Add(new BulkItemError(id, $"Unknown action: {request.Action}"));
                        break;
                }
            }
            catch (Exception ex)
            {
                errors.Add(new BulkItemError(id, ex.Message));
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new BulkActionResult(succeeded, errors.Count, errors);
    }
}
