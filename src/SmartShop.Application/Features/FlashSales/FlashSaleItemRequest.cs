namespace SmartShop.Application.Features.FlashSales;

public record CreateFlashSaleItemRequest(
    Guid ProductId,
    Guid? SizeId,
    decimal SalePrice,
    int StockLimit);

public record UpdateFlashSaleItemRequest(
    Guid ProductId,
    Guid? SizeId,
    decimal SalePrice,
    int StockLimit);

public record CreateFlashSaleRequest(
    string Name,
    DateTime StartAt,
    DateTime EndAt,
    List<CreateFlashSaleItemRequest> Items);

public record UpdateFlashSaleRequest(
    string Name,
    DateTime StartAt,
    DateTime EndAt,
    List<UpdateFlashSaleItemRequest> Items);
