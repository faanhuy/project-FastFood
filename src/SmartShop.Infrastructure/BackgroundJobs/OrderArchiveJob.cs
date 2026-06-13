using System.Text.Json;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Infrastructure.BackgroundJobs;

public class OrderArchiveJob(
    IOrderArchiveRepository archiveRepository,
    IUnitOfWork unitOfWork)
{
    private readonly IOrderArchiveRepository _archiveRepository = archiveRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task ExecuteAsync()
    {
        // Archive orders older than 6 months
        var cutoff = DateTime.UtcNow.AddMonths(-6);
        var orders = await _archiveRepository.GetOrdersToArchiveAsync(cutoff);

        if (orders.Count == 0)
            return;

        foreach (var order in orders)
        {
            // Create snapshot of order
            var snapshot = new
            {
                order.Id,
                order.UserId,
                order.Status,
                order.TotalAmount,
                order.OriginalAmount,
                order.DiscountAmount,
                order.PaymentMethod,
                order.PaymentStatus,
                order.CouponCode,
                order.CreatedAt,
                order.UpdatedAt,
                ItemCount = order.Items.Count,
                order.Notes
            };

            var snapshotJson = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            var archive = OrderArchive.Create(order.Id, snapshotJson);
            await _archiveRepository.AddAsync(archive);

            // Mark order as archived
            order.Archive();
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
