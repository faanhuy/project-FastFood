using MediatR;
using SmartShop.Application.Features.Loyalty.Dtos;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Loyalty.Queries.GetPointTransactions;

public record GetPointTransactionsQuery(Guid UserId, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<PointTransactionDto>>;
