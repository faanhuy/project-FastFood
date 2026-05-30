using MediatR;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Inventory.Commands.ToggleSizeActive;

public class ToggleSizeActiveCommandHandler(ISizeRepository repo, IUnitOfWork uow)
    : IRequestHandler<ToggleSizeActiveCommand, ApiResponse<SizeDto>>
{
    public async Task<ApiResponse<SizeDto>> Handle(ToggleSizeActiveCommand request, CancellationToken ct)
    {
        var size = await repo.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Size), request.Id);

        if (size.IsActive)
            size.Deactivate();
        else
            size.Activate();

        repo.Update(size);
        await uow.SaveChangesAsync(ct);

        return ApiResponse<SizeDto>.Ok(SizeDto.From(size));
    }
}
