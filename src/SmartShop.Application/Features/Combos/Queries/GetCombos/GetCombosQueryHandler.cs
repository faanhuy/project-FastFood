using MediatR;
using SmartShop.Application.Common.Models;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Combos.Queries.GetCombos;

public class GetCombosQueryHandler(IComboRepository comboRepository)
    : IRequestHandler<GetCombosQuery, ApiResponse<GetCombosResult>>
{
    public async Task<ApiResponse<GetCombosResult>> Handle(GetCombosQuery request, CancellationToken cancellationToken)
    {
        var combos = await comboRepository.GetAllAsync(request.Page, request.PageSize, cancellationToken);
        var totalCount = await comboRepository.CountAsync(cancellationToken);

        var items = combos.Select(combo =>
        {
            var currentOriginalPrice = combo.Items.Sum(i => (i.Product?.Price ?? i.UnitPriceSnapshot) * i.Quantity);
            return new ComboSummaryDto
            {
                Id = combo.Id,
                Name = combo.Name,
                Title = combo.Title,
                ImageUrl = combo.ImageUrl,
                OriginalPrice = combo.OriginalPrice,
                CurrentOriginalPrice = currentOriginalPrice,
                SalePrice = combo.SalePrice,
                IsActive = combo.IsActive,
                StartsAt = combo.StartsAt,
                EndsAt = combo.EndsAt,
                IsCurrentlyActive = combo.IsCurrentlyActive(),
                ItemCount = combo.Items.Count,
                CreatedAt = combo.CreatedAt
            };
        }).ToList();

        var totalPages = (totalCount + request.PageSize - 1) / request.PageSize;

        var result = new GetCombosResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };

        return ApiResponse<GetCombosResult>.Ok(result);
    }
}
