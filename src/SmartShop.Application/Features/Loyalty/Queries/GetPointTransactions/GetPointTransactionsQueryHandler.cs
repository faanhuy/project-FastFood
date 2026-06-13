using MediatR;
using SmartShop.Application.Features.Loyalty.Dtos;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Loyalty.Queries.GetPointTransactions;

public class GetPointTransactionsQueryHandler(
    ILoyaltyRepository loyaltyRepository) : IRequestHandler<GetPointTransactionsQuery, PagedResult<PointTransactionDto>>
{
    public async Task<PagedResult<PointTransactionDto>> Handle(
        GetPointTransactionsQuery request, CancellationToken cancellationToken)
    {
        var account = await loyaltyRepository.GetByUserIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("LoyaltyAccount", request.UserId);

        var (items, totalCount) = await loyaltyRepository.GetTransactionsByAccountIdAsync(
            account.Id, request.Page, request.PageSize, cancellationToken);

        var dtos = items.Select(t => new PointTransactionDto
        {
            Id = t.Id,
            AccountId = t.AccountId,
            OrderId = t.OrderId,
            Points = t.Points,
            Type = t.Type.ToString(),
            Note = t.Note,
            CreatedAt = t.CreatedAt
        }).ToList();

        return new PagedResult<PointTransactionDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}
