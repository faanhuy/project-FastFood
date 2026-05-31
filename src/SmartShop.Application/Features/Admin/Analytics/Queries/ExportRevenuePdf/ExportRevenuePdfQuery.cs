using MediatR;

namespace SmartShop.Application.Features.Admin.Analytics.Queries.ExportRevenuePdf;

public record ExportRevenuePdfQuery(DateTime From, DateTime To) : IRequest<byte[]>;
