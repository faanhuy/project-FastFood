using MediatR;
using SmartShop.Application.Features.FlashSales;

namespace SmartShop.Application.Features.FlashSales.Commands.RejectFlashSale;

public record RejectFlashSaleCommand(Guid Id, string Reason) : IRequest<FlashSaleDto>;
