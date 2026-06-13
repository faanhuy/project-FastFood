using MediatR;
using SmartShop.Application.Features.FlashSales;

namespace SmartShop.Application.Features.FlashSales.Commands.CreateFlashSale;

public record CreateFlashSaleCommand(
    string Name,
    DateTime StartAt,
    DateTime EndAt,
    List<CreateFlashSaleItemRequest> Items) : IRequest<FlashSaleDto>;
