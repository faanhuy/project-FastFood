using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.PriceCampaigns.Commands.DeletePriceCampaign;

public class DeletePriceCampaignCommandHandler(
    IPriceCampaignRepository priceCampaignRepo,
    ICacheService cache,
    IUnitOfWork uow,
    ILocalizationService localization,
    ICurrentLanguageService languageService
) : IRequestHandler<DeletePriceCampaignCommand, ApiResponse<object?>>
{
    public async Task<ApiResponse<object?>> Handle(
        DeletePriceCampaignCommand cmd, CancellationToken ct)
    {
        var campaign = await priceCampaignRepo.GetByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException(nameof(PriceCampaign), cmd.Id);

        priceCampaignRepo.Remove(campaign);
        await uow.SaveChangesAsync(ct);

        await cache.RemoveByPrefixAsync("price:effective:", ct);

        return ApiResponse<object?>.Ok(null,
            localization.GetMessage("success.price_campaign_deleted", languageService.Language));
    }
}
