using MediatR;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SmartShop.Domain.Interfaces;
using PdfUnit = QuestPDF.Infrastructure.Unit;

namespace SmartShop.Application.Features.Admin.Analytics.Queries.ExportRevenuePdf;

public class ExportRevenuePdfQueryHandler(IOrderRepository orderRepository) : IRequestHandler<ExportRevenuePdfQuery, byte[]>
{
    public async Task<byte[]> Handle(ExportRevenuePdfQuery request, CancellationToken cancellationToken)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var revenueData = await orderRepository.GetRevenueByDateAsync(request.From, request.To, cancellationToken);
        var dataList = revenueData.ToList();

        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(595.28f, 841.89f, PdfUnit.Point);
                page.Margin(2, PdfUnit.Centimetre);

                // Header
                page.Header().Text("Báo cáo doanh thu SmartShop")
                    .FontSize(20)
                    .Bold()
                    .AlignCenter();

                page.Content().PaddingVertical(1, PdfUnit.Centimetre).Column(column =>
                {
                    // Period info
                    column.Item().Text($"Khoảng thời gian: {request.From:dd/MM/yyyy} - {request.To:dd/MM/yyyy}")
                        .FontSize(11);

                    column.Item().PaddingVertical(0.5f, PdfUnit.Centimetre).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Background("#f0f0f0").Padding(5).Text("Ngày").Bold();
                            header.Cell().Background("#f0f0f0").Padding(5).Text("Số đơn hàng").Bold();
                            header.Cell().Background("#f0f0f0").Padding(5).Text("Doanh thu (đ)").Bold();
                        });

                        // Data rows
                        foreach (var row in dataList)
                        {
                            table.Cell().Padding(5).Text(row.Date.ToString("dd/MM/yyyy"));
                            table.Cell().Padding(5).Text(row.OrderCount.ToString());
                            table.Cell().Padding(5).Text(row.Revenue.ToString("N0"));
                        }
                    });
                });

                // Footer
                page.Footer().AlignCenter().Text(x =>
                {
                    x.DefaultTextStyle(t => t.FontSize(9));
                    x.Span("Trang ");
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();

        return bytes;
    }
}
