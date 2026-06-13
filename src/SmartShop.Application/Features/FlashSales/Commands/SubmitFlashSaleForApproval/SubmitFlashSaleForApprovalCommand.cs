using MediatR;
using SmartShop.Application.Features.FlashSales;

namespace SmartShop.Application.Features.FlashSales.Commands.SubmitFlashSaleForApproval;

public record SubmitFlashSaleForApprovalCommand(Guid Id) : IRequest<FlashSaleDto>;
