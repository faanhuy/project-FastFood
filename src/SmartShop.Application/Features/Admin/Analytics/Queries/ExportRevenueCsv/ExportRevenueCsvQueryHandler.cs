using System.Text;
using MediatR;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Admin.Analytics.Queries.ExportRevenueCsv;

public class ExportRevenueCsvQueryHandler(IOrderRepository orderRepository) : IRequestHandler<ExportRevenueCsvQuery, byte[]>
{
    public async Task<byte[]> Handle(ExportRevenueCsvQuery request, CancellationToken cancellationToken)
    {
        var revenueData = await orderRepository.GetRevenueByDateAsync(request.From, request.To, cancellationToken);

        var csv = new StringBuilder();

        // Header
        csv.AppendLine("Date,Orders,Revenue (VND)");

        // Data rows
        foreach (var row in revenueData)
        {
            var dateStr = row.Date.ToString("yyyy-MM-dd");
            var ordersStr = row.OrderCount.ToString();
            var revenueStr = row.Revenue.ToString("0");
            csv.AppendLine($"{dateStr},{ordersStr},{revenueStr}");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        // Add UTF-8 BOM
        return Encoding.UTF8.GetPreamble().Concat(bytes).ToArray();
    }
}
