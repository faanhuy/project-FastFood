using MediatR;
using SmartShop.Application.Common.Models;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Catalog.Queries.GetCatalog;

public class GetCatalogQueryHandler(
    IProductRepository productRepository,
    IComboRepository comboRepository
) : IRequestHandler<GetCatalogQuery, ApiResponse<GetCatalogResult>>
{
    public async Task<ApiResponse<GetCatalogResult>> Handle(GetCatalogQuery request, CancellationToken ct)
    {
        // Fetch active products (paginated)
        var (products, totalProducts) = await productRepository.GetPagedAsync(
            request.Page, request.PageSize, null, null, "newest", ct);

        var activeProducts = products.Where(p => p.IsActive).ToList();

        // Fetch active combos (not paginated - fetch all for now)
        var activeCombos = await comboRepository.GetActiveAsync(DateTime.UtcNow, ct);

        // Map products to CatalogItemDto
        var productDtos = activeProducts.Select(p => new CatalogItemDto
        {
            Id = p.Id,
            ItemType = "Product",
            Name = p.Name,
            Slug = p.Slug,
            Description = p.Description,
            ImageUrl = p.ImageUrl,
            Price = p.Price,
            OriginalPrice = p.OriginalPrice > p.Price ? p.OriginalPrice : null,
            DiscountPercent = null,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name,
            HasSizes = p.HasSizes,
            ComboItemCount = null,
            StartsAt = null,
            EndsAt = null,
            CreatedAt = p.CreatedAt
        }).ToList();

        // Map combos to CatalogItemDto
        var comboDtos = activeCombos.Select(c =>
        {
            var currentOriginalPrice = c.Items.Sum(i => (i.Product?.Price ?? i.UnitPriceSnapshot) * i.Quantity);
            return new CatalogItemDto
            {
                Id = c.Id,
                ItemType = "Combo",
                Name = c.Title,
                Slug = string.Empty,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                Price = c.SalePrice,
                OriginalPrice = currentOriginalPrice > c.SalePrice ? currentOriginalPrice : null,
                DiscountPercent = currentOriginalPrice > c.SalePrice
                    ? Math.Round((currentOriginalPrice - c.SalePrice) / currentOriginalPrice * 100, 1)
                    : null,
                CategoryId = null,
                CategoryName = null,
                HasSizes = false,
                ComboItemCount = c.Items.Count,
                StartsAt = c.StartsAt,
                EndsAt = c.EndsAt,
                CreatedAt = c.CreatedAt
            };
        }).ToList();

        var result = new GetCatalogResult
        {
            Products = productDtos,
            Combos = comboDtos,
            TotalProducts = totalProducts,
            TotalCombos = activeCombos.Count
        };

        return ApiResponse<GetCatalogResult>.Ok(result);
    }
}
