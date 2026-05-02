using MediatR;
using SmartShop.Application.Common.Models;

namespace SmartShop.Application.Features.Wishlist.Commands.RemoveFromWishlist;

public record RemoveFromWishlistCommand(Guid ProductId) : IRequest<ApiResponse<bool>>;
