namespace CleanArchitectureBase.Application.Products.Dtos.Response;

public class ProductOptionDto
{
    public Guid OptionId { get; set; }
    public string? Name { get; set; }
    public List<ProductOptionValueDto> Values { get; set; } = new();
}
