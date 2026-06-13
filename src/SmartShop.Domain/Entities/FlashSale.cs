using SmartShop.Domain.Common;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Enums;

namespace SmartShop.Domain.Entities;

public class FlashSale : BaseAuditableEntity
{
    private FlashSale() { }

    public string Name { get; private set; } = string.Empty;
    public DateTime StartAt { get; private set; }
    public DateTime EndAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public FlashSaleStatus Status { get; private set; } = FlashSaleStatus.Draft;
    public string? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? RejectedReason { get; private set; }

    private List<FlashSaleItem> _items = [];
    public IReadOnlyCollection<FlashSaleItem> Items => _items.AsReadOnly();

    public static FlashSale Create(string name, DateTime startAt, DateTime endAt, List<FlashSaleItem> items)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (items.Count == 0)
            throw new ConflictException("Flash sale must contain at least one item.");
        if (endAt <= startAt)
            throw new ConflictException("End time must be after start time.");

        var flashSale = new FlashSale
        {
            Name = name,
            StartAt = startAt,
            EndAt = endAt,
            IsActive = false,
            Status = FlashSaleStatus.Draft,
            _items = new List<FlashSaleItem>(items)
        };

        return flashSale;
    }

    public void Update(string name, DateTime startAt, DateTime endAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (endAt <= startAt)
            throw new ConflictException("End time must be after start time.");

        Name = name;
        StartAt = startAt;
        EndAt = endAt;
    }

    public void SubmitForApproval()
    {
        if (Status != FlashSaleStatus.Draft && Status != FlashSaleStatus.Rejected)
            throw new ConflictException("error.flashsale_cannot_submit", null);
        Status = FlashSaleStatus.PendingApproval;
    }

    public void Approve(string approvedBy)
    {
        if (Status != FlashSaleStatus.PendingApproval)
            throw new ConflictException("error.flashsale_cannot_approve", null);
        Status = FlashSaleStatus.Approved;
        IsActive = true;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
    }

    public void Reject(string reason)
    {
        if (Status != FlashSaleStatus.PendingApproval)
            throw new ConflictException("error.flashsale_cannot_reject", null);
        Status = FlashSaleStatus.Rejected;
        IsActive = false;
        RejectedReason = reason;
    }

    public void Deactivate() => IsActive = false;

    public bool IsCurrentlyActive(DateTime now) =>
        IsActive && StartAt <= now && now < EndAt && _items.Any(i => i.HasStock());
}
