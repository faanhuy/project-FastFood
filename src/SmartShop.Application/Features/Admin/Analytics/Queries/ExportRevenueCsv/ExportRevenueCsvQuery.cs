using MediatR;

namespace SmartShop.Application.Features.Admin.Analytics.Queries.ExportRevenueCsv;

public record ExportRevenueCsvQuery(DateTime From, DateTime To) : IRequest<byte[]>;
