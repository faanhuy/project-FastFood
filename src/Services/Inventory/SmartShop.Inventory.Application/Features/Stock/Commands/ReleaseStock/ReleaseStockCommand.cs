using MediatR;

namespace SmartShop.Inventory.Application.Features.Stock.Commands.ReleaseStock;

public record ReleaseStockCommand(Guid OrderId) : IRequest;
