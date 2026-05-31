using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Combos.Commands.DeleteCombo;

public class DeleteComboCommandHandler(
    IComboRepository comboRepository,
    IUnitOfWork unitOfWork,
    ILocalizationService localization,
    ICurrentLanguageService languageService
) : IRequestHandler<DeleteComboCommand, ApiResponse<object?>>
{
    public async Task<ApiResponse<object?>> Handle(DeleteComboCommand command, CancellationToken cancellationToken)
    {
        var combo = await comboRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Combo), command.Id);

        combo.Deactivate();

        comboRepository.Update(combo);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<object?>.Ok(null,
            localization.GetMessage("success.combo_deactivated", languageService.Language));
    }
}
