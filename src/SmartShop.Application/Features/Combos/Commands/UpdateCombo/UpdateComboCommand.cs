using MediatR;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Features.Combos.Commands.CreateCombo;

namespace SmartShop.Application.Features.Combos.Commands.UpdateCombo;

public record UpdateComboCommand(
    Guid Id,
    string Name,
    string Title,
    string? Description,
    string ImageUrl,
    decimal SalePrice,
    DateTime StartsAt,
    DateTime? EndsAt,
    List<CreateComboItemRequest> Items
) : IRequest<ApiResponse<ComboDto>>;
