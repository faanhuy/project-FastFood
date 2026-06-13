using MediatR;
using SmartShop.Application.Features.AI;

namespace SmartShop.Application.Features.AI.Queries.SemanticSearch;

public record SemanticSearchQuery(string Query, int TopN = 10, Guid? StoreId = null) : IRequest<IReadOnlyList<SemanticSearchResultDto>>;
