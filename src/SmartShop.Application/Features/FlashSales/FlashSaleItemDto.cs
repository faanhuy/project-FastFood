namespace SmartShop.Application.Features.FlashSales;

public class FlashSaleItemDto
{
    public Guid FlashSaleId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public Guid? SizeId { get; set; }
    public string? SizeLabel { get; set; }

    public decimal SalePrice { get; set; }
    public decimal OriginalPrice { get; set; }
    public int StockLimit { get; set; }
    public int SoldCount { get; set; }
    public int RemainingStock { get; set; }
    public decimal PercentDiscount { get; set; }
}
