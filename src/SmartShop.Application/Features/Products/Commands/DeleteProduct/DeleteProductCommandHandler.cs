using MediatR;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler(
    IProductRepository repository,
    IUnitOfWork unitOfWork,
    ICacheService cache
) : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        // Soft delete — deactivate instead of hard delete
        product.Deactivate();
        repository.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cache.RemoveAsync($"products:id:{request.Id}", cancellationToken);
        await cache.RemoveByPrefixAsync("products:list:", cancellationToken);
    }
}
