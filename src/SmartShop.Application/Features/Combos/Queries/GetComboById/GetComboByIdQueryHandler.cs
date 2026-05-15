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
        var combo = await comboRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Combo), request.Id);

        var dto = MapToDto(combo);
        return ApiResponse<ComboDto>.Ok(dto);
    }

    private static ComboDto MapToDto(Domain.Entities.Combo combo)
    {
        return new ComboDto
        {
            Id = combo.Id,
            Name = combo.Name,
            Title = combo.Title,
            Description = combo.Description,
            ImageUrl = combo.ImageUrl,
            OriginalPrice = combo.OriginalPrice,
            SalePrice = combo.SalePrice,
            IsActive = combo.IsActive,
            StartsAt = combo.StartsAt,
            EndsAt = combo.EndsAt,
            IsCurrentlyActive = combo.IsCurrentlyActive(),
            Items = combo.Items.Select(item => new ComboItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                SizeId = item.SizeId,
                SizeLabel = item.SizeLabel,
                Quantity = item.Quantity,
                UnitPriceSnapshot = item.UnitPriceSnapshot
            }).ToList(),
            CreatedAt = combo.CreatedAt
        };
    }
}
