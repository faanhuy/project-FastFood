using MediatR;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Enums;
using SmartShop.Domain.Events;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Orders.Commands.PlaceOrder;

public class PlaceOrderCommandHandler(
    ICartRepository cartRepository,
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    IStoreRepository storeRepository,
    IStoreInventoryRepository storeInventoryRepository,
    IStoreSizeInventoryRepository storeSizeInventoryRepository,
    ICouponRepository couponRepository,
    ICouponUsageRepository couponUsageRepository,
    IUserRepository userRepository,
    IUserAddressRepository userAddressRepository,
    IPriceCampaignRepository priceCampaignRepository,
    IFlashSaleRepository flashSaleRepository,
    IOrderFlashSaleUsageRepository orderFlashSaleUsageRepository,
    ILoyaltyRepository loyaltyRepository,
    IUnitOfWork unitOfWork,
    IMediator mediator) : IRequestHandler<PlaceOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByUserIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("Cart", request.UserId);

        if (!cart.Items.Any())
            throw new ConflictException("error.order_cart_empty", null);

        var store = await storeRepository.GetByIdAsync(request.StoreId, cancellationToken)
            ?? throw new NotFoundException(nameof(Store), request.StoreId);

        if (!store.IsActive)
            throw new ConflictException("error.order_store_inactive", null);

        var productItems = cart.Items.Where(i => i.ItemType == CartItemType.Product).ToList();
        var comboItems   = cart.Items.Where(i => i.ItemType == CartItemType.Combo).ToList();

        // ── Load products for product-type items ──────────────────────────────
        var products = new Dictionary<Guid, Product>();
        foreach (var item in productItems)
        {
            var product = await productRepository.GetByIdAsync(item.ProductId!.Value, cancellationToken)
                ?? throw new NotFoundException("Product", item.ProductId!.Value);

            if (!product.IsActive)
                throw new ConflictException("error.order_product_discontinued", new Dictionary<string, string> { ["name"] = product.Name });

            products[item.ProductId!.Value] = product;
        }

        // ── Build inventory keys ──────────────────────────────────────────────
        // Product items: direct ProductId + SizeId
        var directProductIds = productItems.Select(i => i.ProductId!.Value).Distinct().ToList();
        var directSizedItems = productItems.Where(i => i.SizeId.HasValue).ToList();
        var directSizeIds    = directSizedItems.Select(i => i.SizeId!.Value).ToList();

        // Combo components: collect ProductId + SizeId from CartItemComponents
        var allComponents = comboItems.SelectMany(ci => ci.Components).ToList();
        var comboProductIds = allComponents.Select(c => c.ProductId).Distinct().ToList();
        var comboSizeIds    = allComponents.Where(c => c.SizeId.HasValue)
                                           .Select(c => c.SizeId!.Value).Distinct().ToList();

        var allProductIds = directProductIds.Concat(comboProductIds).Distinct().ToList();
        var allSizeIds    = directSizeIds.Concat(comboSizeIds).Distinct().ToList();

        // ── Load inventories ──────────────────────────────────────────────────
        var inventories = allProductIds.Count > 0
            ? (await storeInventoryRepository.GetByStoreAndProductsAsync(
                request.StoreId, allProductIds, cancellationToken))
                .ToDictionary(i => i.ProductId)
            : new Dictionary<Guid, StoreInventory>();

        var sizeInventories = allSizeIds.Count > 0
            ? (await storeSizeInventoryRepository.GetByStoreAndSizesAsync(
                request.StoreId, allSizeIds, cancellationToken))
                .ToDictionary(i => i.SizeId)
            : new Dictionary<Guid, StoreSizeInventory>();

        // ── Phase 1: Validate stock (read-only) ───────────────────────────────
        ValidateProductStock(productItems, products, inventories, sizeInventories);
        ValidateComboStock(comboItems, inventories, sizeInventories);

        // ── Build shipping address ────────────────────────────────────────────
        var address = await userAddressRepository.GetByIdAsync(request.AddressId, cancellationToken)
            ?? throw new NotFoundException("Address", request.AddressId);

        var wardName     = address.WardEntity?.Name;
        var provinceName = address.Province?.Name;

        // ── Create Order ──────────────────────────────────────────────────────
        var order = Order.Create(
            request.UserId, request.Notes,
            shippingStreet: address.Street,
            shippingWardId: address.WardId,
            shippingProvinceId: address.ProvinceId,
            shippingAddressId: address.Id);
        order.SetStoreId(request.StoreId);
        order.SetPaymentMethod(request.PaymentMethod);

        // ── Add product items with price campaign and flash sales ─────────────
        var priceKeys = productItems.Select(i => (i.ProductId!.Value, i.SizeId)).ToList();
        var effectivePriceRules = await priceCampaignRepository.GetEffectivePriceItemsAsync(
            request.StoreId, priceKeys, DateTime.UtcNow, cancellationToken);

        var now = DateTime.UtcNow;
        var productIds = productItems.Select(i => i.ProductId!.Value).Distinct().ToList();
        var activeFlashSalesByProduct = new Dictionary<Guid, FlashSale>();
        foreach (var productId in productIds)
        {
            var flashSale = await flashSaleRepository.GetActiveByProductIdAsync(productId, now, cancellationToken);
            if (flashSale != null)
                activeFlashSalesByProduct[productId] = flashSale;
        }

        foreach (var item in productItems)
        {
            var product   = products[item.ProductId!.Value];
            var basePrice = product.Price;
            var key       = (item.ProductId!.Value, item.SizeId);

            decimal unitPrice = basePrice;
            decimal? originalUnitPrice = null;

            // Check flash sale first (highest priority) - find matching item
            if (activeFlashSalesByProduct.TryGetValue(item.ProductId!.Value, out var flashSale))
            {
                var matchingFlashSaleItem = flashSale.Items.FirstOrDefault(
                    fsi => fsi.ProductId == item.ProductId && fsi.SizeId == item.SizeId);

                if (matchingFlashSaleItem != null)
                {
                    if (!matchingFlashSaleItem.HasStock(item.Quantity))
                        throw new ConflictException("error.order_flash_sale_insufficient_stock",
                            new Dictionary<string, string>
                            {
                                ["name"] = products[item.ProductId!.Value].Name,
                                ["remaining"] = matchingFlashSaleItem.GetRemainingStock().ToString()
                            });

                    unitPrice = matchingFlashSaleItem.SalePrice;
                    originalUnitPrice = basePrice;
                    matchingFlashSaleItem.IncrementSoldCount(item.Quantity);
                    flashSaleRepository.Update(flashSale);

                    var usage = OrderFlashSaleUsage.Create(
                        order.Id, flashSale.Id, matchingFlashSaleItem.Id,
                        item.ProductId!.Value, item.SizeId,
                        item.Quantity, matchingFlashSaleItem.SalePrice, basePrice);
                    await orderFlashSaleUsageRepository.AddAsync(usage, cancellationToken);
                }
            }
            // Then check price campaign
            if (unitPrice == basePrice && effectivePriceRules.TryGetValue(key, out var rule))
            {
                var promoPrice = (PriceRuleType)rule.ruleType switch
                {
                    PriceRuleType.Coefficient => basePrice * rule.discountValue,
                    PriceRuleType.FixedPrice  => rule.discountValue,
                    _                         => basePrice
                };
                unitPrice = promoPrice;
                originalUnitPrice = basePrice;
            }

            var orderItem = OrderItem.Create(
                order.Id, item.ProductId!.Value, product.Name, item.Quantity, unitPrice,
                sizeId: item.SizeId, sizeLabel: item.SizeLabel,
                originalUnitPrice: originalUnitPrice ?? basePrice,
                imageUrl: product.ImageUrl);
            order.AddItem(orderItem);
        }

        // ── Add combo items ───────────────────────────────────────────────────
        foreach (var cartItem in comboItems)
        {
            var orderItem = OrderItem.CreateCombo(
                order.Id, cartItem.ComboId!.Value, cartItem.DisplayName, cartItem.ImageUrl,
                cartItem.Quantity, cartItem.UnitPrice,
                originalUnitPrice: cartItem.UnitPrice);

            foreach (var c in cartItem.Components)
            {
                var component = OrderItemComponent.Create(
                    orderItem.Id,
                    c.ProductId, c.ProductName, productImageUrl: null,
                    c.SizeId, c.SizeLabel,
                    c.QuantityPerCombo, cartItem.Quantity, c.UnitPriceSnapshot);
                orderItem.AddComponent(component);
            }

            order.AddItem(orderItem);
        }

        // ── Phase 2: Deduct stock ─────────────────────────────────────────────
        DeductProductStock(productItems, inventories, sizeInventories);
        DeductComboStock(comboItems, inventories, sizeInventories);

        // ── Coupon ────────────────────────────────────────────────────────────
        if (!string.IsNullOrEmpty(request.CouponCode))
        {
            var coupon = await couponRepository.GetByCodeAsync(request.CouponCode, cancellationToken)
                ?? throw new NotFoundException("Coupon", request.CouponCode);

            if (coupon.IsExpired())
                throw new ConflictException("error.order_coupon_expired", new Dictionary<string, string> { ["code"] = request.CouponCode! });

            if (!coupon.HasRemaining())
                throw new ConflictException("error.order_coupon_used_up", new Dictionary<string, string> { ["code"] = request.CouponCode! });

            if (!coupon.MeetsMinOrderValue(order.TotalAmount))
                throw new ConflictException("error.order_coupon_min_value", null);

            var alreadyUsed = await couponRepository.HasUsageByUserAsync(coupon.Id, request.UserId, cancellationToken);
            if (alreadyUsed)
                throw new ConflictException("error.order_coupon_already_used", new Dictionary<string, string> { ["code"] = request.CouponCode! });

            order.ApplyCoupon(coupon.Code, coupon.CalculateDiscount(order.TotalAmount));
            coupon.Use();

            var couponUsage = CouponUsage.Create(request.UserId, order.Id, coupon.Id);
            await couponUsageRepository.AddAsync(couponUsage, cancellationToken);
            couponRepository.Update(coupon);
        }

        // ── Loyalty Points ────────────────────────────────────────────────
        decimal loyaltyDiscount = 0;
        decimal loyaltyPointsUsed = 0;
        if (request.UsePoints > 0)
        {
            var loyaltyAccount = await loyaltyRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            if (loyaltyAccount != null)
            {
                // Validate: points available
                if (request.UsePoints > loyaltyAccount.TotalPoints)
                    throw new ConflictException("error.order_insufficient_loyalty_points", null);

                // Validate: max 30% of current order value
                var maxRedeemVnd = order.TotalAmount * 0.30m;
                var pointValueVnd = request.UsePoints * 10m;  // 1 point = 10 VND
                if (pointValueVnd > maxRedeemVnd)
                    throw new ConflictException("error.order_loyalty_exceed_max_redeem",
                        new Dictionary<string, string> { ["max"] = maxRedeemVnd.ToString("C0") });

                loyaltyDiscount = pointValueVnd;
                loyaltyPointsUsed = request.UsePoints;
            }
        }

        // Apply loyalty discount to order
        if (loyaltyDiscount > 0)
        {
            order.ApplyLoyaltyDiscount(loyaltyDiscount, loyaltyPointsUsed);
        }

        await orderRepository.AddAsync(order, cancellationToken);
        cart.Clear();

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyException)
        {
            // Retry once
            var freshInventories = allProductIds.Count > 0
                ? (await storeInventoryRepository.GetByStoreAndProductsAsync(
                    request.StoreId, allProductIds, cancellationToken))
                    .ToDictionary(i => i.ProductId)
                : new Dictionary<Guid, StoreInventory>();

            var freshSizeInventories = allSizeIds.Count > 0
                ? (await storeSizeInventoryRepository.GetByStoreAndSizesAsync(
                    request.StoreId, allSizeIds, cancellationToken))
                    .ToDictionary(i => i.SizeId)
                : new Dictionary<Guid, StoreSizeInventory>();

            ValidateProductStock(productItems, products, freshInventories, freshSizeInventories);
            ValidateComboStock(comboItems, freshInventories, freshSizeInventories);
            DeductProductStock(productItems, freshInventories, freshSizeInventories);
            DeductComboStock(comboItems, freshInventories, freshSizeInventories);

            try { await unitOfWork.SaveChangesAsync(cancellationToken); }
            catch (ConcurrencyException) { throw new ConflictException("error.order_out_of_stock_race", null); }
        }

        // ── Deduct loyalty points if redeemed ──────────────────────────────
        if (loyaltyPointsUsed > 0)
        {
            var loyaltyAccount = await loyaltyRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            if (loyaltyAccount != null)
            {
                loyaltyAccount.RedeemPoints(loyaltyPointsUsed, order.Id, $"Order {order.Id:N}");
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is not null)
        {
            var eventItems = order.Items.Select(i =>
                new OrderEventItemDto(i.ProductName, i.Quantity, i.UnitPrice)).ToList();

            await mediator.Publish(new OrderPlacedEvent(
                OrderId: order.Id,
                UserId: user.Id.ToString(),
                UserEmail: user.Email,
                UserName: $"{user.FirstName} {user.LastName}".Trim(),
                TotalPrice: order.TotalAmount,
                Items: eventItems), cancellationToken);
        }

        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            OriginalAmount = order.OriginalAmount,
            DiscountAmount = order.DiscountAmount,
            ShippingAddress = string.Join(", ", new[] { order.ShippingStreet, wardName, provinceName }.Where(s => !string.IsNullOrWhiteSpace(s))),
            ShippingAddressId = order.ShippingAddressId,
            ShippingStreet = order.ShippingStreet,
            ShippingWardId = order.ShippingWardId,
            ShippingProvinceId = order.ShippingProvinceId,
            ShippingWardName = wardName,
            ShippingProvinceName = provinceName,
            CouponCode = order.CouponCode,
            Notes = order.Notes,
            PaymentMethod = order.PaymentMethod.ToString(),
            PaymentStatus = order.PaymentStatus.ToString(),
            PaidAt = order.PaidAt,
            VnpayTransactionId = order.VnpayTransactionId,
            Items = order.Items.Select(OrderMapper.ToDto).ToList(),
            CreatedAt = order.CreatedAt
        };
    }

    private static void ValidateProductStock(
        IEnumerable<CartItem> items,
        Dictionary<Guid, Product> products,
        Dictionary<Guid, StoreInventory> inventories,
        Dictionary<Guid, StoreSizeInventory> sizeInventories)
    {
        foreach (var item in items)
        {
            var pid = item.ProductId!.Value;
            var productName = products.TryGetValue(pid, out var p) ? p.Name : pid.ToString();

            if (item.SizeId.HasValue)
            {
                if (!sizeInventories.TryGetValue(item.SizeId.Value, out var sizeInv))
                    throw new ConflictException("error.order_inventory_no_size",
                        new Dictionary<string, string> { ["name"] = productName, ["size"] = item.SizeLabel ?? "" });
                if (item.Quantity > sizeInv.Quantity)
                    throw new ConflictException("error.order_inventory_insufficient_size",
                        new Dictionary<string, string> { ["name"] = productName, ["size"] = item.SizeLabel ?? "", ["qty"] = sizeInv.Quantity.ToString() });
            }
            else
            {
                if (!inventories.TryGetValue(pid, out var inv))
                    throw new ConflictException("error.order_inventory_no_stock",
                        new Dictionary<string, string> { ["name"] = productName });
                if (item.Quantity > inv.Quantity)
                    throw new ConflictException("error.order_inventory_insufficient",
                        new Dictionary<string, string> { ["name"] = productName, ["qty"] = inv.Quantity.ToString() });
            }
        }
    }

    private static void ValidateComboStock(
        IEnumerable<CartItem> comboItems,
        Dictionary<Guid, StoreInventory> inventories,
        Dictionary<Guid, StoreSizeInventory> sizeInventories)
    {
        foreach (var comboItem in comboItems)
        {
            foreach (var c in comboItem.Components)
            {
                if (c.SizeId.HasValue)
                {
                    if (!sizeInventories.TryGetValue(c.SizeId.Value, out var sizeInv))
                        throw new ConflictException("error.order_combo_no_size",
                            new Dictionary<string, string> { ["name"] = c.ProductName, ["size"] = c.SizeLabel ?? "" });
                    if (c.TotalQuantity > sizeInv.Quantity)
                        throw new ConflictException("error.order_combo_insufficient_size",
                            new Dictionary<string, string> { ["name"] = c.ProductName, ["size"] = c.SizeLabel ?? "", ["qty"] = sizeInv.Quantity.ToString() });
                }
                else
                {
                    if (!inventories.TryGetValue(c.ProductId, out var inv))
                        throw new ConflictException("error.order_combo_no_stock",
                            new Dictionary<string, string> { ["name"] = c.ProductName });
                    if (c.TotalQuantity > inv.Quantity)
                        throw new ConflictException("error.order_combo_insufficient",
                            new Dictionary<string, string> { ["name"] = c.ProductName, ["qty"] = inv.Quantity.ToString() });
                }
            }
        }
    }

    private static void DeductProductStock(
        IEnumerable<CartItem> items,
        Dictionary<Guid, StoreInventory> inventories,
        Dictionary<Guid, StoreSizeInventory> sizeInventories)
    {
        foreach (var item in items.OrderBy(i => i.SizeId.HasValue ? i.SizeId.ToString() : i.ProductId.ToString()))
        {
            var pid = item.ProductId!.Value;
            if (item.SizeId.HasValue)
            {
                sizeInventories[item.SizeId.Value].DeductStock(item.Quantity);
                if (!inventories.TryGetValue(pid, out var inv))
                    throw new ConflictException("error.order_no_total_inventory", null);
                inv.DeductStock(item.Quantity);
            }
            else
            {
                inventories[pid].DeductStock(item.Quantity);
            }
        }
    }

    private static void DeductComboStock(
        IEnumerable<CartItem> comboItems,
        Dictionary<Guid, StoreInventory> inventories,
        Dictionary<Guid, StoreSizeInventory> sizeInventories)
    {
        foreach (var comboItem in comboItems)
        {
            foreach (var c in comboItem.Components.OrderBy(c => c.SizeId.HasValue ? c.SizeId.ToString() : c.ProductId.ToString()))
            {
                if (c.SizeId.HasValue)
                {
                    sizeInventories[c.SizeId.Value].DeductStock(c.TotalQuantity);
                    if (inventories.TryGetValue(c.ProductId, out var inv))
                        inv.DeductStock(c.TotalQuantity);
                }
                else
                {
                    inventories[c.ProductId].DeductStock(c.TotalQuantity);
                }
            }
        }
    }
}
