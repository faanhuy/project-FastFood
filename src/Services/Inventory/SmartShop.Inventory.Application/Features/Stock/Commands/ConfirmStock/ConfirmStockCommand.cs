using MediatR;

namespace SmartShop.Inventory.Application.Features.Stock.Commands.ConfirmStock;

public record ConfirmStockCommand(Guid OrderId) : IRequest;
