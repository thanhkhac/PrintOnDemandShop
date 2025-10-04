namespace CleanArchitectureBase.Application.Products.Dtos;

public class ProductOptionRqDto
{
    public string? Name { get; set; }
    public List<string> Values { get; set; } = new();
}
