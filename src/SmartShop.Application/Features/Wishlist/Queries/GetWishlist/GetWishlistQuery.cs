using MediatR;
using SmartShop.Application.Common.Models;

namespace SmartShop.Application.Features.Wishlist.Queries.GetWishlist;

public record GetWishlistQuery(Guid? StoreId = null) : IRequest<ApiResponse<List<WishlistItemDto>>>;

public record WishlistItemDto(
    Guid ProductId,
    string ProductName,
    decimal Price,
    decimal? EffectivePrice,
    string? ImageUrl,
    bool IsInStock,
    string Slug);
