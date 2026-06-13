using SmartShop.Domain.Common;
using SmartShop.Domain.Common.Exceptions;

namespace SmartShop.Domain.Entities;

public class PriceCampaign : BaseAuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public DateTime StartsAt { get; private set; }
    public DateTime EndsAt { get; private set; }
    public bool AppliesToAll { get; private set; }
    public bool IsActive { get; private set; }

    // Backing list for store IDs — mapped as join table via EF
    private readonly List<Guid> _storeIds = [];
    public IReadOnlyCollection<Guid> StoreIds => _storeIds.AsReadOnly();

    // EF navigation for items
    public ICollection<PriceCampaignItem> ItemsNav { get; private set; } = new List<PriceCampaignItem>();

    private PriceCampaign() { }

    public static PriceCampaign Create(
        string name, DateTime startsAt, DateTime endsAt,
        bool appliesToAll, IEnumerable<Guid>? storeIds = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (endsAt <= startsAt)
            throw new ConflictException("validation.date_from_before_to", null);

        var campaign = new PriceCampaign
        {
            Name = name,
            StartsAt = startsAt,
            EndsAt = endsAt,
            AppliesToAll = appliesToAll,
            IsActive = true
        };

        if (!appliesToAll && storeIds != null)
            campaign._storeIds.AddRange(storeIds);

        return campaign;
    }

    public void UpdateHeader(string name, DateTime startsAt, DateTime endsAt, bool appliesToAll)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (endsAt <= startsAt)
            throw new ConflictException("validation.date_from_before_to", null);

        Name = name;
        StartsAt = startsAt;
        EndsAt = endsAt;
        AppliesToAll = appliesToAll;
    }

    public void SetStores(IEnumerable<Guid> storeIds)
    {
        _storeIds.Clear();
        _storeIds.AddRange(storeIds);
    }

    public void Deactivate() => IsActive = false;

    public bool IsEffectiveAt(DateTime at) =>
        IsActive && StartsAt <= at && at < EndsAt;

    public bool AppliesToStore(Guid storeId) =>
        AppliesToAll || _storeIds.Contains(storeId);
}
