using MediatR;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Cart.Queries.GetCart;

public class GetCartQueryHandler(ICartRepository cartRepository) : IRequestHandler<GetCartQuery, CartDto>
{
    public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (cart == null)
        {
            return new CartDto
            {
                UserId = request.UserId,
                Items = [],
                TotalAmount = 0
            };
        }

        return CartMapper.ToDto(cart);
    }
}
