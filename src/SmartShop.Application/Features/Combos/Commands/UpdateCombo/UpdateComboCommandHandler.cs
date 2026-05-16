using MediatR;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Features.Combos.Commands.CreateCombo;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Combos.Commands.UpdateCombo;

public class UpdateComboCommandHandler(
    IComboRepository comboRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateComboCommand, ApiResponse<ComboDto>>
{
    public async Task<ApiResponse<ComboDto>> Handle(UpdateComboCommand command, CancellationToken cancellationToken)
    {
        var combo = await comboRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Combo), command.Id);

        if (command.Items.Count == 0)
            throw new ConflictException("Combo phải có ít nhất 1 sản phẩm.");

        var oldItems = combo.Items.ToList();

        combo.Update(
            command.Name,
            command.Title,
            command.Description,
            command.ImageUrl,
            command.SalePrice,
            command.StartsAt,
            command.EndsAt
        );

        // Diff-based item update: giữ nguyên item nếu (ProductId, SizeId, Quantity) không đổi
        var matchedOldIds = new HashSet<Guid>();
        var itemsToDelete = new List<Domain.Entities.ComboItem>();
        var itemsToAdd = new List<Domain.Entities.ComboItem>();
        var keptItems = new List<Domain.Entities.ComboItem>();

        foreach (var itemRequest in command.Items)
        {
            var product = await productRepository.GetByIdWithSizesAsync(itemRequest.ProductId, cancellationToken)
                ?? throw new NotFoundException(nameof(Product), itemRequest.ProductId);

            if (!product.IsActive)
                throw new ConflictException($"Sản phẩm '{product.Name}' không hoạt động.");

            string? sizeLabel = null;
            if (itemRequest.SizeId.HasValue)
            {
                var productSize = product.Sizes.FirstOrDefault(s => s.Id == itemRequest.SizeId.Value);
                if (productSize == null)
                    throw new NotFoundException("Size", itemRequest.SizeId.Value);
                sizeLabel = productSize.SizeLabel;
            }

            var matchingOld = oldItems.FirstOrDefault(o =>
                o.ProductId == itemRequest.ProductId &&
                o.SizeId == itemRequest.SizeId &&
                !matchedOldIds.Contains(o.Id));

            if (matchingOld != null && matchingOld.Quantity == itemRequest.Quantity)
            {
                // Không thay đổi gì — giữ nguyên
                matchedOldIds.Add(matchingOld.Id);
                keptItems.Add(matchingOld);
            }
            else
            {
                // Quantity thay đổi hoặc item mới — xóa cũ (nếu có), thêm mới
                if (matchingOld != null)
                {
                    matchedOldIds.Add(matchingOld.Id);
                    itemsToDelete.Add(matchingOld);
                }
                itemsToAdd.Add(Domain.Entities.ComboItem.Create(
                    combo.Id, product.Id, product.Name,
                    itemRequest.SizeId, sizeLabel,
                    itemRequest.Quantity, product.Price));
            }
        }

        // Items cũ không còn trong request → xóa
        foreach (var old in oldItems.Where(o => !matchedOldIds.Contains(o.Id)))
            itemsToDelete.Add(old);

        comboRepository.RemoveItems(itemsToDelete);
        comboRepository.AddItems(itemsToAdd);
        combo.ReplaceItems([.. keptItems, .. itemsToAdd]);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = MapToDto(combo);
        return ApiResponse<ComboDto>.Ok(dto);
    }

    private static ComboDto MapToDto(Domain.Entities.Combo combo)
    {
        var items = combo.Items.Select(item => new ComboItemDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            SizeId = item.SizeId,
            SizeLabel = item.SizeLabel,
            Quantity = item.Quantity,
            UnitPriceSnapshot = item.UnitPriceSnapshot,
            CurrentUnitPrice = item.UnitPriceSnapshot
        }).ToList();

        return new ComboDto
        {
            Id = combo.Id,
            Name = combo.Name,
            Title = combo.Title,
            Description = combo.Description,
            ImageUrl = combo.ImageUrl,
            OriginalPrice = combo.OriginalPrice,
            CurrentOriginalPrice = combo.OriginalPrice,
            SalePrice = combo.SalePrice,
            IsActive = combo.IsActive,
            StartsAt = combo.StartsAt,
            EndsAt = combo.EndsAt,
            IsCurrentlyActive = combo.IsCurrentlyActive(),
            Items = items,
            CreatedAt = combo.CreatedAt
        };
    }
}
