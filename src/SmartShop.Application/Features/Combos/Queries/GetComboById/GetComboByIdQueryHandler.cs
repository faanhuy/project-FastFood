using MediatR;
using SmartShop.Application.Common.Models;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Combos.Queries.GetComboById;

public class GetComboByIdQueryHandler(IComboRepository comboRepository)
    : IRequestHandler<GetComboByIdQuery, ApiResponse<ComboDto>>
{
    public async Task<ApiResponse<ComboDto>> Handle(GetComboByIdQuery request, CancellationToken cancellationToken)
    {
        var combo = await comboRepository.GetByIdWithProductsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Combo), request.Id);

        var dto = MapToDto(combo);
        return ApiResponse<ComboDto>.Ok(dto);
    }

    private static ComboDto MapToDto(Domain.Entities.Combo combo)
    {
        var items = combo.Items.Select(item => new ComboItemDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            SizeId = item.SizeId,
            SizeLabel = item.SizeLabel,
            Quantity = item.Quantity,
            UnitPriceSnapshot = item.UnitPriceSnapshot,
            CurrentUnitPrice = item.Product?.Price ?? item.UnitPriceSnapshot
        }).ToList();

        var currentOriginalPrice = items.Sum(i => i.CurrentUnitPrice * i.Quantity);

        return new ComboDto
        {
            Id = combo.Id,
            Name = combo.Name,
            Title = combo.Title,
            Description = combo.Description,
            ImageUrl = combo.ImageUrl,
            OriginalPrice = combo.OriginalPrice,
            CurrentOriginalPrice = currentOriginalPrice,
            SalePrice = combo.SalePrice,
            IsActive = combo.IsActive,
            StartsAt = combo.StartsAt,
            EndsAt = combo.EndsAt,
            IsCurrentlyActive = combo.IsCurrentlyActive(),
            Items = items,
            CreatedAt = combo.CreatedAt
        };
    }
}
