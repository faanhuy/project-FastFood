using MediatR;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Combos.Commands.CreateCombo;

public class CreateComboCommandHandler(
    IComboRepository comboRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateComboCommand, ApiResponse<ComboDto>>
{
    public async Task<ApiResponse<ComboDto>> Handle(CreateComboCommand command, CancellationToken cancellationToken)
    {
        if (command.Items.Count == 0)
            throw new ConflictException("Combo phải có ít nhất 1 sản phẩm.");

        // Create combo
        var combo = Domain.Entities.Combo.Create(
            command.Name,
            command.Title,
            command.Description,
            command.ImageUrl,
            command.SalePrice,
            command.StartsAt,
            command.EndsAt
        );

        // Load và validate products, tạo combo items
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

            var comboItem = Domain.Entities.ComboItem.Create(
                combo.Id,
                product.Id,
                product.Name,
                itemRequest.SizeId,
                sizeLabel,
                itemRequest.Quantity,
                product.Price
            );

            combo.AddItem(comboItem);
        }

        await comboRepository.AddAsync(combo, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = MapToDto(combo);
        return ApiResponse<ComboDto>.Ok(dto);
    }

    private static ComboDto MapToDto(Domain.Entities.Combo combo)
    {
        return new ComboDto
        {
            Id = combo.Id,
            Name = combo.Name,
            Title = combo.Title,
            Description = combo.Description,
            ImageUrl = combo.ImageUrl,
            OriginalPrice = combo.OriginalPrice,
            SalePrice = combo.SalePrice,
            IsActive = combo.IsActive,
            StartsAt = combo.StartsAt,
            EndsAt = combo.EndsAt,
            IsCurrentlyActive = combo.IsCurrentlyActive(),
            Items = combo.Items.Select(item => new ComboItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                SizeId = item.SizeId,
                SizeLabel = item.SizeLabel,
                Quantity = item.Quantity,
                UnitPriceSnapshot = item.UnitPriceSnapshot
            }).ToList(),
            CreatedAt = combo.CreatedAt
        };
    }
}
