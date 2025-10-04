namespace CleanArchitectureBase.Application.Products.Dtos.RequestDtos;

public class UpsertProductVariantRequestDto
{
    public Guid? Id { get; set; }
    public string? Sku { get; set; }
    public long Price { get; set; }
    public int Stock { get; set; }
    public Dictionary<string, string> OptionValues { get; set; } = new();
}
