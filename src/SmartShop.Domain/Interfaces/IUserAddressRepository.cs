using SmartShop.Domain.Entities;

namespace SmartShop.Domain.Interfaces;

public interface IUserAddressRepository
{
    Task<IReadOnlyList<UserAddress>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<UserAddress?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserAddress?> GetDefaultByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(UserAddress address, CancellationToken ct = default);
    void Update(UserAddress address);
    void Remove(UserAddress address);
}
