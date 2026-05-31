using MediatR;
using SmartShop.Application.Products.Queries.GetProducts;

namespace SmartShop.Application.Features.AuditLogs.Queries.GetAuditLogs;

public record GetAuditLogsQuery(
    Guid? UserId = null,
    string? Action = null,
    string? EntityType = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 50
) : IRequest<PagedResult<AuditLogDto>>;
