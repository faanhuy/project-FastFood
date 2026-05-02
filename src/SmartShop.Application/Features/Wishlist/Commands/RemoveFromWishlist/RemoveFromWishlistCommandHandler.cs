using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Wishlist.Commands.RemoveFromWishlist;

public class RemoveFromWishlistCommandHandler(
    IWishlistRepository wishlistRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IRequestHandler<RemoveFromWishlistCommand, ApiResponse<bool>>
{
    public async Task<ApiResponse<bool>> Handle(RemoveFromWishlistCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        var item = await wishlistRepository.GetByUserAndProductAsync(userId, request.ProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(WishlistItem), request.ProductId);

        wishlistRepository.RemoveAsync(item);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Đã xóa khỏi danh sách yêu thích.");
    }
}
