using MediatR;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Cart.Commands.UpdateCartItemById;

public class UpdateCartItemByIdCommandHandler(
    ICartRepository cartRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateCartItemByIdCommand, CartDto>
{
    public async Task<CartDto> Handle(UpdateCartItemByIdCommand request, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByUserIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("Cart", request.UserId);

        cart.UpdateItemQuantityById(request.CartItemId, request.Quantity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedCart = await cartRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        return CartMapper.ToDto(updatedCart!);
    }
}
