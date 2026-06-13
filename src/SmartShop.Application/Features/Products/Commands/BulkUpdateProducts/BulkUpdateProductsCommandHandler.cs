using MediatR;
using SmartShop.Application.Features.Common;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Products.Commands.BulkUpdateProducts;

public class BulkUpdateProductsCommandHandler(
    IProductRepository repository,
    IUnitOfWork unitOfWork
) : IRequestHandler<BulkUpdateProductsCommand, BulkActionResult>
{
    public async Task<BulkActionResult> Handle(BulkUpdateProductsCommand request, CancellationToken cancellationToken)
    {
        // Max 100 items per batch
        if (request.ProductIds.Count > 100)
            throw new ConflictException("error.bulk_max_items", null);

        var products = await repository.GetByIdsAsync(request.ProductIds, cancellationToken);
        var errors = new List<BulkItemError>();
        var succeeded = 0;

        foreach (var id in request.ProductIds)
        {
            var product = products.FirstOrDefault(p => p.Id == id);
            if (product is null)
            {
                errors.Add(new BulkItemError(id, "Product not found"));
                continue;
            }

            try
            {
                switch (request.Action.ToLowerInvariant())
                {
                    case "activate":
                        product.Activate();
                        succeeded++;
                        break;
                    case "deactivate":
                        product.Deactivate();
                        succeeded++;
                        break;
                    case "delete":
                        repository.Delete(product);
                        succeeded++;
                        break;
                    default:
                        errors.Add(new BulkItemError(id, $"Unknown action: {request.Action}"));
                        break;
                }
            }
            catch (Exception ex)
            {
                errors.Add(new BulkItemError(id, $"Error: {ex.Message}"));
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new BulkActionResult(succeeded, errors.Count, errors);
    }
}
