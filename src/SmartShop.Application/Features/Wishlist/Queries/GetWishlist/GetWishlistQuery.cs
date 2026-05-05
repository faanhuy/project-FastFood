using MediatR;
using SmartShop.Application.Common.Models;

namespace SmartShop.Application.Features.Wishlist.Queries.GetWishlist;

public record GetWishlistQuery : IRequest<ApiResponse<List<WishlistItemDto>>>;

public record WishlistItemDto(
    Guid ProductId,
    string ProductName,
    decimal Price,
    string? ImageUrl,
    bool IsInStock,
    string Slug);
