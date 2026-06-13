using SmartShop.Domain.Common;
using SmartShop.Domain.Common.Exceptions;

namespace SmartShop.Domain.Entities;

public class StockReceiptItem : BaseEntity
{
    public Guid StockReceiptId { get; private set; }
    public StockReceipt StockReceipt { get; private set; } = null!;
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public Guid? SizeId { get; private set; }
    public Size? Size { get; private set; }
    public int Quantity { get; private set; }
    public string? Notes { get; private set; }

    private StockReceiptItem() { }

    public static StockReceiptItem Create(Guid stockReceiptId, Guid productId, Guid? sizeId, int quantity, string? notes = null)
    {
        if (stockReceiptId == Guid.Empty)
            throw new ConflictException("validation.order_id_invalid", null);
        if (productId == Guid.Empty)
            throw new ConflictException("validation.product_id_invalid", null);
        if (quantity <= 0)
            throw new ConflictException("validation.quantity_positive", null);

        return new StockReceiptItem
        {
            Id = Guid.NewGuid(),
            StockReceiptId = stockReceiptId,
            ProductId = productId,
            SizeId = sizeId,
            Quantity = quantity,
            Notes = notes
        };
    }
}
