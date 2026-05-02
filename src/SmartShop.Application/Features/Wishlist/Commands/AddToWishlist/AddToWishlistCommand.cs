using MediatR;
using SmartShop.Application.Common.Models;

namespace SmartShop.Application.Features.Wishlist.Commands.AddToWishlist;

public record AddToWishlistCommand(Guid ProductId) : IRequest<ApiResponse<bool>>;
