using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Features.Loyalty.Dtos;

namespace SmartShop.Application.Features.Loyalty.Queries.GetLoyaltyAccount;

public class GetLoyaltyAccountQueryHandler(
    ILoyaltyService loyaltyService) : IRequestHandler<GetLoyaltyAccountQuery, LoyaltyAccountDto>
{
    public async Task<LoyaltyAccountDto> Handle(
        GetLoyaltyAccountQuery request, CancellationToken cancellationToken)
    {
        var account = await loyaltyService.GetOrCreateAccountAsync(request.UserId, cancellationToken);

        var (tierName, nextTier, nextTierPoints) = GetTierInfo(account.Tier, account.LifetimePoints);

        return new LoyaltyAccountDto
        {
            Id = account.Id,
            UserId = account.UserId,
            TotalPoints = account.TotalPoints,
            LifetimePoints = account.LifetimePoints,
            Tier = account.Tier,
            TierName = tierName,
            NextTier = nextTier,
            NextTierPoints = nextTierPoints,
            PointsValueVnd = account.TotalPoints * 10,
            CreatedAt = account.CreatedAt
        };
    }

    private static (string Name, int NextTier, decimal NextPoints) GetTierInfo(int tier, decimal lifetime)
    {
        return tier switch
        {
            0 => ("Bronze", 1, 1000 - lifetime),
            1 => ("Silver", 2, 5000 - lifetime),
            2 => ("Gold", 3, 20000 - lifetime),
            3 => ("Platinum", -1, 0),
            _ => ("Unknown", -1, 0)
        };
    }
}
