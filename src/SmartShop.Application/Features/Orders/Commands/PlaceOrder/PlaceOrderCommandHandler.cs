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
    IUnitOfWork unitOfWork,
    IMediator mediator) : IRequestHandler<PlaceOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByUserIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("Cart", request.UserId);

        if (!cart.Items.Any())
            throw new ConflictException("Giỏ hàng đang trống.");

        var store = await storeRepository.GetByIdAsync(request.StoreId, cancellationToken)
            ?? throw new NotFoundException(nameof(Store), request.StoreId);

        if (!store.IsActive)
            throw new ConflictException("Chi nhánh đã tạm ngừng hoạt động.");

        var productItems = cart.Items.Where(i => i.ItemType == CartItemType.Product).ToList();
        var comboItems   = cart.Items.Where(i => i.ItemType == CartItemType.Combo).ToList();

        // ── Load products for product-type items ──────────────────────────────
        var products = new Dictionary<Guid, Product>();
        foreach (var item in productItems)
        {
            var product = await productRepository.GetByIdAsync(item.ProductId!.Value, cancellationToken)
                ?? throw new NotFoundException("Product", item.ProductId!.Value);

            if (!product.IsActive)
                throw new ConflictException($"Sản phẩm '{product.Name}' không còn bán.");

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

        var wardName     = address.WardEntity?.Name ?? address.Ward;
        var provinceName = address.Province?.Name ?? address.City;

        var shippingAddress = string.Join(", ", new[]
        {
            address.RecipientName, address.Phone, address.Street, wardName, provinceName
        }.Where(s => !string.IsNullOrWhiteSpace(s)));

        // ── Create Order ──────────────────────────────────────────────────────
        var order = Order.Create(
            request.UserId, shippingAddress, request.Notes,
            shippingStreet: address.Street,
            shippingWardId: address.WardId,
            shippingProvinceId: address.ProvinceId,
            shippingAddressId: address.Id);
        order.SetStoreId(request.StoreId);
        order.SetPaymentMethod(request.PaymentMethod);

        // ── Add product items with price campaign ─────────────────────────────
        var priceKeys = productItems.Select(i => (i.ProductId!.Value, i.SizeId)).ToList();
        var effectivePriceRules = await priceCampaignRepository.GetEffectivePriceItemsAsync(
            request.StoreId, priceKeys, DateTime.UtcNow, cancellationToken);

        foreach (var item in productItems)
        {
            var product   = products[item.ProductId!.Value];
            var basePrice = product.Price;
            var key       = (item.ProductId!.Value, item.SizeId);

            decimal unitPrice = basePrice;
            decimal? originalUnitPrice = null;

            if (effectivePriceRules.TryGetValue(key, out var rule))
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
                throw new ConflictException($"Coupon '{request.CouponCode}' đã hết hạn.");

            if (!coupon.HasRemaining())
                throw new ConflictException($"Coupon '{request.CouponCode}' đã hết lượt sử dụng.");

            if (!coupon.MeetsMinOrderValue(order.TotalAmount))
                throw new ConflictException($"Đơn hàng chưa đạt giá trị tối thiểu để dùng coupon.");

            var alreadyUsed = await couponRepository.HasUsageByUserAsync(coupon.Id, request.UserId, cancellationToken);
            if (alreadyUsed)
                throw new ConflictException($"Bạn đã sử dụng coupon '{request.CouponCode}' trước đó.");

            order.ApplyCoupon(coupon.Code, coupon.CalculateDiscount(order.TotalAmount));
            coupon.Use();

            var couponUsage = CouponUsage.Create(request.UserId, order.Id, coupon.Id);
            await couponUsageRepository.AddAsync(couponUsage, cancellationToken);
            couponRepository.Update(coupon);
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
            catch (ConcurrencyException) { throw new ConflictException("Sản phẩm vừa hết hàng, vui lòng thử lại."); }
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
            ShippingAddress = order.ShippingAddress,
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
                    throw new ConflictException($"Sản phẩm '{productName}' size {item.SizeLabel} không có trong kho chi nhánh này.");
                if (item.Quantity > sizeInv.Quantity)
                    throw new ConflictException($"Sản phẩm '{productName}' size {item.SizeLabel} chỉ còn {sizeInv.Quantity} trong kho.");
            }
            else
            {
                if (!inventories.TryGetValue(pid, out var inv))
                    throw new ConflictException($"Sản phẩm '{productName}' không có trong kho chi nhánh này.");
                if (item.Quantity > inv.Quantity)
                    throw new ConflictException($"Sản phẩm '{productName}' chỉ còn {inv.Quantity} trong kho.");
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
                        throw new ConflictException($"Món '{c.ProductName}' size {c.SizeLabel} trong combo không có trong kho.");
                    if (c.TotalQuantity > sizeInv.Quantity)
                        throw new ConflictException($"Món '{c.ProductName}' size {c.SizeLabel} trong combo chỉ còn {sizeInv.Quantity} trong kho.");
                }
                else
                {
                    if (!inventories.TryGetValue(c.ProductId, out var inv))
                        throw new ConflictException($"Món '{c.ProductName}' trong combo không có trong kho chi nhánh này.");
                    if (c.TotalQuantity > inv.Quantity)
                        throw new ConflictException($"Món '{c.ProductName}' trong combo chỉ còn {inv.Quantity} trong kho.");
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
                    throw new ConflictException("Sản phẩm chưa có tồn tổng tại chi nhánh này.");
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
