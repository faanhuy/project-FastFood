using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.FlashSales.Commands.DeleteFlashSale;

public class DeleteFlashSaleCommandHandler(
    IFlashSaleRepository flashSaleRepository,
    IOrderFlashSaleUsageRepository orderFlashSaleUsageRepository,
    IUnitOfWork unitOfWork,
    ICacheService cache) : IRequestHandler<DeleteFlashSaleCommand>
{
    public async Task Handle(DeleteFlashSaleCommand request, CancellationToken cancellationToken)
    {
        var flashSale = await flashSaleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(FlashSale), request.Id);

        // Không cho xóa flash sale đã được Approved
        if (flashSale.Status == FlashSaleStatus.Approved)
            throw new ConflictException("error.flashsale_cannot_delete_approved", null);

        // Kiểm tra đã từng có OrderFlashSaleUsage chưa
        var usages = await orderFlashSaleUsageRepository.GetByFlashSaleIdAsync(flashSale.Id, cancellationToken);
        if (usages.Any())
            throw new ConflictException("error.flashsale_has_orders", null);

        flashSaleRepository.Remove(flashSale);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cache.RemoveByPrefixAsync("flashsales:active:", cancellationToken);
    }
}
