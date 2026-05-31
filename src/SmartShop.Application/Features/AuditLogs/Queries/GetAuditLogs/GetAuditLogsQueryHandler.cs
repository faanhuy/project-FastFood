using MediatR;
using SmartShop.Application.Products.Queries.GetProducts;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.AuditLogs.Queries.GetAuditLogs;

public class GetAuditLogsQueryHandler(IAuditLogRepository auditLogRepository)
    : IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogDto>>
{
    public async Task<PagedResult<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var pagedLogs = await auditLogRepository.GetPagedAsync(
            request.UserId,
            request.Action,
            request.EntityType,
            request.FromDate,
            request.ToDate,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = pagedLogs.Items.Select(AuditLogDto.From).ToList();

        return new PagedResult<AuditLogDto>(dtos, pagedLogs.TotalCount, pagedLogs.Page, pagedLogs.PageSize);
    }
}
