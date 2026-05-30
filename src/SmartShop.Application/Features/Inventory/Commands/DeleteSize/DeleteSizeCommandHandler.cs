using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Inventory.Commands.DeleteSize;

public class DeleteSizeCommandHandler(
    ISizeRepository repo,
    IUnitOfWork uow,
    ILocalizationService localization,
    ICurrentLanguageService languageService)
    : IRequestHandler<DeleteSizeCommand, ApiResponse<object>>
{
    public async Task<ApiResponse<object>> Handle(DeleteSizeCommand request, CancellationToken ct)
    {
        var size = await repo.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Size), request.Id);

        if (await repo.IsReferencedAsync(request.Id, ct))
            throw new ConflictException("error.size_in_use", null);

        repo.Remove(size);
        await uow.SaveChangesAsync(ct);

        var lang = languageService.Language;
        return ApiResponse<object>.Ok(new { }, localization.GetMessage("success.size_deactivated", lang));
    }
}
