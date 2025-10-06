namespace CleanArchitectureBase.Application.Products.Dtos.Response;

public class ProductVariantDto
{
    public Guid VariantId { get; set; }
    public string? Sku { get; set; }
    public long Price { get; set; }
    public int Stock { get; set; }
    public Dictionary<string, string> OptionValues { get; set; } = new();
}
