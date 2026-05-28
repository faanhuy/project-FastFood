using SmartShop.Domain.Common;

namespace SmartShop.Domain.Entities;

public class UserAddress : BaseAuditableEntity
{
    private UserAddress() { }

    public Guid UserId { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public string RecipientName { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Street { get; private set; } = string.Empty;
    public bool IsDefault { get; private set; }

    // Structured geography FKs (Sprint 18B)
    public int? ProvinceId { get; private set; }
    public int? WardId { get; private set; }
    public Province? Province { get; private set; }
    public Ward? WardEntity { get; private set; }

    public static UserAddress Create(
        Guid userId,
        string label,
        string recipientName,
        string phone,
        string street,
        int? provinceId = null,
        int? wardId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientName);
        ArgumentException.ThrowIfNullOrWhiteSpace(phone);
        ArgumentException.ThrowIfNullOrWhiteSpace(street);

        return new UserAddress
        {
            UserId = userId,
            Label = label,
            RecipientName = recipientName,
            Phone = phone,
            Street = street,
            IsDefault = false,
            ProvinceId = provinceId,
            WardId = wardId
        };
    }

    public void Update(
        string label,
        string recipientName,
        string phone,
        string street,
        int? provinceId = null,
        int? wardId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientName);
        ArgumentException.ThrowIfNullOrWhiteSpace(phone);
        ArgumentException.ThrowIfNullOrWhiteSpace(street);

        Label = label;
        RecipientName = recipientName;
        Phone = phone;
        Street = street;
        ProvinceId = provinceId;
        WardId = wardId;
    }

    public void SetAsDefault() => IsDefault = true;

    public void UnsetDefault() => IsDefault = false;
}
