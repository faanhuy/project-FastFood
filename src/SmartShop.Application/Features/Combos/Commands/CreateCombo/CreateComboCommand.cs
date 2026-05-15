using MediatR;
using SmartShop.Application.Common.Models;

namespace SmartShop.Application.Features.Combos.Commands.CreateCombo;

public record CreateComboItemRequest(
    Guid ProductId,
    Guid? SizeId,
    int Quantity
);

public record CreateComboCommand(
    string Name,
    string Title,
    string? Description,
    string ImageUrl,
    decimal SalePrice,
    DateTime StartsAt,
    DateTime? EndsAt,
    List<CreateComboItemRequest> Items
) : IRequest<ApiResponse<ComboDto>>;
