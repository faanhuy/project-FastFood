using SmartShop.Domain.Common;

namespace SmartShop.Domain.Entities;

public class Store : BaseAuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    // Structured geography FKs (Sprint 18B)
    public int? ProvinceId { get; private set; }
    public int? WardId { get; private set; }
    public string? Street { get; private set; }
    public Province? Province { get; private set; }
    public Ward? Ward { get; private set; }

    private Store() { }

    public static Store Create(
        string name,
        string phone,
        int? provinceId = null,
        int? wardId = null,
        string? street = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(phone);

        return new Store
        {
            Name = name,
            Phone = phone,
            IsActive = true,
            ProvinceId = provinceId,
            WardId = wardId,
            Street = street
        };
    }

    public void Update(
        string name,
        string phone,
        int? provinceId = null,
        int? wardId = null,
        string? street = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(phone);

        Name = name;
        Phone = phone;
        ProvinceId = provinceId;
        WardId = wardId;
        Street = street;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
