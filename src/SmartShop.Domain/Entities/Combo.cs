using SmartShop.Domain.Common;
using SmartShop.Domain.Common.Exceptions;

namespace SmartShop.Domain.Entities;

public class Combo : BaseAuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public decimal OriginalPrice { get; private set; }
    public decimal SalePrice { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime StartsAt { get; private set; }
    public DateTime? EndsAt { get; private set; }

    private readonly List<ComboItem> _items = [];
    public IReadOnlyCollection<ComboItem> Items => _items.AsReadOnly();

    private Combo() { }

    public static Combo Create(string name, string title, string? description,
        string imageUrl, decimal salePrice, DateTime startsAt, DateTime? endsAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(imageUrl);

        if (endsAt.HasValue && endsAt <= startsAt)
            throw new ConflictException("validation.date_from_before_to", null);

        return new Combo
        {
            Name = name,
            Title = title,
            Description = description,
            ImageUrl = imageUrl,
            SalePrice = salePrice,
            StartsAt = startsAt,
            EndsAt = endsAt,
            OriginalPrice = 0 // Will be recalculated when items are added
        };
    }

    public void AddItem(ComboItem item)
    {
        _items.Add(item);
        RecalculateOriginalPrice();
    }

    public void RecalculateOriginalPrice()
    {
        OriginalPrice = _items.Sum(i => i.UnitPriceSnapshot * i.Quantity);
    }

    public void Update(string name, string title, string? description,
        string imageUrl, decimal salePrice, DateTime startsAt, DateTime? endsAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(imageUrl);

        if (endsAt.HasValue && endsAt <= startsAt)
            throw new ConflictException("validation.date_from_before_to", null);

        Name = name;
        Title = title;
        Description = description;
        ImageUrl = imageUrl;
        SalePrice = salePrice;
        StartsAt = startsAt;
        EndsAt = endsAt;
    }

    public void ReplaceItems(List<ComboItem> newItems)
    {
        _items.Clear();
        foreach (var item in newItems)
        {
            _items.Add(item);
        }
        RecalculateOriginalPrice();
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    public bool IsCurrentlyActive()
    {
        var now = DateTime.UtcNow;
        return IsActive && StartsAt <= now && (EndsAt == null || EndsAt > now);
    }
}
