using SmartShop.Application.Interfaces;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Infrastructure.BackgroundJobs;

public class FlashSaleExpiryJob(
    IFlashSaleRepository flashSaleRepository,
    IUnitOfWork unitOfWork)
{
    public async Task ExecuteAsync()
    {
        var now = DateTime.UtcNow;
        var expired = await flashSaleRepository.GetExpiredFlashSalesAsync(now);

        foreach (var sale in expired)
        {
            sale.Deactivate();
            flashSaleRepository.Update(sale);
        }

        if (expired.Count > 0)
            await unitOfWork.SaveChangesAsync();
    }
}
