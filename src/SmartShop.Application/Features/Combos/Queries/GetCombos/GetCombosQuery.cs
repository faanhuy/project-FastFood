using MediatR;
using SmartShop.Application.Common.Models;

namespace SmartShop.Application.Features.Combos.Queries.GetCombos;

public record GetCombosQuery(int Page = 1, int PageSize = 20) : IRequest<ApiResponse<GetCombosResult>>;

public class GetCombosResult
{
    public List<ComboSummaryDto> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}
