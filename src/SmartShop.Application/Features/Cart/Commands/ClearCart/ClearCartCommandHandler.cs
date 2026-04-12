using MediatR;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Cart.Commands.ClearCart;

public class ClearCartCommandHandler(
    ICartRepository cartRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ClearCartCommand>
{
    public async Task Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByUserIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("Cart", request.UserId);

        cart.Clear();
        cartRepository.Update(cart);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
