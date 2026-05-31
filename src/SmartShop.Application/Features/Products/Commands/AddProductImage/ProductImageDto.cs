namespace SmartShop.Application.Features.Products.Commands.AddProductImage;

public record ProductImageDto(Guid Id, Guid ProductId, string ImageUrl, bool IsPrimary, int SortOrder);
