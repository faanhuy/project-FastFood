using MediatR;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Interfaces;
using CartEntity = SmartShop.Domain.Entities.Cart;

namespace SmartShop.Application.Features.Cart.Commands.AddComboToCart;

public class AddComboToCartCommandHandler(
    ICartRepository cartRepository,
    IComboRepository comboRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<AddComboToCartCommand, CartDto>
{
    public async Task<CartDto> Handle(AddComboToCartCommand request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
            throw new ArgumentException("Số lượng phải lớn hơn 0.");

        var combo = await comboRepository.GetByIdAsync(request.ComboId, cancellationToken)
            ?? throw new NotFoundException("Combo", request.ComboId);

        if (!combo.IsCurrentlyActive())
            throw new ConflictException("error.cart_combo_unavailable", null);

        if (!combo.Items.Any())
            throw new ConflictException("error.cart_combo_no_items", null);

        var cart = await cartRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (cart == null)
        {
            cart = CartEntity.Create(request.UserId);
            var cartItem = CartItem.CreateCombo(cart.Id, combo.Id, combo.Title, combo.ImageUrl,
                request.Quantity, combo.SalePrice);

            foreach (var ci in combo.Items)
            {
                var component = CartItemComponent.Create(
                    cartItem.Id, ci.ProductId, ci.ProductName,
                    ci.SizeId, ci.SizeLabel,
                    ci.Quantity, request.Quantity, ci.UnitPriceSnapshot);
                cartItem.AddComponent(component);
            }

            cart.AddComboItem(combo.Id, combo.Title, combo.ImageUrl,
                request.Quantity, combo.SalePrice, cartItem.Components);
            await cartRepository.AddAsync(cart, cancellationToken);
        }
        else
        {
            var isNewItem = !cart.Items.Any(i =>
                i.ItemType == CartItemType.Combo && i.ComboId == combo.Id);

            if (isNewItem)
            {
                var cartItem = CartItem.CreateCombo(cart.Id, combo.Id, combo.Title, combo.ImageUrl,
                    request.Quantity, combo.SalePrice);

                var components = combo.Items.Select(ci => CartItemComponent.Create(
                    cartItem.Id, ci.ProductId, ci.ProductName,
                    ci.SizeId, ci.SizeLabel,
                    ci.Quantity, request.Quantity, ci.UnitPriceSnapshot)).ToList();

                foreach (var c in components)
                    cartItem.AddComponent(c);

                cart.AddComboItem(combo.Id, combo.Title, combo.ImageUrl,
                    request.Quantity, combo.SalePrice, cartItem.Components);

                var addedItem = cart.Items.First(i => i.ItemType == CartItemType.Combo && i.ComboId == combo.Id);
                await cartRepository.AddCartItemAsync(addedItem, cancellationToken);
                foreach (var c in addedItem.Components)
                    await cartRepository.AddCartItemComponentAsync(c, cancellationToken);
            }
            else
            {
                cart.AddComboItem(combo.Id, combo.Title, combo.ImageUrl,
                    request.Quantity, combo.SalePrice, []);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedCart = await cartRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        return CartMapper.ToDto(updatedCart!);
    }
}
