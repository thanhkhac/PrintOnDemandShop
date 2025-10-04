namespace CleanArchitectureBase.Application.Products.Dtos;

public class ProductVariantRqDto
{
    public string? Sku { get; set; }
    public long Price { get; set; }
    public int Stock { get; set; }
    public Dictionary<string, string> OptionValues { get; set; } = new();
    public List<VariantImageRqDto> Images { get; set; } = new();
}
