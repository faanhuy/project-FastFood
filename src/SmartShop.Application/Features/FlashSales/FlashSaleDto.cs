namespace SmartShop.Application.Features.FlashSales;

public class FlashSaleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public bool IsActive { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectedReason { get; set; }
    public int RemainingSeconds { get; set; }
    public List<FlashSaleItemDto> Items { get; set; } = [];
}
