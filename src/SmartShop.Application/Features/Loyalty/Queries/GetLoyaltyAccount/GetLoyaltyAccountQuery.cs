using MediatR;
using SmartShop.Application.Features.Loyalty.Dtos;

namespace SmartShop.Application.Features.Loyalty.Queries.GetLoyaltyAccount;

public record GetLoyaltyAccountQuery(Guid UserId) : IRequest<LoyaltyAccountDto>;
