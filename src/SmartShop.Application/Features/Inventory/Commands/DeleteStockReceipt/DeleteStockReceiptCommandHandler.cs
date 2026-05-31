using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Inventory.Commands.DeleteStockReceipt;

public class DeleteStockReceiptCommandHandler(
    IStockReceiptRepository repo,
    IUnitOfWork uow,
    ILocalizationService localization,
    ICurrentLanguageService languageService)
    : IRequestHandler<DeleteStockReceiptCommand, ApiResponse<object>>
{
    public async Task<ApiResponse<object>> Handle(DeleteStockReceiptCommand request, CancellationToken ct)
    {
        var receipt = await repo.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(StockReceipt), request.Id);

        if (receipt.Status != ReceiptStatus.Pending)
            throw new ConflictException("error.stock_receipt_not_pending", null);

        repo.Delete(receipt);
        await uow.SaveChangesAsync(ct);

        return ApiResponse<object>.Ok(new { },
            localization.GetMessage("success.stock_receipt_deleted", languageService.Language));
    }
}
