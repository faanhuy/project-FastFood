using MediatR;

namespace SmartShop.Application.Features.FlashSales.Commands.DeleteFlashSale;

public record DeleteFlashSaleCommand(Guid Id) : IRequest;
