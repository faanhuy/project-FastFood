using MediatR;
using SmartShop.Application.Features.FlashSales;

namespace SmartShop.Application.Features.FlashSales.Commands.ApproveFlashSale;

public record ApproveFlashSaleCommand(Guid Id, string ApprovedBy) : IRequest<FlashSaleDto>;
