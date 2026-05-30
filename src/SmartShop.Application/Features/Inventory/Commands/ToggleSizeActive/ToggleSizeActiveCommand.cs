using MediatR;
using SmartShop.Application.Common.Models;

namespace SmartShop.Application.Features.Inventory.Commands.ToggleSizeActive;

public record ToggleSizeActiveCommand(Guid Id) : IRequest<ApiResponse<SizeDto>>;
