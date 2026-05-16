namespace SmartShop.Application.Features.Products.Commands.BulkImportProducts;

public class ProductCsvRow
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? ImageUrl { get; set; }
}
