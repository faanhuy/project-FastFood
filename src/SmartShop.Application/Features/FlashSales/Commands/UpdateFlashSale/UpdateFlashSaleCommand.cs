using MediatR;
using SmartShop.Application.Features.FlashSales;

namespace SmartShop.Application.Features.FlashSales.Commands.UpdateFlashSale;

public record UpdateFlashSaleCommand(
    Guid Id,
    string Name,
    DateTime StartAt,
    DateTime EndAt,
    List<UpdateFlashSaleItemRequest> Items) : IRequest<FlashSaleDto>;
