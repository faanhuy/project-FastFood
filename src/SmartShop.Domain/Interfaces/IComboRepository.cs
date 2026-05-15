using SmartShop.Domain.Entities;

namespace SmartShop.Domain.Interfaces;

public interface IComboRepository
{
    Task<Combo?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Combo>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<List<Combo>> GetActiveAsync(DateTime now, CancellationToken ct = default);
    Task AddAsync(Combo combo, CancellationToken ct = default);
    void Update(Combo combo);
    Task<int> CountAsync(CancellationToken ct = default);
}
