using SmartShop.Domain.Entities;

namespace SmartShop.Domain.Interfaces;

public interface IFaqDocumentRepository
{
    Task<IReadOnlyList<FaqDocument>> GetAllActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<FaqDocument>> GetByCategoryAsync(string category, CancellationToken ct = default);
}
