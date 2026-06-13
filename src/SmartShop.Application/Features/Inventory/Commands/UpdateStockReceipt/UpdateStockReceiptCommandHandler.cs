using MediatR;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Inventory.Commands.UpdateStockReceipt;

public class UpdateStockReceiptCommandHandler(
    IStockReceiptRepository receiptRepo,
    IProductRepository productRepo,
    ISizeRepository sizeRepo,
    IUnitOfWork uow)
    : IRequestHandler<UpdateStockReceiptCommand, ApiResponse<StockReceiptDetailDto>>
{
    public async Task<ApiResponse<StockReceiptDetailDto>> Handle(UpdateStockReceiptCommand request, CancellationToken ct)
    {
        var receipt = await receiptRepo.GetByIdWithItemsAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(StockReceipt), request.Id);

        if (receipt.Status != ReceiptStatus.Pending)
            throw new ConflictException("error.stock_receipt_invalid_status", null);

        if (request.Items == null || request.Items.Count == 0)
            throw new ConflictException("validation.required_field", null);

        // Validate items
        foreach (var item in request.Items)
        {
            var product = await productRepo.GetByIdAsync(item.ProductId, ct)
                ?? throw new InvalidOperationException($"Sản phẩm với ID {item.ProductId} không tồn tại.");

            if (product.HasSizes && item.SizeId == null)
                throw new InvalidOperationException($"Sản phẩm '{product.Name}' yêu cầu chọn kích thước.");

            if (item.SizeId.HasValue)
            {
                _ = await sizeRepo.GetByIdAsync(item.SizeId.Value, ct)
                    ?? throw new InvalidOperationException($"Kích thước với ID {item.SizeId} không tồn tại.");
            }
        }

        // Replace all items
        receipt.ClearItems();
        receipt.UpdateNotes(request.Notes);
        receipt.UpdateReceiptDate(request.ReceiptDate);

        foreach (var item in request.Items)
        {
            var receiptItem = StockReceiptItem.Create(
                receipt.Id,
                item.ProductId,
                item.SizeId,
                item.Quantity,
                item.Notes
            );
            receipt.AddItem(receiptItem);
        }

        receiptRepo.Update(receipt);
        await uow.SaveChangesAsync(ct);

        return ApiResponse<StockReceiptDetailDto>.Ok(StockReceiptDetailDto.From(receipt));
    }
}
